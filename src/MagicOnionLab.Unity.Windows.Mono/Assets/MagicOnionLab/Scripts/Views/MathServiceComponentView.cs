#nullable enable
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

        private object _lock = new object();

        public void RegisterClickEvent(UnityAction onClick)
        {
            RequestButton.onClick.AddListener(onClick);
        }
        public void AppendResult(int result)
        {
            if (_resultText is null)
            {
                throw new ArgumentNullException(nameof(_resultText));
            }

            lock (_lock)
            {
                _resultText.text = _resultText.text + $"\n{result}"; // zatsu
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
