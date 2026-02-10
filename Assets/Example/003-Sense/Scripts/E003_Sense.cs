using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace unillm.Example
{
    /// <summary>
    /// Sense示例
    /// </summary>
    public class E003_Sense : MonoBehaviour, IUnillmSense
    {
        private class Input
        {
            [UnillmPropmtDescription("当前时间戳")]
            public List<string> Timestaps = new();
        }

        private class Output
        {
            [UnillmPropmtDescription("结果")]
            public List<string> Result = new();
        }

        private class Human : UnillmCommonHuman<Input, Output>
        {
            private Input _input = new();

            public readonly List<IUnillmSense> UnillmSenses = new();

            protected override IEnumerable<IUnillmBody> CollectBodies()
            {
                return new List<IUnillmBody>();
            }

            protected override IEnumerable<IUnillmSense> CollectSenses()
            {
                return UnillmSenses;
            }

            protected override IUnillmBrain<Input, Output> MakeBrain()
            {
                return new UnillmCommonBrain<Input, Output>(new UnillmCommonBrainInitConfig("你是一个聪明的机器人，能够按照我的要求正确输出结果。接下来我会给你一个时间戳，请你返回时间戳对应的时间"));
            }

            protected override void OnSensed(IUnillmSense sense, UnillmOnSensedEventArgs args)
            {
                _input.Timestaps.Add(args.Sensed);

                // 如果正在思考则等思考完毕后再思考
                if (!Brain.IsThinking)
                {
                    Brain.Think(_input);
                    _input = new Input();
                }
            }

            protected override void OnThinkCompleted(UnillmCommonBrain<Input, Output> brain, UnillmOnBrainThinkCompletedEventArgs<Input, Output> args)
            {
                var builder = new StringBuilder();
                for (int i = 0; i < args.Input.Timestaps.Count; i++)
                {
                    builder.AppendLine($"{args.Input.Timestaps[i]} => {args.Output.Result[i]}");
                }
                Debug.Log(builder);
            }
        }

        public event OnUnillmSensedEventHandler OnSensed;

        private Human _human;

        void Start()
        {
            _human = new Human();
            _human.UnillmSenses.Add(this);
            _human.Init();

            StartCoroutine(Ticker());
        }

        IEnumerator Ticker()
        {
            yield return new WaitForSeconds(1f);

            var args = new UnillmOnSensedEventArgs
            {
                Sensed = DateTime.Now.ToUnixTimestampMs().ToString()
            };
            OnSensed?.Invoke(this, args);

            StartCoroutine(Ticker());
        }
    }
}
