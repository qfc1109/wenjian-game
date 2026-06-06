using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Net
{
    public sealed class FirstChainHudControllerTests
    {
        [Test]
        public void ApplyLoginOkUpdatesHudAndQueuesEnterRegion()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);

            bool applied = controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");

            Assert.That(applied, Is.True);
            Assert.That(presenter.ConnectionText.text, Is.EqualTo("Conn: Online"));
            Assert.That(presenter.PlayerText.text, Is.EqualTo("Player 1000001  HP 100/100"));
            Assert.That(presenter.RegionText.text, Is.EqualTo("Region 1001  Rogue Field"));
            Assert.That(presenter.DebugText.text, Is.EqualTo("Tick 0  Pos 3200,2400"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: LOGIN_OK"));
            Assert.That(controller.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("ENTER_REGION 1000001 1001"));
        }

        [Test]
        public void ApplyRegionSnapshotUpdatesTickEntitySummaryAndMarksDetailsMissing()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool applied = controller.ApplyServerPayload(
                "type=REGION_SNAPSHOT code=OK regionId=1001 self=1000001 entities=2 serverTick=7");

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.ServerTick, Is.EqualTo(7L));
            Assert.That(controller.CurrentSnapshot.EntityCount, Is.EqualTo(2));
            Assert.That(controller.LastSnapshotMissingEntityDetails, Is.True);
            Assert.That(presenter.DebugText.text, Is.EqualTo("Tick 7  Pos 3200,2400  Ent 2"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: REGION_SNAPSHOT entities=2"));
        }

        [Test]
        public void ApplyInvalidPayloadLeavesHudOffline()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);

            bool applied = controller.ApplyServerPayload("code=OK playerId=1000001");

            Assert.That(applied, Is.False);
            Assert.That(presenter.ConnectionText.text, Is.EqualTo("Conn: Offline"));
            Assert.That(controller.TryDequeueOutgoingCommand(out _), Is.False);
        }

        private static FirstChainHudController CreateController(PrototypeHudPresenter presenter)
        {
            var controller = new GameObject("FirstChainHudController").AddComponent<FirstChainHudController>();
            controller.HudPresenter = presenter;
            controller.DefaultRegionId = 1001;
            controller.ApplyOfflineSnapshot();
            return controller;
        }

        private static PrototypeHudPresenter CreatePresenter()
        {
            var presenter = new GameObject("HUD").AddComponent<PrototypeHudPresenter>();
            presenter.ConnectionText = CreateText("Connection");
            presenter.PlayerText = CreateText("Player");
            presenter.RegionText = CreateText("Region");
            presenter.DebugText = CreateText("Debug");
            presenter.RecentEventText = CreateText("Event");
            presenter.PlayerHealthFill = new GameObject("PlayerHealthFill").transform;
            presenter.EnemyHealthFill = new GameObject("EnemyHealthFill").transform;
            return presenter;
        }

        private static TextMesh CreateText(string name)
        {
            return new GameObject(name).AddComponent<TextMesh>();
        }
    }
}
