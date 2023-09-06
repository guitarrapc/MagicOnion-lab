using System.Collections;
using System.Threading.Tasks;
using MagicOnionLab.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlayModeTests
{
    public class MathServiceTest
    {
        private ILogger _logger;

        [OneTimeSetUp]
        public void OneTimeSetUp() => _logger = UnityNullLogger.Instance;

        // A Test behaves as an ordinary method
        [Test]
        public void MathServiceSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator MathServiceWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
