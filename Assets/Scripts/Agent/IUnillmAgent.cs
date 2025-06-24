using System;

namespace unillm
{
    public class UnillmOnAgentReceivedMessageEventArgs : EventArgs
    {
        public UnillmMessage Message { get; set; }
    }

    public delegate void UnillmOnAgentReceivedMessageEventHandler(IUnillmAgent agent, UnillmOnAgentReceivedMessageEventArgs args);

    /// <summary>
    /// �������ģ��ֱ�ӽ���
    /// </summary>
    public interface IUnillmAgent
    {
        /// <summary>
        /// �յ���Ϣ�Ļص�ʱ��
        /// </summary>
        event UnillmOnAgentReceivedMessageEventHandler OnReceivedMessage;

        /// <summary>
        /// ��ȡ��������Ϣ
        /// </summary>
        UnillmMessage[] Context { get; }

        /// <summary>
        /// �Ƿ��ʼ�����
        /// </summary>
        bool HasInit { get; }

        /// <summary>
        /// �Ƿ����ڴ�����Ϣ
        /// </summary>
        bool IsPending { get; }

        /// <summary>
        /// ��ʼ��ϵͳ��ʾ��
        /// </summary>
        /// <param name="systemPropmt"></param>
        void Init(string systemPropmt);

        /// <summary>
        /// ������Ϣ
        /// </summary>
        /// <param name="messagesToSend"></param>
        /// <returns></returns>
        bool Send(UnillmMessage messageToSend);

    }
}
