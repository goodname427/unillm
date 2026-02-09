namespace unillm
{
    public class UnillmCommonBrainInitConfig
    {
        /// <summary>
        /// 所使用的Agent
        /// </summary>
        public IUnillmAgent Agent;

        /// <summary>
        /// 提示词
        /// </summary>
        public string Prompt;

        /// <summary>
        /// 是否附加InOut信息
        /// </summary>
        public bool AttachInOutInfo;

        public UnillmCommonBrainInitConfig(IUnillmAgent agent, string prompt, bool attachInOutInfo)
        {
            Agent = agent;
            Prompt = prompt;
            AttachInOutInfo = attachInOutInfo;
        }

        public UnillmCommonBrainInitConfig(string prompt = "", bool attachInOutInfo = true) : this(new UnillmCommmonAgent(), prompt, attachInOutInfo)
        {
        }
    }

    /// <summary>
    /// 通用的Brain实现
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
        private IUnillmAgent _agent;

        /// <summary>
        /// 当前输入
        /// </summary>
        private TInput _currentInput = default;

        /// <summary>
        /// 是否正在思考
        /// </summary>
        public bool IsThinking => _agent.IsPending;

        /// <summary>
        /// 初始化参数
        /// </summary>
        private UnillmCommonBrainInitConfig _config;

        public UnillmCommonBrain(UnillmCommonBrainInitConfig config)
        {
            _config = config;
        }

        public void OnEquipped(IUnillmHuman<TInput, TOutput> human)
        {
            _agent = _config.Agent;
            if (_agent.HasInit)
            {
                UnillmLogger.Warrning("Agent has init, brain will create a new agent");
                _agent = new UnillmCommmonAgent();
                return;
            }

            var propmt = _config.AttachInOutInfo ? 
                new UnillmConcatPropmtBuilder(
                    _config.Prompt, 
                    UnillmTypePropmtBuilder.PropertyFormat, 
                    new UnillmInOutPropmtBuilder<TInput, TOutput>().Build()
                ).Build() : 
                _config.Prompt;
            
            _agent.Init(propmt);
            _agent.OnReceivedMessage += OnReceivedMessage;
        }

        public void OnUnequipped(IUnillmHuman<TInput, TOutput> human)
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
                        ErrorReason = $"Can not transition the message content to a instance of the type({typeof(TOutput)})\nThe original message is:\n {args.Message.Content}"
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
