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

        [Header("Randomize view values for each execution")]
        [SerializeField]
        private bool _randomValues = false;

        public bool Executing { get => _executing; set => _executing = value; }
        [Header("Indicate currently executing or not")]
        [SerializeField]
        private bool _executing = false;

        private object _lock = new object();

        /// <summary>
        /// Initialize View
        /// </summary>
        public void Initialize()
        {
            ClearResult();
            Randomize();
        }

        /// <summary>
        /// Execute on Complete
        /// </summary>
        public void ExecutionBegin()
        {
            _executing = true;
        }

        /// <summary>
        /// Execute on Complete
        /// </summary>
        public void ExecutionComplete()
        {
            Randomize();
            _executing = false;
        }

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
                _resultText.text = $"{(_resultText.text != "" ? $"{_resultText.text}\n" : "")}{text}"; // zatsu
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

        private void Randomize()
        {
            if (_randomValues)
            {
                // _roomName!.text = RandomAlphabet(4, 6)
                _roomName!.text = RandomUUID(10);
                _userCount!.text = UnityEngine.Random.Range(4, 8).ToString();
                _capacity!.text = _userCount.text;

            }

            // GUID RoomName
            static string RandomUUID(int max)
            {
                var roomName = Guid.NewGuid().ToString();
                return roomName.AsSpan().Slice(0, max).ToString();
            }
            // Alphabet RoomName
            static string RandomAlphabet(int min, int max)
            {
                var roomName = "";
                var roomLength = UnityEngine.Random.Range(min, max);
                for (var i = 0; i < roomLength; i++)
                {
                    // A-Za-z
                    roomName += (char)UnityEngine.Random.Range(65, 122);
                }
                return roomName;
            }
        }
    }
}
