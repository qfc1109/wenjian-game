using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class RogueDebugControllerTests
    {
        [Test]
        public void TryStartRogueQueuesStartCommand()
        {
            var hud = CreateLoggedInHudController();
            var controller = new GameObject("RogueDebugController").AddComponent<RogueDebugController>();
            controller.HudController = hud;
            controller.RogueId = 4001;

            bool queued = controller.TryStartRogue();

            Assert.That(queued, Is.True);
            Assert.That(hud.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("START_ROGUE 1000001 4001"));
        }

        [Test]
        public void TryFinishRogueQueuesFinishCommandForCurrentInstance()
        {
            var hud = CreateLoggedInHudController();
            hud.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=8 rewardPoolId=5001 entities=9");
            var controller = new GameObject("RogueDebugController").AddComponent<RogueDebugController>();
            controller.HudController = hud;

            bool queued = controller.TryFinishRogue();

            Assert.That(queued, Is.True);
            Assert.That(hud.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("FINISH_ROGUE 1000001 9000001"));
        }

        private static FirstChainHudController CreateLoggedInHudController()
        {
            var presenter = new GameObject("HUD").AddComponent<PrototypeHudPresenter>();
            presenter.ConnectionText = new GameObject("Connection").AddComponent<TextMesh>();
            presenter.PlayerText = new GameObject("Player").AddComponent<TextMesh>();
            presenter.RegionText = new GameObject("Region").AddComponent<TextMesh>();
            presenter.DebugText = new GameObject("Debug").AddComponent<TextMesh>();
            presenter.RecentEventText = new GameObject("Event").AddComponent<TextMesh>();
            presenter.RogueDetailText = new GameObject("Rogue").AddComponent<TextMesh>();
            presenter.RewardText = new GameObject("Reward").AddComponent<TextMesh>();
            presenter.PlayerHealthFill = new GameObject("PlayerHealthFill").transform;
            presenter.EnemyHealthFill = new GameObject("EnemyHealthFill").transform;

            var hud = new GameObject("FirstChainHudController").AddComponent<FirstChainHudController>();
            hud.HudPresenter = presenter;
            hud.ApplyOfflineSnapshot();
            hud.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            hud.TryDequeueOutgoingCommand(out _);
            return hud;
        }
    }
}
