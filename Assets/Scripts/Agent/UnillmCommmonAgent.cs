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
        private readonly HttpClient _httpClient = new();

        private UnillmCommonAgentModelConfig _config;

        private readonly List<UnillmMessage> _context = new();

        public UnillmMessage[] Context => _context.ToArray();

        public event OnReceivedMessageEventHandler OnReceivedMessage;

        public UnillmCommmonAgent(string systemPropmt = null) : this(new UnillmCommonAgentModelConfig(), systemPropmt)
        {
        }

        public UnillmCommmonAgent(UnillmCommonAgentModelConfig config, string systemPropmt = null)
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

            if (string.IsNullOrEmpty(systemPropmt))
            {
                systemPropmt = "you are a helpful assitant";
            }

            PushMessage(UnillmMessage.MakeSystemMessage(systemPropmt));
        }

        private bool CheckConfig()
        {
            if (_config is null)
            {
                UnillmLogger.Warrning($"Config is null");
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
                    OnReceivedMessage?.Invoke(this, new OnReceivedMessageEventArgs()
                    {
                        Message = receivedMessage
                    });
                }
                else
                {
                    UnillmLogger.Error($"Received none message");
                }
            }
            else
            {
                UnillmLogger.Error($"Request Failed: {response.StatusCode}");
            }
        }

        public bool Send(UnillmMessage messageToSend)
        {
            return Send(new UnillmMessage[] { messageToSend });
        }

        public bool Send(IEnumerable<UnillmMessage> messagesToSend)
        {
            foreach (var message in messagesToSend)
            {
                PushMessage(message);
            }

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
