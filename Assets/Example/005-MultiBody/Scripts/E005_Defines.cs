using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace unillm.Example
{
    static class E005_Constant
    {
        /// <summary>
        /// 默认手牌
        /// </summary>
        public readonly static int[] DefaultHandCard = new int[5] { 1, 2, 3, 4, 5 };

        /// <summary>
        /// 默认手牌数量
        /// </summary>
        public static int HandCardCount => DefaultHandCard.Length;
    }

    /// <summary>
    /// 花色
    /// </summary>
    enum E005_Suit
    {
        Heart,
        Diamond,
        Club,
        Spade,
    }

    /// <summary>
    /// Rank
    /// </summary>
    enum E005_Rank
    {
        Ace,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
    }

    /// <summary>
    /// 牌
    /// </summary>
    struct E005_Card : IEquatable<E005_Card>, IComparable<E005_Card>
    {
        public E005_Suit Suit { get; set; }
        public E005_Rank Rank { get; set; }

        public E005_Card(E005_Rank rank, E005_Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public override readonly string ToString()
        {
            return $"{Rank}_{Suit}";
        }

        public static bool operator ==(E005_Card l, E005_Card r)
        {
            return l.Equals(r);
        }

        public static bool operator !=(E005_Card l, E005_Card r)
        {
            return !(l == r);
        }

        public override bool Equals(object obj)
        {
            return obj is E005_Card other
                && Equals(other);
        }

        public bool Equals(E005_Card other)
        {
            return Suit == other.Suit && Rank == other.Rank;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Suit, Rank);
        }

        public int CompareTo(E005_Card other)
        {
            if (Rank != other.Rank)
            {
                return Rank - other.Rank;
            }

            return Suit - other.Suit;
        }
    }

    class E005_CardManager : IUnillmSense
    {
        public static bool Exchange(E005_CardManager source, E005_CardManager target, E005_Card card)
        {
            if (!source.CanRemove(card))
            {
                return false;
            }

            if (!target.CanAdd(card))
            {
                return false;
            }

            source.Remove(card);
            target.Add(card);

            return true;
        }

        public event Action<E005_CardManager> OnCardChanged;
        public event OnUnillmSensedEventHandler OnSensed;

        protected List<E005_Card> OwnedCards { get; } = new();

        public IEnumerable<E005_Card> Cards => OwnedCards.OrderBy(c => c);

        public int CardCount => OwnedCards.Count;

        public virtual bool CanRemove(E005_Card card)
        {
            return OwnedCards.Contains(card);
        }

        public virtual bool CanAdd(E005_Card card)
        {
            return true;
        }

        public virtual bool Add(E005_Card card)
        {
            OwnedCards.Add(card);
            OnCardChanged?.Invoke(this);
            return true;
        }

        public virtual bool Remove(E005_Card card)
        {
            if (OwnedCards.Remove(card))
            {
                OnCardChanged?.Invoke(this);
                return true;
            }

            return false;
        }

        public virtual bool Empty()
        {
            OwnedCards.Clear();
            OnCardChanged?.Invoke(this);
            return true;
        }

        public virtual void SensedCard()
        {
            var builder = new StringBuilder();
            builder.Append("你所拥有的手牌: ");
            foreach (var card in Cards)
            {
                builder.Append($"{card} ");
            }

            OnSensed?.Invoke(this, new UnillmOnSensedEventArgs
            {
                Sensed = builder.ToString()
            });
        }
    }

    class E005_PileCardManager : E005_CardManager
    {
        private IEnumerable<E005_Card> GetPiles()
        {
            for (int i = (int)E005_Suit.Heart; i <= (int) E005_Suit.Spade; i++)
            {
                for (int j = (int)E005_Rank.Ace; j <= (int)E005_Rank.King; j++)
                {
                    yield return new E005_Card((E005_Rank)j, (E005_Suit)i);
                }
            }
        }

        public void Fill()
        {
            OwnedCards.Clear();
            foreach (var card in GetPiles())
            {
                OwnedCards.Add(card);
            }
        }

        public void Ruffle()
        {
            for (int i = 0; i < 100; i++)
            {
                var j = UnityEngine.Random.Range(0, OwnedCards.Count);
                var k = UnityEngine.Random.Range(0, OwnedCards.Count);
                (OwnedCards[j], OwnedCards[k]) = (OwnedCards[k], OwnedCards[j]);
            }
        }

        public void DrawBy(E005_CardManager cardManager)
        {
            if (OwnedCards.Count > 0)
            {
                Exchange(this, cardManager, OwnedCards[^1]);
            }
        }
    }
}
