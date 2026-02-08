using TMPro;
using UnityEngine;

namespace unillm.Example
{
    /// <summary>
    /// 简单的对话窗口
    /// </summary>
    public class E001_MessagePanel : MonoBehaviour
    {
        [SerializeField]
        TMP_Text _titleText;
        [SerializeField]
        TMP_Text _bodyText;

        public void SetMessage(UnillmMessage message)
        {
            _titleText.text = $"{message.Role}\t{DateTimeExtensions.FromUnixTimestamp(message.Created)}";
            _bodyText.text = message.Content;
        }
    }
}
