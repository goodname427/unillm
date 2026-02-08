using System;

namespace unillm
{
    public class UnillmOnBodyDoEventArgs : EventArgs
    {
        
    }

    /// <summary>
    /// 能够执行某种任务
    /// </summary>
    public interface IUnillmBody
    {
        /// <summary>
        /// 用于描述该行动的用法
        /// </summary>
        string Description { get; }

        /// <summary>
        /// 执行某一项任务
        /// </summary>
        /// <returns></returns>
        bool Do(UnillmOnBodyDoEventArgs eventArgs);
    }
}
