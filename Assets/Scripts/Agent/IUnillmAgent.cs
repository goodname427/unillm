using System;

namespace unillm
{
    public class UnillmOnAgentReceivedMessageEventArgs : UnillmFuctionalEventArgs
    {
        public UnillmMessage Message { get; set; }
    }

    public delegate void UnillmOnAgentReceivedMessageEventHandler(IUnillmAgent agent, UnillmOnAgentReceivedMessageEventArgs args);

    /// <summary>
    /// 用于与大模型直接交互
    /// </summary>
    public interface IUnillmAgent
    {
        /// <summary>
        /// 收到消息的回调事件
        /// </summary>
        event UnillmOnAgentReceivedMessageEventHandler OnReceivedMessage;

        /// <summary>
        /// 获取上下文消息
        /// </summary>
        UnillmMessage[] Context { get; }

        /// <summary>
        /// 是否初始化完成
        /// </summary>
        bool HasInit { get; }

        /// <summary>
        /// 是否正在处理消息
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// 初始化系统提示词
        /// </summary>
        /// <param name="systemPropmt"></param>
        void Init(string systemPropmt);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="messagesToSend"></param>
        /// <returns></returns>
        bool Send(UnillmMessage messageToSend);

    }
}
