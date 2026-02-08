namespace unillm
{
    public class UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> : UnillmFuctionalEventArgs
    {
        public TInput Input { get; set; }
        public TOutput Output { get; set; }
    }

    public delegate void UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput>(
        UnillmCommonBrain<TInput, TOutput> brain,
        UnillmOnBrainThinkCompletedEventArgs<TInput, TOutput> args)
        where TInput : new() where TOutput : new();

    /// <summary>
    /// 能够接受输入，并思考后输出
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public interface IUnillmBrain<TInput, TOutput> where TInput : new() where TOutput : new()
    {
        /// <summary>
        /// 思考结束时调用
        /// </summary>
        event UnillmOnBrainThinkCompletedEventHandler<TInput, TOutput> OnThinkCompleted;

        /// <summary>
        /// 是否正在思考
        /// </summary>
        bool IsThinking { get; }

        /// <summary>
        /// 思考
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        bool Think(TInput input);
    }
}
