using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    class E005_GameManager : MonoBehaviour, IUnillmSense
    {
        private static E005_GameManager s_instance;
        public static E005_GameManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<E005_GameManager>();
                }

                return s_instance;
            }
        }

        // ==================================================================
        public E005_Player Playerfabs;
        public int PlayerCount;

        public E005_CardManagerView CurrentCardManagerView;
        public E005_PlayerView PlayerViewPrefabs;
        public Transform PlayerPanel;
        public TextMeshProUGUI NotifyText;
        public Button NextButton;

        // ==================================================================
        public event OnUnillmSensedEventHandler OnSensed;

        // ==================================================================
        private readonly List<E005_Player> _players = new();

        private bool _canNext = true;

        /// <summary>
        /// 当前玩家索引
        /// </summary>
        private int _current = -1;

        /// <summary>
        /// 指定下回合的玩家
        /// </summary>
        private int _overrideNextPlayer = -1;

        /// <summary>
        /// 当前玩家
        /// </summary>
        private E005_Player CurrentPlayer => _current < 0 ? null : _players[_current];

        /// <summary>
        /// 发牌堆
        /// </summary>
        private E005_PileCardManager _pile = new();

        /// <summary>
        /// 当前牌堆
        /// </summary>
        private E005_CardManager _currentCards = new();

        /// <summary>
        /// 当前牌堆叫的点数
        /// </summary>
        private E005_Rank? _currentRank = null;

        /// <summary>
        /// 上一轮出牌的玩家
        /// </summary>
        private E005_Player _lastPlayedPlayer = null;

        /// <summary>
        /// 上一轮出的牌
        /// </summary>
        private List<E005_Card> _lastPlayedCards = new();

        private void Start()
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                var player = Instantiate(Playerfabs);
                player.No = i;
                player.Init();
                player.OnTurnCompleted += OnPlayerTurnCompleted;

                var playerView = Instantiate(PlayerViewPrefabs, PlayerPanel);
                playerView.Init(player);

                _players.Add(player);
            }

            CurrentCardManagerView.Bind(_currentCards);

            EnableNextButton(false);
            NextButton.onClick.AddListener(Next);

            StartGame();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Next();
            }
        }

        private void Notify(string msg)
        {
            OnSensed?.Invoke(this, new UnillmOnSensedEventArgs
            {
                Sensed = $"[系统] {msg}"
            });

            Debug.Log(msg);
            NotifyText.text = $"[系统] {msg}";
        }

        private void StartGame()
        {
            _current = -1;

            _pile.Fill();
            _pile.Ruffle();

            // 初始手牌
            int cardCount = _pile.CardCount / _players.Count;
            foreach (var player in _players)
            {
                for (int i = 0; i < cardCount; i++)
                {
                    _pile.DrawBy(player.HandCards);
                }
            }

            EnableNextButton(true);
        }

        public void EnableNextButton(bool enabled)
        {
            _canNext = enabled;
            NextButton.enabled = enabled;
            NextButton.interactable = enabled;
            // NextButton.image.color = enabled ? NextButton.colors.normalColor : NextButton.colors.disabledColor;
        }

        public void Next()
        {
            if (!_canNext)
            {
                return;
            }
            EnableNextButton(false);

            _current = _overrideNextPlayer >= 0 ? _overrideNextPlayer : _current + 1;
            _overrideNextPlayer = -1;
            if (_current >= PlayerCount)
            {
                _current = 0;
            }

            if (_lastPlayedPlayer == CurrentPlayer)
            {
                Clear("所有玩家跳过");
            }

            Notify($"现在是 {CurrentPlayer.Name} 的回合, 他的手牌数量为 {CurrentPlayer.HandCards.CardCount}");
            CurrentPlayer.StartTurn();
        }

        private void OnPlayerTurnCompleted(E005_Player player)
        {
            if (player.HandCards.CardCount == 0)
            {
                Notify("游戏结束");
                return;
            }

            EnableNextButton(true);
        }

        public string Play(E005_Player player, List<E005_Card> cards, E005_Rank? rank = null)
        {
            if (player != CurrentPlayer)
            {
                return "当前并不是你的回合，你无法出牌";
            }

            if (_currentRank == null && rank == null)
            {
                return "本轮由你开始，你必须选择本轮喊出的点数";
            }
            _currentRank ??= rank;

            foreach (var card in cards)
            {
                if (!player.HandCards.CanRemove(card))
                {
                    return $"你的手牌中不包含 {card}，请你重新出牌";
                }

                if (cards.Count(c => c == card) > 1)
                {
                    return $"你不能重复选择相同的手牌（{card}）";
                }
            }

            _lastPlayedCards.Clear();
            foreach (var card in cards)
            {
                E005_CardManager.Exchange(player.HandCards, _currentCards, card);
                _lastPlayedCards.Add(card);
            }
            _lastPlayedPlayer = player;

            Notify($"{player.Name} 出牌 {cards.Count} 个 {_currentRank}");

            return null;
        }

        public string Call(E005_Player player)
        {
            if (player != CurrentPlayer)
            {
                return "当前并不是你的回合，你无法出牌";
            }

            if (player == _lastPlayedPlayer)
            {
                return "你不能质疑自己的出牌";
            }

            if (_lastPlayedPlayer == null)
            {
                return "本轮由你开始，你不能选择开牌";
            }

            var success = _lastPlayedCards.Any(card => card.Rank != _currentRank);
            var cardsToAdd = _currentCards.Cards.Select(c => c).ToArray();
            var playerToAdd = success ? _lastPlayedPlayer : player;
            foreach (var card in cardsToAdd)
            {
                E005_CardManager.Exchange(_currentCards, playerToAdd.HandCards, card);
            }

            if (success)
            {
                Notify($"{player.Name} 质疑 {_lastPlayedPlayer.Name} 成功，{_lastPlayedPlayer.Name} 收走牌面上的所有牌");
            }
            else
            {
                Notify($"{player.Name} 质疑 {_lastPlayedPlayer.Name} 失败，{player.Name} 收走牌面上的所有牌");
            }

            _overrideNextPlayer = playerToAdd.No;
            Clear($"{player.Name} 开牌");

            return null;
        }

        public string Skip(E005_Player player)
        {
            if (player != CurrentPlayer)
            {
                return "当前并不是你的回合，你无法出牌";
            }

            if (_lastPlayedPlayer == null)
            {
                return "本轮由你开始，你不能跳过";
            }

            Notify($"{player.Name} 跳过");

            return null;
        }

        public void Clear(string reason)
        {
            _lastPlayedPlayer = null;
            _currentCards.Empty();
            _currentRank = null;

            Notify($"由于 {reason}, 本轮结束，开始下一轮");
        }
    }
}
