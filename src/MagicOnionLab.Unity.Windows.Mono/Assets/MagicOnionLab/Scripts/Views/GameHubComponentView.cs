#nullable enable
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MagicOnionLab.Unity.Views
{
    public class GameHubComponentView : MonoBehaviour
    {
        public string RoomName => _roomName?.text ?? throw new ArgumentNullException(nameof(_roomName));
        [SerializeField]
        private TMP_InputField? _roomName = default;

        public int UserCount => int.Parse(_userCount?.text ?? throw new ArgumentNullException(nameof(_userCount)));
        [SerializeField]
        private TMP_InputField? _userCount = default;

        public int Capacity => int.Parse(_userCount?.text ?? throw new ArgumentNullException(nameof(_capacity)));
        [SerializeField]
        private TMP_InputField? _capacity = default;

        [SerializeField]
        private TextMeshProUGUI? _resultText = default;

        public Button RequestButton => _requestButton ?? throw new ArgumentNullException(nameof(_requestButton));
        [SerializeField]
        private Button? _requestButton = default;

        private object _lock = new object();

        public void RegisterClickEvent(UnityAction onClick)
        {
            RequestButton.onClick.AddListener(onClick);
        }
        public void AppendResult(string text)
        {
            if (_resultText is null)
            {
                throw new ArgumentNullException(nameof(_resultText));
            }

            lock (_lock)
            {
                _resultText.text = _resultText.text + $"\n{text}"; // zatsu
            }
        }

        public void ClearResult()
        {
            if (_resultText is null)
            {
                throw new ArgumentNullException(nameof(_resultText));
            }
            lock (_lock)
            {
                _resultText.text = "";
            }
        }
    }
}
