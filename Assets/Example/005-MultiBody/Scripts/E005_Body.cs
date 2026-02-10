using System.Collections.Generic;

namespace unillm.Example
{
    class E005_CardEventArgs : UnillmBodyDoEventArgs
    {
        [UnillmPropmtDescription("理由")]
        public string Reason = "";
    }

    class E005_PlayEventArgs : E005_CardEventArgs
    {
        [UnillmPropmtDescription("你选择出的牌。这些牌必须是你的手牌，且不能重复")]
        public List<E005_Card> Cards = new() { new E005_Card() };

        [UnillmPropmtDescription("如果当前是你叫牌，你需要指定你叫的点数，点数可以与实际的牌的点数不同；如果当前是你跟牌，则无需指定")]
        public E005_Rank Rank;
    }

    class E005_Play : IUnillmBody<E005_PlayEventArgs>
    {
        private E005_Player _owner;

        public string Name => "出牌";

        public string Description => "出任意数量的牌";

        public bool Do(E005_PlayEventArgs eventArgs, UnillmBodyDoResult result)
        {
            result.ErrorReason = E005_GameManager.Instance.Play(_owner, eventArgs.Cards, eventArgs.Rank);
            return result.IsSuccess;
        }

        public void OnEquipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new()
        {
            if (human is E005_Player.Human player)
            {
                _owner = player.Outer;
            }
        }
    }

    class E005_Skip : IUnillmBody<E005_CardEventArgs>
    {
        private E005_Player _owner;

        public string Name => "过牌";

        public string Description => "不跟也不质疑，直接跳过，轮到下家";

        public bool Do(E005_CardEventArgs eventArgs, UnillmBodyDoResult result)
        {
            result.ErrorReason = E005_GameManager.Instance.Skip(_owner);
            return result.IsSuccess;
        }

        public void OnEquipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new()
        {
            if (human is E005_Player.Human player)
            {
                _owner = player.Outer;
            }
        }
    }

    class E005_Call : IUnillmBody<E005_CardEventArgs>
    {
        private E005_Player _owner;

        public string Name => "质疑";

        public string Description => "不信上家，翻开上家出的牌进行验证";

        public bool Do(E005_CardEventArgs eventArgs, UnillmBodyDoResult result)
        {
            result.ErrorReason = E005_GameManager.Instance.Call(_owner);
            return result.IsSuccess;
        }

        public void OnEquipped<TInput, TOutput>(IUnillmHuman<TInput, TOutput> human) where TInput : new() where TOutput : new()
        {
            if (human is E005_Player.Human player)
            {
                _owner = player.Outer;
            }
        }
    }
}