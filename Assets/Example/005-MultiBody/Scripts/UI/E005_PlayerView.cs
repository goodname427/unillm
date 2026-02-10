using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    class E005_PlayerView : MonoBehaviour
    {
        public Image BackgroudImage;
        public Image PlayerImage;
        public E005_CardManagerView CardManagerView;
        public TextMeshProUGUI TipsText;

        private Color _backgroundDefaultColor;
        public Color _backgroundActiveColor;

        public void Init(E005_Player player)
        {
            CardManagerView.Bind(player.HandCards);
            _backgroundDefaultColor = BackgroudImage.color;

            player.OnTipsUpdate += OnTipsUpdate;
            player.OnTurnStart += OnTurnStart;
            player.OnTurnCompleted += OnTurnCompleted;
        }

        private void OnTurnCompleted(E005_Player obj)
        {
            BackgroudImage.color = _backgroundDefaultColor;
        }

        private void OnTurnStart(E005_Player obj)
        {
            BackgroudImage.color = _backgroundActiveColor;
        }

        private void OnTipsUpdate(E005_Player player, string tips)
        {
            TipsText.text = tips;
        }
    }
}
