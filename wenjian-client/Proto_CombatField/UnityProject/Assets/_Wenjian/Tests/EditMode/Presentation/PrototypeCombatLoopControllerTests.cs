using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class PrototypeCombatLoopControllerTests
    {
        [Test]
        public void EnemyAttackEventAppliesPlayerDamageThroughHudController()
        {
            var presenter = new GameObject("HUD").AddComponent<PrototypeHudPresenter>();
            presenter.ConnectionText = new GameObject("Connection").AddComponent<TextMesh>();
            presenter.PlayerText = new GameObject("Player").AddComponent<TextMesh>();
            presenter.RegionText = new GameObject("Region").AddComponent<TextMesh>();
            presenter.DebugText = new GameObject("Debug").AddComponent<TextMesh>();
            presenter.RecentEventText = new GameObject("Event").AddComponent<TextMesh>();
            presenter.PlayerHealthFill = new GameObject("PlayerHealthFill").transform;
            presenter.EnemyHealthFill = new GameObject("EnemyHealthFill").transform;
            var hud = new GameObject("HudController").AddComponent<FirstChainHudController>();
            hud.HudPresenter = presenter;
            hud.ApplyOfflineSnapshot();
            hud.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            hud.TryDequeueOutgoingCommand(out _);

            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var enemy = new GameObject("Enemy");
            enemy.transform.position = new Vector3(0.7f, 0f, 0f);
            var enemyMotor = enemy.AddComponent<TrainingEnemyMotor>();
            enemyMotor.Target = player.transform;
            enemyMotor.AttackRangeWorldUnits = 1f;
            enemyMotor.AttackDamage = 9;
            var loop = new GameObject("Loop").AddComponent<PrototypeCombatLoopController>();
            loop.HudController = hud;
            loop.EnemyMotor = enemyMotor;

            enemyMotor.TickAI(0.1f);

            Assert.That(hud.CurrentSnapshot.PlayerHp, Is.EqualTo(91));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ENEMY_ATTACK hpDelta=-9"));
        }
    }
}
