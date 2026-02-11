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
        [Header("游戏设置")]
        public List<E005_Player> Players = new();

        [Header("Player UI")]
        public E005_PlayerView PlayerViewPrefabs;
        public Transform PlayerPanel;

        [Header("Global UI")]
        public TextMeshProUGUI NotifyText;
        public TextMeshProUGUI CurrentCardsText;
        public E005_CardManagerView LastPlayedCardManagerView;
        public E005_CardManagerView CurrentCardManagerView;
        public Button NextButton;

        // ==================================================================
        public event OnUnillmSensedEventHandler OnSensed;

        // ==================================================================
        /// <summary>
        /// 是否能够进入下一回合
        /// </summary>
        private bool _canNext = true;

        /// <summary>
        /// 当前玩家索引
        /// </summary>
        private int _current = -1;

        /// <summary>
        /// 指定下回合的玩家
        /// </summary>
        private int _overrideNextPlayer = -1;

        private int PlayerCount => Players.Count;

        /// <summary>
        /// 当前玩家
        /// </summary>
        private E005_Player CurrentPlayer => _current < 0 ? null : Players[_current];

        /// <summary>
        /// 发牌堆
        /// </summary>
        private readonly E005_PileCardManager _pile = new();

        /// <summary>
        /// 当前牌堆
        /// </summary>
        private readonly E005_CardManager _currentCards = new();

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
        private readonly E005_CardManager _lastPlayedCards = new();

        private void Start()
        {
            int i = 0;
            foreach (var player in Players)
            {
                player.No = i++;
                player.Init();
                player.OnTurnCompleted += OnPlayerTurnCompleted;

                var playerView = Instantiate(PlayerViewPrefabs, PlayerPanel);
                playerView.Bind(player);
            }

            LastPlayedCardManagerView.Bind(_lastPlayedCards);
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

        /// <summary>
        /// 将当前信息通知玩家
        /// </summary>
        /// <param name="msg"></param>
        public void Notify(string msg, string role = "系统")
        {
            OnSensed?.Invoke(this, new UnillmOnSensedEventArgs
            {
                Sensed = $"[{role}] {msg}"
            });

            Debug.Log(msg);
            if (role == "系统")
            {
                NotifyText.text = $"[{role}] {msg}";
            }
        }

        /// <summary>
        /// 游戏开始
        /// </summary>
        private void StartGame()
        {
            _current = -1;

            // 初始手牌
            _pile.Fill();
            _pile.Ruffle();

            int cardCount = _pile.CardCount / Players.Count;
            foreach (var player in Players)
            {
                for (int i = 0; i < cardCount; i++)
                {
                    _pile.DrawBy(player.HandCards);
                }
            }

            Notify($"游戏开始，场上玩家分别为{string.Join(' ', Players.Select(p => p.Name))}");

            EnableNextButton(true);
        }

        /// <summary>
        /// 允许或者禁止进入下一回合
        /// </summary>
        /// <param name="enabled"></param>
        public void EnableNextButton(bool enabled)
        {
            _canNext = enabled;
            NextButton.enabled = enabled;
            NextButton.interactable = enabled;
            // NextButton.image.color = enabled ? NextButton.colors.normalColor : NextButton.colors.disabledColor;
        }

        /// <summary>
        /// 进入下一回合
        /// </summary>
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
                Clear($"现在是 {CurrentPlayer.Name} 的回合，他的手牌数量为 {CurrentPlayer.HandCards.CardCount}。 由于他上一轮出牌后其他人全部选择了过牌");
            }
            else
            {
                Notify($"现在是 {CurrentPlayer.Name} 的回合，他的手牌数量为 {CurrentPlayer.HandCards.CardCount}");
            }
            CurrentPlayer.StartTurn();
        }

        /// <summary>
        /// 回合结束
        /// </summary>
        /// <param name="player"></param>
        private void OnPlayerTurnCompleted(E005_Player player)
        {
            if (player.HandCards.CardCount == 0)
            {
                Notify("游戏结束");
                return;
            }

            CurrentCardsText.text = $"当前叫牌点数：{(_currentRank == null ? "无" : _currentRank)} 上一轮出牌玩家：{(_lastPlayedPlayer == null ? "无" : _lastPlayedPlayer.Name)}";

            EnableNextButton(true);
        }

        /// <summary>
        /// 出牌
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cards"></param>
        /// <param name="rank"></param>
        /// <returns></returns>
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

            if (_currentRank != null && rank != _currentRank)
            {
                return $"本轮已经开始，出牌时你只能喊出与本轮相同的点数 {_currentRank}";
            }

            if (cards.Count == 0)
            {
                return "你不能选择出0张牌";
            }

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

            _currentRank ??= rank;
            _lastPlayedCards.Empty();
            foreach (var card in cards)
            {
                E005_CardManager.Exchange(player.HandCards, _currentCards, card);
                _lastPlayedCards.Add(card);
            }
            _lastPlayedPlayer = player;

            Notify($"{player.Name} 出牌 {cards.Count} 个 {_currentRank}。当前桌面上牌的数量：{_currentCards.CardCount}");

            return null;
        }

        /// <summary>
        /// 开牌
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
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
                return "本轮由你开始，你不能选择质疑";
            }

            var success = _lastPlayedCards.Cards.Any(card => card.Rank != _currentRank);
            var cardsToAdd = _currentCards.Cards.Select(c => c).ToArray();
            var playerToAdd = success ? _lastPlayedPlayer : player;
            foreach (var card in cardsToAdd)
            {
                E005_CardManager.Exchange(_currentCards, playerToAdd.HandCards, card);
            }

            if (success)
            {
                Clear($"{player.Name} 质疑 {_lastPlayedPlayer.Name} 成功。{_lastPlayedPlayer.Name} 收走桌上的所有牌");
            }
            else
            {
                Clear($"{player.Name} 质疑 {_lastPlayedPlayer.Name} 失败。{player.Name} 收走桌上的所有牌");
            }

            _overrideNextPlayer = playerToAdd.No;

            return null;
        }

        /// <summary>
        /// 过牌
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 清空当前牌堆
        /// </summary>
        /// <param name="reason"></param>
        public void Clear(string reason)
        {
            _lastPlayedPlayer = null;
            _lastPlayedCards.Empty();
            _currentRank = null;
            _currentCards.Empty();

            Notify($"{reason}，本轮结束，开始下一轮");
        }
    }
}
