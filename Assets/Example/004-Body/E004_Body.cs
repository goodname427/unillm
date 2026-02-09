using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace unillm.Example
{
    public class E004_Body : MonoBehaviour, IUnillmSense
    {
        private enum MoveDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        private class MoveArgs : UnillmBodyDoEventArgs
        {
            [UnillmPropmtDescription("要移动的方向")]
            public MoveDirection Direction;

            [UnillmPropmtDescription("往这个方向移动的理由")]
            public string Reason = "";
        }

        private class MoveBody : IUnillmBody<MoveArgs>
        {
            public Transform TransformToMove;

            string IUnillmBody.Name => "Moving";

            string IUnillmBody.Description => "朝某一个方向移动1m。移动前务必考虑与Block的位置";

            bool IUnillmBody<MoveArgs>.Do(MoveArgs eventArgs)
            {
                var translate = Vector2.zero;
                switch (eventArgs.Direction)
                {
                    case MoveDirection.Up:
                        translate = Vector2.up;
                        break;
                    case MoveDirection.Down:
                        translate = Vector2.down;
                        break;
                    case MoveDirection.Left:
                        translate = Vector2.left;
                        break;
                    case MoveDirection.Right:
                        translate = Vector2.right;
                        break;
                }

                TransformToMove.Translate(translate);
                Debug.Log($"移动{translate}，理由是{eventArgs.Reason}");
                // Debug.Log($"移动{translate}");

                return true;
            }
        }

        private class Human : UnillmStandardHuman
        {
            public List<IUnillmBody> InitBodies = new();
            public List<IUnillmSense> InitSenses = new();

            public override string MakeBackground()
            {
                return @$"
在你周围拥有很多Block，每个Block的大小为1mx1m，你的大小也为1mx1m，你不能与任何Block发生交叉，否则游戏结束。你获胜的条件是到达终点。请你获得游戏的胜利
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
                return InitBodies;
            }

            protected override IEnumerable<IUnillmSense> CollectSenses()
            {
                return InitSenses;
            }
        }

        private Human _human;
        public event OnUnillmSensedEventHandler OnSensed;

        public void Start()
        {
            _human = new Human();
            _human.InitBodies.Add(new MoveBody()
            {
                TransformToMove = transform
            });
            _human.InitSenses.Add(this);
            _human.Init();

            _human.OnTurnCompleted += OnTurnCompleted;
            StartCoroutine(StartTurn());
        }

        private IEnumerator StartTurn()
        {
            var sensed = CollectBlockInfo(out bool end);
            if (end)
            {
                Debug.Log("游戏结束");
                yield break;
            }    

            yield return new WaitForSeconds(0.1f);

            Debug.Log($"开始行动，当前信息：\n{sensed}");
            
            OnSensed?.Invoke(this, new UnillmOnSensedEventArgs()
            {
                Sensed = sensed
            });
            _human.StartTurn();
        }

        private string CollectBlockInfo(out bool end)
        {
            bool IsInteractedWith(Vector3 position)
            {
                var delta = transform.position - position;
                if (Mathf.Abs(delta.x) < 1 && Mathf.Abs(delta.y) < 1)
                {
                    return true;
                }

                return false;
            }

            end = false;
            var blocks = FindObjectsOfType<E004_Block>();
            var sensed = new StringBuilder();
            sensed.AppendLine("你周围的Block与你的相对位置(单位为m)：");

            foreach (var block in blocks)
            {
                var delta = block.transform.position - transform.position;
                if (Mathf.Approximately(Mathf.Abs(delta.x), 1) || Mathf.Approximately(Mathf.Abs(delta.x), 1))
                {
                    sensed.AppendLine(delta.ToString());
                }
                if (IsInteractedWith(block.transform.position))
                {
                    end = true;
                }
            }

            sensed.AppendLine("终点与你的相对位置(单位为m):");
            sensed.AppendLine((new Vector3(10, 10) - transform.position).ToString());
            if (IsInteractedWith(new Vector3(10, 10)))
            {
                end = true;
            }

            return sensed.ToString();
        }

        private void OnTurnCompleted(UnillmStandardHuman human, UnillmOnStandardHumanTurnCompletedEventArgs args)
        {
            StartCoroutine(StartTurn());
        }
    }
}
