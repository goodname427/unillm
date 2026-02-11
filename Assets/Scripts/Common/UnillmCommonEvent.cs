using System;

namespace unillm
{
    /// <summary>
    /// 函数型事件，该类型的事件主要用于延迟返回一个函数的结果，无论成功与否都一定会调用
    /// </summary>
    public class UnillmFuctionalEventArgs : EventArgs
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public virtual bool IsSuccess => string.IsNullOrEmpty(ErrorReason);

        private string _errorReason = string.Empty;
        /// <summary>
        /// 错误原因
        /// </summary>
        public virtual string ErrorReason
        {
            get => _errorReason; 
            set
            {
                _errorReason = value;
                if (!IsSuccess)
                {
                    UnillmLogger.Error(ErrorReason);
                }
            }
        }
    }
}