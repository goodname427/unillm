using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    class E005_CardView : MonoBehaviour
    {
        private E005_Card _card;
        public E005_Card Card
        {
            get => _card;
            set 
            {
                _card = value;
                Image.sprite = CardSprites[(int)_card.Rank * 4 + (int)_card.Suit];
            }
        }

        public Sprite[] CardSprites;
        public Image Image;
    }
}
