using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace unillm
{
    public class UnillmCommonAgentModelConfig
    {
        public string URL { get; } = "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions";
        public string Key { get; } = Environment.GetEnvironmentVariable("QWEN_API_KEY", EnvironmentVariableTarget.User);
        public string Model { get; } = "qwen-plus";
    }

    public sealed class UnillmCommmonAgent : IUnillmAgent
    {
        public event UnillmOnAgentReceivedMessageEventHandler OnReceivedMessage;

        private readonly HttpClient _httpClient = new();

        private UnillmCommonAgentModelConfig _config;

        private readonly List<UnillmMessage> _context = new();

        public bool HasInit { get; private set; } = false;

        public bool IsPending { get; private set; } = false;

        public UnillmMessage[] Context => _context.ToArray();

        public UnillmCommmonAgent() : this(new UnillmCommonAgentModelConfig())
        {
        }

        public UnillmCommmonAgent(UnillmCommonAgentModelConfig config)
        {
            _config = config;

            if (!CheckConfig())
            {
                UnillmLogger.Error("Config is not valid");
                return;
            }

            // 设置请求头
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _config.Key);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public void Init(string systemPropmt = "you are a helpful assitant.")
        {
            if (HasInit)
            {
                UnillmLogger.Warrning("Agent has init");
                return;
            }

            HasInit = true;
            PushMessage(UnillmMessage.MakeSystemMessage(systemPropmt));
        }

        private bool CheckConfig()
        {
            if (_config is null)
            {
                UnillmLogger.Warrning($"Config is null");
                return false;
            }

            if (string.IsNullOrEmpty(_config.URL))
            {
                UnillmLogger.Warrning("Config url is empty");
                return false;
            }

            if (string.IsNullOrEmpty(_config.Key))
            {
                UnillmLogger.Warrning("Config key is empty");
                return false;
            }

            return true;
        }

        private void PushMessage(UnillmMessage message)
        {
            _context.Add(message);
        }

        private async System.Threading.Tasks.Task InternalSend()
        {
            string jsonContent = UnillmJsonHelper.ToJson(new RequestContent()
            {
                model = _config.Model,
                messages = _context.Select(MessageContent.FromMessage).ToArray()
            });

            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // 发送请求并获取响应
            HttpResponseMessage response = await _httpClient.PostAsync(_config.URL, content);

            // 处理响应
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var receivedMessage = UnillmJsonHelper.ToObject<ResponseContent>(responseContent)?.FilterMessage();

                if (receivedMessage is not null)
                {
                    PushMessage(receivedMessage);
                    OnReceivedMessage?.Invoke(this, new UnillmOnAgentReceivedMessageEventArgs()
                    {
                        Message = receivedMessage
                    });
                }
                else
                {
                    OnReceivedMessage?.Invoke(this, new UnillmOnAgentReceivedMessageEventArgs()
                    {
                        ErrorReason = $"Received a null message.\nResponse content is\n{responseContent}"
                    });
                }
            }
            else
            {
                OnReceivedMessage?.Invoke(this, new UnillmOnAgentReceivedMessageEventArgs()
                {
                    ErrorReason = $"Request failed.\nResponse status code is {response.StatusCode}"
                });
            }

            IsPending = false;
        }

        public bool Send(UnillmMessage messageToSend)
        {
            if (!HasInit)
            {
                UnillmLogger.Warrning("Agent has not init");
                return false;
            }

            if (IsPending)
            {
                UnillmLogger.Warrning("Agent is pending");
                return false;
            }

            IsPending = true;
            PushMessage(messageToSend);
            _ = InternalSend();
            return true;
        }

        private class MessageContent
        {
            public string role;
            public string content;

            public static MessageContent FromMessage(UnillmMessage message)
            {
                return new()
                {
                    role = message.Role,
                    content = message.Content
                };
            }
        }

        private class RequestContent
        {
            public string model;
            public MessageContent[] messages;
        }

        private class ResponseChoiceContent
        {
            public MessageContent message;
        }

        private class ResponseContent
        {
            public string id;

            public long created;

            public ResponseChoiceContent[] choices;

            public UnillmMessage FilterMessage()
            {
                if (choices is not null && choices.Length > 0)
                {
                    var fisrtChoice = choices[0];
                    if (fisrtChoice is not null)
                    {
                        return new UnillmMessage(fisrtChoice.message.role, fisrtChoice.message.content, id, created);
                    }
                }

                return null;
            }
        }
    }
}
