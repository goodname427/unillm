using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    class E005_PlayerView : MonoBehaviour
    {
        public Image PlayerImage;
        public E005_CardManagerView CardManagerView;
        public TextMeshProUGUI TipsText;

        [Header("±³¾°ÑÕÉ«")]
        public Image BackgroudImage;
        public Color BackgroundDefaultColor;
        public Color BackgroundActiveColor;

        public void Bind(E005_Player player)
        {
            PlayerImage.sprite = player.Icon;

            CardManagerView.Bind(player.HandCards);

            player.OnTipsUpdate += OnTipsUpdate;
            player.OnTurnStart += OnTurnStart;
            player.OnTurnCompleted += OnTurnCompleted;
        }

        private void OnTurnCompleted(E005_Player player)
        {
            BackgroudImage.color = BackgroundDefaultColor;
        }

        private void OnTurnStart(E005_Player player)
        {
            BackgroudImage.color = BackgroundActiveColor;
        }

        private void OnTipsUpdate(E005_Player player, string tips)
        {
            TipsText.text = tips;
        }
    }
}
