using System;
using System.Text.RegularExpressions;

namespace unillm
{
    [System.Serializable]
    public class UnillmMessage
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public string ID { get; }
        /// <summary>
        /// 发送消息的角色
        /// </summary>
        public string Role { get; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Content { get; }
        /// <summary>
        /// 消息发送的时间戳
        /// </summary>
        public long Created { get; }

        public UnillmMessage(string role, string content, string id = null, long created = -1)
        {
            Role = role;
            Content = content;

            // todo 去重计算
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

        /// <summary>
        /// 将Content转为指定对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Get<T>() where T : new()
        {
            string pattern = @"^```json\s*|\s*```$";

            // 过滤可能的代码包围块
            string cleanJson = Regex.Replace(Content, pattern, "", RegexOptions.Multiline);

            return UnillmJsonHelper.ToObject<T>(cleanJson.Trim());
        }
    }
}
