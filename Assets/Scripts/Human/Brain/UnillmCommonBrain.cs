using System;

namespace unillm
{
    /// <summary>
    /// 通用的Brain，提供一个较为通用的解决方案
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class UnillmCommonBrain<TInput, TOutput> : IUnillmBrain<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        /// <summary>
        /// 思考结束时调用
        /// </summary>
        public event UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput> OnThinkCompleted;

        /// <summary>
        /// 所使用的Agent
        /// </summary>
        private readonly IUnillmAgent _agent;

        /// <summary>
        /// 当前输入
        /// </summary>
        private TInput _currentInput = default;

        /// <summary>
        /// 是否正在思考
        /// </summary>
        public bool IsThinking => _agent.IsPending;

        public UnillmCommonBrain(IUnillmAgent agent = null, string background = "")
        {
            _agent = agent ?? new UnillmCommmonAgent();
            if (_agent.HasInit)
            {
                UnillmLogger.Warrning("Agent has init, brain will create a new agent");
                _agent = new UnillmCommmonAgent();
                return;
            }

            var propmt = new UnillmInOutPropmtBuilder<TInput, TOutput>(background).Build();
            UnillmLogger.Log($"The brain propmt is {propmt}");
            _agent.Init(propmt);
            _agent.OnReceivedMessage += OnReceivedMessage;
        }

        public UnillmCommonBrain(string background) : this(null, background)
        {
        }

        ~UnillmCommonBrain()
        {
            _agent.OnReceivedMessage -= OnReceivedMessage;
        }

        public virtual bool Think(TInput input)
        {
            if (_agent is null)
            {
                UnillmLogger.Error("Agent is null");
                return false;
            }

            if (IsThinking)
            {
                UnillmLogger.Warrning("Brain is thinking now");
                return false;
            }

            if (_agent.Send(UnillmMessage.MakeUserMessage(UnillmJsonHelper.ToJson(input))))
            {
                _currentInput = input;
                return true;
            }

            return false;
        }

        protected virtual void OnReceivedMessage(IUnillmAgent agent, UnillmOnAgentReceivedMessageEventArgs args)
        {
            if (args.IsSuccess)
            {
                // UnillmLogger.Log($"The brain received message is {args.Message.Content}");
                var output = args.Message.Get<TOutput>();
                if (output is not null)
                {
                    OnThinkCompleted?.Invoke(this, new UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput>()
                    {
                        Input = _currentInput,
                        Output = output
                    });
                }
                else
                {
                    OnThinkCompleted?.Invoke(this, new UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput>()
                    {
                        Input = _currentInput,
                        ErrorReason = $"Can not transition the message content to a instance of the type({typeof(TOutput)})"
                    });
                }
            }
            else
            {
                OnThinkCompleted?.Invoke(this, new UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput>
                {
                    Input = _currentInput,
                    ErrorReason = $"Can not received a message because {args.ErrorReason}"
                });
            }

            _currentInput = default;
        }
    }
}
