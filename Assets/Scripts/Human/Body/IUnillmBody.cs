using System;

namespace unillm
{
    public class UnillmBodyDoEventArgs : EventArgs
    {
        
    }

    public class UnillmBodyDoResult : UnillmFuctionalEventArgs
    {

    }

    /// <summary>
    /// 能够执行某种任务
    /// </summary>
    public interface IUnillmBody : IUnillmEquipable
    {
        /// <summary>
        /// 用于标识该Body
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 用于描述该行动的用法
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 参数的类型
        /// </summary>
        Type ArgsType { get; }

        /// <summary>
        /// 执行某一项任务
        /// </summary>
        /// <returns></returns>
        bool Do(UnillmBodyDoEventArgs eventArgs, UnillmBodyDoResult result);
    }

    public interface IUnillmBody<TDoArgs> : IUnillmBody where TDoArgs : class
    {
        Type IUnillmBody.ArgsType => typeof(TDoArgs);

        bool IUnillmBody.Do(UnillmBodyDoEventArgs eventArgs, UnillmBodyDoResult result)
        {
            return Do(eventArgs as TDoArgs, result);
        }

        bool Do(TDoArgs eventArgs, UnillmBodyDoResult result);
    }
}
