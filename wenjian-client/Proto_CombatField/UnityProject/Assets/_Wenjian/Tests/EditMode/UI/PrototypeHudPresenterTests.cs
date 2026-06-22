using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.UI
{
    public sealed class PrototypeHudPresenterTests
    {
        [Test]
        public void ApplySnapshotUpdatesStatusTextDebugTextAndHealthBars()
        {
            var presenter = CreatePresenter();
            var snapshot = new CombatHudSnapshot
            {
                IsConnected = true,
                PlayerId = 1000001,
                RegionId = 1001,
                RogueState = "Field",
                PrototypeRewardSummary = "Sword Intent +10%",
                ServerTick = 42,
                ServerX = 3200,
                ServerY = 2400,
                PlayerHp = 75,
                PlayerMaxHp = 100,
                EnemyHp = 20,
                EnemyMaxHp = 50,
                RecentEvent = "LOGIN_OK"
            };

            presenter.ApplySnapshot(snapshot);

            Assert.That(presenter.ConnectionText.text, Is.EqualTo("Conn: Online"));
            Assert.That(presenter.PlayerText.text, Is.EqualTo("Player 1000001  HP 75/100"));
            Assert.That(presenter.RegionText.text, Is.EqualTo("Region 1001  Rogue Field"));
            Assert.That(presenter.DebugText.text, Is.EqualTo("Tick 42  Pos 3200,2400"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: LOGIN_OK"));
            Assert.That(presenter.RewardText.text, Is.EqualTo("Reward Sword Intent +10%"));
            Assert.That(presenter.PlayerHealthFill.localScale.x, Is.EqualTo(0.75f).Within(0.001f));
            Assert.That(presenter.EnemyHealthFill.localScale.x, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(presenter.EnemyWorldHealthFill.localScale.x, Is.EqualTo(0.4f).Within(0.001f));
        }

        [Test]
        public void ApplySnapshotClampsHealthBarsAndUsesOfflineFallbacks()
        {
            var presenter = CreatePresenter();
            var snapshot = new CombatHudSnapshot
            {
                IsConnected = false,
                PlayerId = 0,
                RegionId = 0,
                RogueState = "",
                ServerTick = 0,
                ServerX = 0,
                ServerY = 0,
                PlayerHp = 120,
                PlayerMaxHp = 100,
                EnemyHp = 5,
                EnemyMaxHp = 0,
                RecentEvent = ""
            };

            presenter.ApplySnapshot(snapshot);

            Assert.That(presenter.ConnectionText.text, Is.EqualTo("Conn: Offline"));
            Assert.That(presenter.RegionText.text, Is.EqualTo("Region --  Rogue --"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: --"));
            Assert.That(presenter.PlayerHealthFill.localScale.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(presenter.EnemyHealthFill.localScale.x, Is.EqualTo(0f).Within(0.001f));
            Assert.That(presenter.EnemyWorldHealthFill.localScale.x, Is.EqualTo(0f).Within(0.001f));
        }

        private static PrototypeHudPresenter CreatePresenter()
        {
            var root = new GameObject("HUD");
            var presenter = root.AddComponent<PrototypeHudPresenter>();
            presenter.ConnectionText = CreateText("Connection");
            presenter.PlayerText = CreateText("Player");
            presenter.RegionText = CreateText("Region");
            presenter.DebugText = CreateText("Debug");
            presenter.RecentEventText = CreateText("Event");
            presenter.RewardText = CreateText("Reward");
            presenter.PlayerHealthFill = new GameObject("PlayerHealthFill").transform;
            presenter.EnemyHealthFill = new GameObject("EnemyHealthFill").transform;
            presenter.EnemyWorldHealthFill = new GameObject("EnemyWorldHealthFill").transform;
            return presenter;
        }

        private static TextMesh CreateText(string name)
        {
            return new GameObject(name).AddComponent<TextMesh>();
        }
    }
}
