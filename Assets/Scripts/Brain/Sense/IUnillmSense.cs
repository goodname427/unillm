using System;

namespace unillm
{
    public class UnillmOnSensedEventArgs : EventArgs
    {
        public string Sensed;
    }

    public delegate void OnUnillmSensedEventHandler(IUnillmSense sense, UnillmOnSensedEventArgs args);

    public interface IUnillmSense
    {
        /// <summary>
        /// 感知到任何事情时通知人类
        /// </summary>
        public event OnUnillmSensedEventHandler OnSensed;

        /// <summary>
        /// 被人类装备时调用
        /// </summary>
        /// <param name="human"></param>
        public void OnEquiped(IUnillmHuman human);

        /// <summary>
        /// 从人类取消装备时调用
        /// </summary>
        /// <param name="human"></param>
        public void OnUnequiped(IUnillmHuman human);
    }
}
