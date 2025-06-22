using System.Collections.Generic;

namespace unillm
{
    public interface IUnillmAgent
    {
        /// <summary>
        /// 收到消息的回调时间
        /// </summary>
        event OnReceivedMessageEventHandler OnReceivedMessage;

        /// <summary>
        /// 获取上下文消息
        /// </summary>
        UnillmMessage[] Context { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messagesToSend"></param>
        /// <returns></returns>
        bool Send(IEnumerable<UnillmMessage> messagesToSend);
    }
}
