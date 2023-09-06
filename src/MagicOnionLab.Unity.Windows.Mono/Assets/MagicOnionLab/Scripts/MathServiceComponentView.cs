#nullable enable
using MagicOnionLab.Shared.Mpos;
using System;
using TMPro;
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class MathServiceComponentView : MonoBehaviour
    {
        public int X => int.Parse(_x?.text ?? throw new ArgumentNullException(nameof(_x)));
        [SerializeField]
        private TextMeshProUGUI? _x = default;

        public int Y => int.Parse(_y?.text ?? throw new ArgumentNullException(nameof(_y)));
        [SerializeField]
        private TextMeshProUGUI? _y = default;

        [SerializeField]
        private TMP_InputField? _textField = default;

        public void SetResult(MathResultMpo result)
        {
            if (_textField is null)
            {
                throw new ArgumentNullException(nameof(_textField));
            }

            _textField.text = $"Math Service result: '{result.Result}'";
        }
    }
}
