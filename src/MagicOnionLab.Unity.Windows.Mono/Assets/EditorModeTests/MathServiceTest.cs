using System.Collections;
using System.Threading.Tasks;
using MagicOnionLab.Unity;
using MagicOnionLab.Unity.Infrastructures.Loggers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace EditoryModeTests
{
    public class MathServiceTest
    {
        private ILogger _logger;

        [OneTimeSetUp]
        public void OneTimeSetUp() => _logger = UnityNullLogger.Instance;

        // A Test behaves as an ordinary method
        [Test]
        public void MathServiceTestSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MathServiceTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }

        [Test]
        public async Task TestAsyncCode()
        {
            var tmp = 0;
            await Task.Delay(1000);
            tmp++;
            await Task.Delay(100);
            Assert.That(tmp, Is.EqualTo(1));
        }

        // Cannot run GrpcRelated.
    }
}
