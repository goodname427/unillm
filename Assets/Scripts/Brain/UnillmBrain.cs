using System;

namespace unillm
{
    public class UnillmOnBrainThinkCompleteEventArgs<TInput, TOutput> : EventArgs
    {
        public TOutput Output { get; set; }
    }

    public delegate void UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput>(UnillmBrain<TInput, TOutput> brain, UnillmOnBrainThinkCompleteEventArgs<TInput, TOutput> args) where TInput : new() where TOutput : new();

    /// <summary>
    /// 接受特定对象输入和产生特定对象输出以及存储记忆
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class UnillmBrain<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        public event UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput> OnThinkCompleted;

        private readonly IUnillmAgent _agent;

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

        public bool Think(TInput input)
        {
            if (_agent is null)
            {
                UnillmLogger.Error("Agent is null");
                return false;
            }

            return _agent.Send(UnillmMessage.MakeUserMessage(UnillmJsonHelper.ToJson(input)));
        }

        private void OnReceivedMessage(IUnillmAgent agent, UnillmOnAgentReceivedMessageEventArgs args)
        {
            UnillmLogger.Log($"The brain received message is {args.Message.Content}");
            OnThinkCompleted?.Invoke(this, new UnillmOnBrainThinkCompleteEventArgs<TInput, TOutput>()
            {
                Output = args.Message.Get<TOutput>()
            });
        }
    }
}
