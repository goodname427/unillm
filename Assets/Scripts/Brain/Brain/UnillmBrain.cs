using System;

namespace unillm
{
    public class UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> : UnillmFuctionalEventArgs
    {
        public TInput Input { get; set; }
        public TOutput Output { get; set; }
    }

    public delegate void UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput>(
        UnillmBrain<TInput, TOutput> brain, 
        UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> args) 
        where TInput : new() where TOutput : new();

    /// <summary>
    /// 接受特定对象输入和产生特定对象输出以及存储记忆
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class UnillmBrain<TInput, TOutput> where TInput : new() where TOutput : new()
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

        public UnillmBrain(IUnillmAgent agent = null, string background = "")
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

        public UnillmBrain(string background) : this(null, background)
        {
        }

        ~UnillmBrain()
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
                        Input  = _currentInput,
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
