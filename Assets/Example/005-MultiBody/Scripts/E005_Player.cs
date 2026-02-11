using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace unillm.Example
{
    class E005_Player : MonoBehaviour
    {
        public class Human : UnillmStandardHuman
        {
            public E005_Player Outer;
            public E005_CardManager HandCards { get; } = new();

            public override string MakeBackground()
            {
                return @$"
你将扮演技术高超的“吹牛逼”游戏玩家 {Outer.Name}，在遵守规则的前提下，运用策略与心理博弈争取最终胜利。

你的人设：
{Outer.GetDescription()}

游戏规则：
    牌具：一副52张扑克牌（无大小王）
    目标：每人轮流出牌，最快出完手牌者获胜。
    
    游戏开始：场上所有玩家平分牌堆，获取初始手牌。开始第一名玩家的回合。
    回合开始：
        如果是本轮第一位出牌的玩家：
            1.出牌: 选择任意手牌并叫出点数（点数可与实际出的手牌不同），扣置在桌上。
        如果不是：
            1.出牌：选择任意手牌并叫出本轮点数（点数可与实际出的手牌不同），扣置在桌上。
            2.质疑：掀开上家刚出的牌验证。
            3.过牌：跳过本回合。
    回合结束：如果未发生质疑，则开始下一名玩家的回合。

    质疑的验证判定：
        若所有牌与叫出的点数相同 → 质疑者收取桌上全部牌，本轮结束。
        若任意一张牌与叫出的点数不同 → 被质疑者收取桌上全部牌，本轮结束。
        轮次交接：收牌者成为下一轮起始出牌人。
";
            }

            protected override IUnillmAgent MakeAgent()
            {
                return new UnillmCommmonAgent(Outer.GetModelConfig());
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

            protected override bool CheckArgs(UnillmOnBrainThinkCompletedEventArgs<UnillmStandardHumanInput, UnillmStandardHumanOutput> args, out string reason)
            {
                if (args.Output.Actions.Length != 1)
                {
                    reason = $"一回合你只能执行一个动作(当前动作数量为{args.Output.Actions.Length})";
                    return false;
                }

                return base.CheckArgs(args, out reason);
            }
        }

        public event Action<E005_Player> OnTurnStart; 
        public event Action<E005_Player> OnTurnCompleted;
        public event Action<E005_Player, string> OnTipsUpdate;

        [Header("基础配置")]
        public Sprite Icon;
        public string NickName;

        [Header("Agent配置")]
        public string AgentURL;
        public string AgentKey;
        public string AgentModel;
        public bool AgentEnableThinking;
        [Multiline]
        public string AgentDescription;

        public int No { get; set; }
        public string Name => $"{NickName}(玩家{No})";

        private Human _human;
        public E005_CardManager HandCards => _human.HandCards;

        public void Init()
        {
            _human = new Human
            {
                Outer = this
            };
            _human.Init();

            _human.OnTurnCompleted += OnHumanTurnCompleted;
        }

        private UnillmCommonAgentModelConfig GetModelConfig()
        {
            return new UnillmCommonAgentModelConfig
            {
                URL = AgentURL,
                Key = Environment.GetEnvironmentVariable(AgentKey, EnvironmentVariableTarget.User),
                Model = AgentModel,
                EnableThinking = AgentEnableThinking
            };
        }

        private string GetDescription()
        {
            return AgentDescription;
        }

        private void OnHumanTurnCompleted(UnillmStandardHuman human, UnillmOnStandardHumanTurnCompletedEventArgs args)
        {
            // 判断是否失败并获取对应的失败理由
            var reasonBuilder = new StringBuilder();
            bool failed = false;
            var firstAction = args.ActionExecuteResults.FirstOrDefault();
            if (!args.IsSuccess)
            {
                failed = true;
                reasonBuilder.AppendLine(args.ErrorReason);
            }
            else if (firstAction == null)
            {
                failed = false;
                reasonBuilder.AppendLine("你未选择任何行动");
            }
            else if (!firstAction.IsSuccess)
            {
                failed = true;
                reasonBuilder.AppendLine(firstAction.FailedReson);
            }

            // 如果失败则需要重新进行操作
            if (failed)
            {
                Delay(() => _human.StartTurn(new UnillmStandardHumanStartTurnArgs
                {
                    Target = $"由于以下原因:\n{reasonBuilder}\n请你重新选择操作",
                    OverrideInput = args.Input
                }));

                return;
            }

            OnTipsUpdate?.Invoke(this, $"{Name}：选择 {firstAction.Name}\n说：{firstAction.Get<E005_CardEventArgs>().Say}\n想：{firstAction.Get<E005_CardEventArgs>().Reason}\n");
            E005_GameManager.Instance.Notify(firstAction.Get<E005_CardEventArgs>().Say, Name);
            
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

        private void Delay(Action func)
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
