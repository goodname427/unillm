using System;
using UnityEngine;

namespace unillm
{
    [System.Serializable]
    public class UnillmMessage
    {
        /// <summary>
        /// ��ϢID
        /// </summary>
        public string ID { get; }
        /// <summary>
        /// ������Ϣ�Ľ�ɫ
        /// </summary>
        public string Role { get; }
        /// <summary>
        /// ��Ϣ����
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// ��Ϣ���͵�ʱ���
        /// </summary>
        public long Created { get; }

        public UnillmMessage(string role, string content, string id = null, long created = -1)
        {
            Role = role;
            Content = content;

            // todo ȥ�ؼ���
            ID = id ?? UnityEngine.Random.Range(0, long.MaxValue).ToString();

            if (created < 0)
            {
                created = DateTime.Now.ToUnixTimestamp();
            }

            Created = created;
        }

        public static UnillmMessage MakeSystemMessage(string content)
        {
            return new UnillmMessage("system", content);
        }

        public static UnillmMessage MakeUserMessage(string content)
        {
            return new UnillmMessage("user", content);
        }

        public static UnillmMessage MakeAssistantMessage(string content)
        {
            return new UnillmMessage("assistant", content);
        }
    }
}
