using System;

namespace unillm
{
    /// <summary>
    /// 用于返回一个函数的执行情况
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