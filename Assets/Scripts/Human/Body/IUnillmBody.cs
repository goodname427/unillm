using System;

namespace unillm
{
    /// <summary>
    /// 动作执行参数
    /// </summary>
    public class UnillmBodyDoArgs
    {
        
    }

    /// <summary>
    /// 动作执行结果
    /// </summary>
    public class UnillmBodyDoResult : UnillmFuctionalEventArgs
    {

    }

    /// <summary>
    /// 能够执行某种任务
    /// </summary>
    public interface IUnillmBody : IUnillmHumanEquipable
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
        bool Do(UnillmBodyDoArgs eventArgs, UnillmBodyDoResult result);
    }

    public interface IUnillmBody<TDoArgs> : IUnillmBody where TDoArgs : UnillmBodyDoArgs
    {
        Type IUnillmBody.ArgsType => typeof(TDoArgs);

        bool IUnillmBody.Do(UnillmBodyDoArgs eventArgs, UnillmBodyDoResult result)
        {
            return Do(eventArgs as TDoArgs, result);
        }

        bool Do(TDoArgs eventArgs, UnillmBodyDoResult result);
    }
}
