using System.Collections.Generic;

namespace unillm
{
    public interface IUnillmAgent
    {
        /// <summary>
        /// �յ���Ϣ�Ļص�ʱ��
        /// </summary>
        event OnReceivedMessageEventHandler OnReceivedMessage;

        /// <summary>
        /// ��ȡ��������Ϣ
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
