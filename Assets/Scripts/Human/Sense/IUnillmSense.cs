using System;

namespace unillm
{
    public class UnillmOnSensedEventArgs : EventArgs
    {
        public string Sensed;
    }

    public delegate void OnUnillmSensedEventHandler(IUnillmSense sense, UnillmOnSensedEventArgs args);

    /// <summary>
    /// 能够感知某些信息
    /// </summary>
    public interface IUnillmSense
    {
        /// <summary>
        /// 感知到任何事情时通知人类
        /// </summary>
        event OnUnillmSensedEventHandler OnSensed;

        /// <summary>
        /// 被人类装备时调用
        /// </summary>
        /// <param name="human"></param>
        void OnEquiped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }

        /// <summary>
        /// 从人类取消装备时调用
        /// </summary>
        /// <param name="human"></param>
        void OnUnequiped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new() { }
    }
}
