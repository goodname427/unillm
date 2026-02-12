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
    public interface IUnillmSense : IUnillmHumanEquipable
    {
        /// <summary>
        /// 感知到任何事情时通知人类
        /// </summary>
        event OnUnillmSensedEventHandler OnSensed;
    }
}
