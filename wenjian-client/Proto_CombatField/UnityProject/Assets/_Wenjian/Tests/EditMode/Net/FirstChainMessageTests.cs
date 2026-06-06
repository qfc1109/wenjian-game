using NUnit.Framework;
using Wenjian.Client.Net;

namespace Wenjian.Client.Tests.Net
{
    public sealed class FirstChainMessageTests
    {
        [Test]
        public void ParsesLoginReplyFieldsWithoutChangingServerValues()
        {
            var ok = FirstChainMessage.TryParse(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100",
                out var message);

            Assert.That(ok, Is.True);
            Assert.That(message.Type, Is.EqualTo("LOGIN_OK"));
            Assert.That(message.Code, Is.EqualTo("OK"));
            Assert.That(message.GetLong("playerId"), Is.EqualTo(1000001L));
            Assert.That(message.GetInt("regionId"), Is.EqualTo(1001));
            Assert.That(message.GetInt("x"), Is.EqualTo(3200));
            Assert.That(message.GetInt("y"), Is.EqualTo(2400));
            Assert.That(message.GetString("sessionToken"), Is.EqualTo("local-dev"));
            Assert.That(message.GetLong("serverTimeMs"), Is.EqualTo(1700000000100L));
        }

        [Test]
        public void KeepsRegionSnapshotAsSummaryWhenServerDoesNotReturnEntityDetails()
        {
            var ok = FirstChainMessage.TryParse(
                "type=REGION_SNAPSHOT code=OK regionId=1001 self=1000001 entities=2 serverTick=1",
                out var message);

            Assert.That(ok, Is.True);
            Assert.That(message.Type, Is.EqualTo("REGION_SNAPSHOT"));
            Assert.That(message.GetLong("self"), Is.EqualTo(1000001L));
            Assert.That(message.GetInt("entities"), Is.EqualTo(2));
            Assert.That(message.GetLong("serverTick"), Is.EqualTo(1L));
            Assert.That(message.Has("targetId"), Is.False);
            Assert.That(message.Has("visualId"), Is.False);
        }

        [Test]
        public void RejectsPayloadsWithoutTypeField()
        {
            var ok = FirstChainMessage.TryParse("code=OK playerId=1000001", out _);

            Assert.That(ok, Is.False);
        }
    }
}
