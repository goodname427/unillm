using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace unillm.Example
{
    /// <summary>
    /// œÚAgent∑¢ÀÕHelloWorld
    /// </summary>
    public class E001_HelloWorld : MonoBehaviour
    {
        [SerializeField]
        private E001_MessagePanel _messagePanelPrefab;
        [SerializeField]
        private VerticalLayoutGroup _messageContainer;
        [SerializeField]
        private TMP_InputField _inputField;
        [SerializeField]
        private Button _sendButton;

        private UnillmCommmonAgent _agent;

        void Start()
        {
            _agent = new();
            _agent.Init();

            _agent.OnReceivedMessage += OnReceivedMessage;

            _sendButton.onClick.AddListener(SendMessage);
            _inputField.onSubmit.AddListener((m) => SendMessage());
        }

        private void PushMessage(UnillmMessage message)
        {
            var msgPanel = Instantiate(_messagePanelPrefab);
            msgPanel.SetMessage(message);
            msgPanel.transform.SetParent(_messageContainer.transform, false);
        }

        private void SendMessage()
        {
            _sendButton.enabled = false;
            _sendButton.GetComponentInChildren<TMP_Text>().text = "Wait..";
            
            var message = UnillmMessage.MakeUserMessage(_inputField.text);
            _inputField.text = string.Empty;

            PushMessage(message);
            _agent.Send(message);
        }

        private void OnReceivedMessage(IUnillmAgent agent, UnillmOnAgentReceivedMessageEventArgs args)
        {
            Debug.Log(args.Message.Content);
            PushMessage(args.Message);

            _sendButton.enabled = true;
            _sendButton.GetComponentInChildren<TMP_Text>().text = "Send";
        }
    }
}
