using System;

namespace unillm
{
    // 任务执行的参数
    public class UnillmOnBodyDoEventArgs : EventArgs
    {
        
    }

    /// <summary>
    /// 用于人类执行行动
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
