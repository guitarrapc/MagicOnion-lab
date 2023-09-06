#nullable enable
using MagicOnionLab.Unity.Infrastructures.Loggers;
using MagicOnionLab.Unity.Services;
using MagicOnionLab.Unity.Views;
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
                _mathServiceComponentView.RegisterClickEvent(async () =>
                {
                    var mathResult = await mathClient.RequestMpoAsync(_mathServiceComponentView.X, _mathServiceComponentView.Y);
                    _mathServiceComponentView.SetResult(mathResult);
                });
            }
            else
            {
                for (var i = 0; i < 5; i++)
                {
                    var x = Random.Range(10, 9999);
                    var y = Random.Range(10, 9999);
                    _ = await mathClient.RequestMpoAsync(x, y);
                }
            }
        }
    }
}
