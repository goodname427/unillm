using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    public class E002_Brain : MonoBehaviour
    {
        private class Input
        {
            [UnillmPropmtDescription("��Ŀ")]
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
            [UnillmPropmtDescription("ԭ������Ŀ")]
            public string q = "1 + 1";

            [UnillmPropmtDescription("���")]
            public string s = "2";

            [UnillmPropmtDescription("�Ƶ�����")]
            public string p = "�Ƶ�����";

            [UnillmPropmtDescription("���׳̶�")]
            public Diffculity d;

            [UnillmPropmtDescription("�Ƿ��Ƽ���ΪСѧһ�꼶����Ŀ")]
            public bool r;

            [UnillmPropmtDescription("�ʺ�Сѧ���꼶")]
            [UnillmPropmtOption(true, "1", "2", "3", "4", "5", "6", "Beyond the framework")]
            public string h;

            [UnillmPropmtDescription("���Ƶ���Ŀ")]
            public Question[] sq = { new() };

            [UnillmPropmtDescription("���ƵĽ�����Ŀ")]
            public Question uq = new();

            public override string ToString()
            {
                return UnillmJsonHelper.ToJson(this);
            }

            public class Question
            {
                [UnillmPropmtDescription("��Ŀ")]
                public string q = "1 + 1";

                [UnillmPropmtDescription("���׳̶�")]
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
            _brain = new UnillmBrain<Input, Output>("����һ�������ļ��������ܹ������ҵ�Ҫ����ȷ���������������һ����һ����Ŀ�����㷵����Ŀ�Ľ�����漰���ı��ĵط���ʹ��Ӣ��");

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

        private void OnThinkCompleted(UnillmBrain<Input, Output> brain, UnillmOnBrainThinkCompleteEventArgs<Input, Output> args)
        {
            Debug.Log(args.Output);

            _resultText.text = args.Output.ToString();

            _thinkButton.enabled = true;
            _thinkButton.GetComponentInChildren<TMP_Text>().text = "Think";
        }
    }
}
