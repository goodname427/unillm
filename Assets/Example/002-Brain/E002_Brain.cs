using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    /// <summary>
    /// 一个简单的计算机大脑
    /// </summary>
    public class E002_Brain : MonoBehaviour
    {
        private class Input
        {
            [UnillmPropmtDescription("题目")]
            public string q = "1 + 1";
        }

        private enum Diffculity
        {
            Easy,
            Middle,
            Hard
        }

        private class Output
        {
            [UnillmPropmtDescription("原本的题目")]
            public string q = "1 + 1";

            [UnillmPropmtDescription("解答")]
            public string s = "2";

            [UnillmPropmtDescription("推导过程")]
            public string p = "推导过程";

            [UnillmPropmtDescription("难易程度")]
            public Diffculity d;

            [UnillmPropmtDescription("是否推荐作为小学一年级的题目")]
            public bool r;

            [UnillmPropmtDescription("适合小学几年级")]
            [UnillmPropmtOption(true, "1", "2", "3", "4", "5", "6", "Beyond the framework")]
            public string h;

            [UnillmPropmtDescription("类似的题目")]
            public Question[] sq = { new() };

            [UnillmPropmtDescription("相似的进阶题目")]
            public Question uq = new();

            public override string ToString()
            {
                return UnillmJsonHelper.ToJson(this);
            }

            public class Question
            {
                [UnillmPropmtDescription("题目")]
                public string q = "1 + 1";

                [UnillmPropmtDescription("难易程度")]
                public Diffculity d;
            }
        }

        [SerializeField]
        private TMP_Text _operationText;
        [SerializeField]
        private TMP_Text _resultText;
        [SerializeField]
        private TMP_InputField _inputField;
        [SerializeField]
        private Button _thinkButton;

        private UnillmBrain<Input, Output> _brain;

        public void Start()
        {
            _brain = new UnillmBrain<Input, Output>("你是一个聪明的计算器，能够按照我的要求正确输出结果。接下来我会给你一个题目，请你返回题目的结果。涉及到文本的地方请使用英语");

            _brain.OnThinkCompleted += OnThinkCompleted;

            _thinkButton.onClick.AddListener(Think);
            _inputField.onSubmit.AddListener((m) => Think());
        }

        private void Think()
        {
            _thinkButton.enabled = false;
            _thinkButton.GetComponentInChildren<TMP_Text>().text = "Wait..";

            var input = new Input { q = _inputField.text };
            _operationText.text = _inputField.text;
            _inputField.text = string.Empty;

            Debug.Log(input.q);
            _brain.Think(input);
        }

        private void OnThinkCompleted(UnillmBrain<Input, Output> brain, UnillmOnBrainThinkCompletedEventArgs<Input, Output> args)
        {
            Debug.Log(args.Output);

            _resultText.text = args.Output.ToString();

            _thinkButton.enabled = true;
            _thinkButton.GetComponentInChildren<TMP_Text>().text = "Think";
        }
    }
}
