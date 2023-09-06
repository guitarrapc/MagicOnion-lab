#nullable enable
using UnityEngine;

namespace MagicOnionLab.Unity
{
    public class GameComponent : MonoBehaviour
    {
        private UnityEngine.ILogger _logger = new UnityCustomLogger(true);

        [SerializeField]
        private MathServiceComponentView? _mathServiceComponentView = default;

        private async void Start()
        {
            // MathService
            var mathClient = new MathService(_logger);
            if (_mathServiceComponentView is not null)
            {
                var mathResult = await mathClient.RequestAsync(_mathServiceComponentView.X, _mathServiceComponentView.Y);
                _mathServiceComponentView.SetResult(mathResult);
            }
            else
            {
                var x = Random.Range(10, 9999);
                var y = Random.Range(10, 9999);
                _ = await mathClient.RequestAsync(x, y);
            }
        }
    }
}
