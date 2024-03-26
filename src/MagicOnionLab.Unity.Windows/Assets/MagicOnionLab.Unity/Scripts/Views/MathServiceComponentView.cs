#nullable enable
using MagicOnionLab.Shared.Mpos;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MagicOnionLab.Unity.Views
{
    public class MathServiceComponentView : MonoBehaviour
    {
        public int X => int.Parse(_x?.text ?? throw new ArgumentNullException(nameof(_x)));
        [SerializeField]
        private TMP_InputField? _x = default;

        public int Y => int.Parse(_y?.text ?? throw new ArgumentNullException(nameof(_y)));
        [SerializeField]
        private TMP_InputField? _y = default;

        [SerializeField]
        private TextMeshProUGUI? _resultText = default;

        public Button RequestButton => _requestButton ?? throw new ArgumentNullException(nameof(_requestButton));
        [SerializeField]
        private Button? _requestButton = default;

        [Header("Randomize view values for each execution")]
        [SerializeField]
        private bool _randomValues = false;

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
        public void ExecutionComplete()
        {
            Randomize();
        }

        public void RegisterClickEvent(UnityAction onClick)
        {
            RequestButton.onClick.AddListener(onClick);
        }

        public void AppendResult(MathResultMpo mpo)
        {
            if (_resultText is null)
            {
                throw new ArgumentNullException(nameof(_resultText));
            }

            lock (_lock)
            {
                _resultText.text = $"{(_resultText.text != "" ? $"{_resultText.text}\n" : "")}{mpo.X} + {mpo.Y} = {mpo.Result}"; // zatsu
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
                // roomName
                _x!.text = UnityEngine.Random.Range(10, 9999).ToString();
                _y!.text = UnityEngine.Random.Range(10000, 999999).ToString();
            }
        }
    }
}
