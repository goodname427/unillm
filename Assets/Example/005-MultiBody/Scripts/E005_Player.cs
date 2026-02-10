using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static UnityEditor.ObjectChangeEventStream;
namespace unillm.Example
{
    class E005_Player : MonoBehaviour
    {
        public class Human : UnillmStandardHuman
        {
            public string Name;
            public E005_Player Outer;
            public E005_CardManager HandCards { get; } = new();

            public override string MakeBackground()
            {
                return @$"
你是一名技术高超的玩家(游戏中代号为{Name})，你现在正在玩吹牛逼。
游戏具体规则如下：
“吹牛逼”（也叫唬牌）是靠撒谎 + 质疑的扑克游戏，核心就是：扣牌喊牌、可真可假、有人质疑就翻牌定输赢。
一、基础准备
人数：2–6 人
牌具：1 副 52 张（不含大小王）
发牌：洗牌后平均分
二、核心流程（每一轮）
1. 叫牌 + 出牌
出牌人把牌背面朝上放在桌上，同时喊出牌型（格式：数量 + 点数）。
例：3 张 5、2 张 A、4 张 K
规则：你喊的可以和实际出的不一样（吹牛）。
2. 下家三选一
轮到下家，只能做一件事：
跟牌：出任意数量的牌，也扣着，喊同样的点数（也可吹牛）。
例：上家喊 “3 张 5”，你也出 3 张，喊 “3 张 5”。
掀牌（质疑）：不信上家，翻开上家出的牌进行验证。
过牌：不跟也不质疑，直接跳过，轮到下家。
3. 掀牌判定（最关键）
翻开后，数桌上符合所喊点数的牌（含大小王）：
真：符合数量 → 质疑的人收走桌上所有牌。
假：不够数量 → 出牌 / 跟牌的人收走桌上所有牌。
收牌后，由输的人开始下一轮。
4. 胜负
谁先出完手里所有牌，谁赢。
";
            }

            protected override IUnillmAgent MakeAgent()
            {
                return new UnillmCommmonAgent(new UnillmCommonAgentModelConfig
                {
                    Model = "qwen3-max"
                });
            }

            protected override IEnumerable<IUnillmBody> CollectBodies()
            {
                yield return new E005_Play();
                yield return new E005_Skip();
                yield return new E005_Call();
            }

            protected override IEnumerable<IUnillmSense> CollectSenses()
            {
                yield return HandCards;
                yield return E005_GameManager.Instance;
            }
        }

        public event Action<E005_Player> OnTurnStart; 
        public event Action<E005_Player> OnTurnCompleted;
        public event Action<E005_Player, string> OnTipsUpdate;

        public int No { get; set; }
        public string Name => $"玩家{No}";

        private Human _human;
        public E005_CardManager HandCards => _human.HandCards;

        public void Init()
        {
            _human = new Human
            {
                Name = Name,
                Outer = this
            };
            _human.Init();

            _human.OnTurnCompleted += OnHumanTurnCompleted;
        }

        private void OnHumanTurnCompleted(UnillmStandardHuman human, UnillmOnStandardHumanTurnCompletedEventArgs args)
        {
            var builder = new StringBuilder();
            if (!args.IsAllActionExecuteSuccess)
            {
                foreach (var result in args.ActionExecuteResults)
                {
                    builder.AppendLine(result.DoResult.ErrorReason);
                }
                
                Delay(() => _human.StartTurn(new UnillmStandardHumanStartTurnArgs
                {
                    Target = $"由于以下原因:\n{builder}\n请你重新选择操作",
                    OverrideInput = args.Input
                }));
                
                return;
            }

            var firstAction = args.ActionExecuteResults.FirstOrDefault();
            OnTipsUpdate?.Invoke(this, $"{Name}：选择 {firstAction.Name}\n{(firstAction.Get<E005_CardEventArgs>()).Reason}");
            
            Delay(() => OnTurnCompleted?.Invoke(this));
        }

        public void StartTurn()
        {
            OnTipsUpdate?.Invoke(this, $"{Name}：思考中...");
            OnTurnStart?.Invoke(this);

            _human.HandCards.SensedCard();
            _human.StartTurn(new UnillmStandardHumanStartTurnArgs
            {
                Target = "轮到你的回合了"
            });
        }

        void Delay(Action func)
        {
            IEnumerator InternalDelay()
            {
                yield return new WaitForSeconds(0.1f);
                func?.Invoke();
            }
            StartCoroutine(InternalDelay());
        }
    }
}
