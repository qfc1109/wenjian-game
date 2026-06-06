using NUnit.Framework;
using Wenjian.Client.Net;

namespace Wenjian.Client.Tests.Net
{
    public sealed class FirstChainCommandBuilderTests
    {
        [Test]
        public void BuildLoginUsesKeyAndClientTime()
        {
            string command = FirstChainCommandBuilder.BuildLogin("local-dev-key", 1700000000000L);

            Assert.That(command, Is.EqualTo("LOGIN local-dev-key 1700000000000"));
        }

        [Test]
        public void BuildEnterRegionUsesPlayerAndRegion()
        {
            string command = FirstChainCommandBuilder.BuildEnterRegion(1000001L, 1001);

            Assert.That(command, Is.EqualTo("ENTER_REGION 1000001 1001"));
        }
    }
}
