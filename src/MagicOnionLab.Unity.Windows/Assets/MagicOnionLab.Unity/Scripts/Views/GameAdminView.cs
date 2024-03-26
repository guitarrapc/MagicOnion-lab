#nullable enable
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MagicOnionLab.Unity.Views
{
    public class GameAdminView : MonoBehaviour
    {
        public Button QuitButton => _quitButton ?? throw new ArgumentNullException(nameof(_quitButton));
        [SerializeField]
        private Button? _quitButton = default;

        public void RegisterQuitGameClickEvent(UnityAction action)
        {
            QuitButton.onClick.AddListener(action);
        }
    }
}
