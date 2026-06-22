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

        [Test]
        public void BuildSkillUsesPlayerSkillAndAimDirection()
        {
            string command = FirstChainCommandBuilder.BuildSkill(1000001L, 2001, 1, 0);

            Assert.That(command, Is.EqualTo("SKILL 1000001 2001 1 0"));
        }

        [Test]
        public void BuildStartRogueUsesPlayerAndRogueId()
        {
            string command = FirstChainCommandBuilder.BuildStartRogue(1000001L, 4001);

            Assert.That(command, Is.EqualTo("START_ROGUE 1000001 4001"));
        }

        [Test]
        public void BuildFinishRogueUsesPlayerAndInstanceId()
        {
            string command = FirstChainCommandBuilder.BuildFinishRogue(1000001L, 9000001L);

            Assert.That(command, Is.EqualTo("FINISH_ROGUE 1000001 9000001"));
        }
    }
}
