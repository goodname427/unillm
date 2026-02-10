using System.Collections.Generic;
using UnityEngine;

namespace unillm.Example
{
    class E005_CardManagerView : MonoBehaviour
    {
        public E005_CardView CardViewPrefab;
        public Transform Content;

        private readonly List<E005_CardView> _owenedCardViews = new();

        public void Bind(E005_CardManager cardManager)
        {
            cardManager.OnCardChanged += OnCardChanged;
            Refresh(cardManager);
        }

        private void OnCardChanged(E005_CardManager cardManager)
        {
            Refresh(cardManager);
        }

        private void Refresh(E005_CardManager cardManager)
        {
            int i = 0;
            foreach (var card in cardManager.Cards)
            {
                if (i >= _owenedCardViews.Count)
                {
                    _owenedCardViews.Add(Instantiate(CardViewPrefab, Content));
                }

                _owenedCardViews[i].gameObject.SetActive(true);
                _owenedCardViews[i].Card = card;

                i++;
            }

            for (; i < _owenedCardViews.Count; i++)
            {
                _owenedCardViews[i].gameObject.SetActive(false);
            }
        }
    }
}
