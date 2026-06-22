using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class PrototypeRogueRewardControllerTests
    {
        [Test]
        public void ShowsRewardChoicesWhenPlayerNearClearedExit()
        {
            var fixture = CreateFixture();
            fixture.Room.ApplyRoomCleared();
            fixture.Player.position = fixture.ExitPoint.position + new Vector3(0.3f, 0f, 0f);

            fixture.Controller.TickInteraction();

            Assert.That(fixture.Controller.RewardPanelVisible, Is.True);
            Assert.That(fixture.RewardPanel.gameObject.activeSelf, Is.True);
            Assert.That(fixture.Room.ExitPromptText.gameObject.activeSelf, Is.True);
            Assert.That(fixture.ChoiceTexts[0].text, Does.Contain("1."));
            Assert.That(fixture.ChoiceTexts[1].text, Does.Contain("2."));
            Assert.That(fixture.ChoiceTexts[2].text, Does.Contain("3."));
        }

        [Test]
        public void DoesNotShowChoicesBeforeRoomCleared()
        {
            var fixture = CreateFixture();
            fixture.Room.ApplyNextRoom(1, 2);
            fixture.Player.position = fixture.ExitPoint.position;

            fixture.Controller.TickInteraction();

            Assert.That(fixture.Controller.RewardPanelVisible, Is.False);
            Assert.That(fixture.RewardPanel.gameObject.activeSelf, Is.False);
        }

        [Test]
        public void ChooseRewardUpdatesHudAndAdvancesRoom()
        {
            var fixture = CreateFixture();
            fixture.Room.ApplyRoomCleared();
            fixture.Player.position = fixture.ExitPoint.position;
            fixture.Controller.TickInteraction();

            bool chosen = fixture.Controller.ChooseReward(1);

            Assert.That(chosen, Is.True);
            Assert.That(fixture.Controller.CurrentRoomIndex, Is.EqualTo(2));
            Assert.That(fixture.Controller.RewardPanelVisible, Is.False);
            Assert.That(fixture.Hud.CurrentSnapshot.RogueState, Is.EqualTo("Room 2"));
            Assert.That(fixture.Hud.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(3));
            Assert.That(fixture.Hud.CurrentSnapshot.PrototypeRewardSummary, Is.EqualTo("Max HP +20"));
            Assert.That(fixture.Presenter.RewardText.text, Is.EqualTo("Reward Max HP +20"));
            Assert.That(fixture.Presenter.RogueDetailText.text, Does.Contain("Monsters 3"));
        }

        [Test]
        public void AdvanceRoomResetsEnemiesAndExitSeal()
        {
            var fixture = CreateFixture();
            fixture.Enemies[0].gameObject.SetActive(false);
            fixture.Enemies[0].transform.position = new Vector3(9f, 9f, 0f);
            fixture.Hud.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-100");
            fixture.Room.ApplyRoomCleared();
            fixture.Player.position = fixture.ExitPoint.position;
            fixture.Controller.TickInteraction();

            fixture.Controller.ChooseReward(0);

            Assert.That(fixture.Enemies[0].gameObject.activeSelf, Is.True);
            Assert.That(fixture.Enemies[0].transform.position.x, Is.EqualTo(2.8f).Within(0.001f));
            Assert.That(fixture.Enemies[1].gameObject.activeSelf, Is.True);
            Assert.That(fixture.Enemies[2].gameObject.activeSelf, Is.True);
            Assert.That(fixture.Room.LastRoomState, Is.EqualTo("Room 2"));
            Assert.That(fixture.Room.ExitSeal.gameObject.activeSelf, Is.True);
            Assert.That(fixture.Room.ExitPortal.gameObject.activeSelf, Is.False);
            Assert.That(fixture.Room.ExitInteractable, Is.False);
            Assert.That(fixture.Hud.CurrentSnapshot.EnemyHp, Is.EqualTo(100));
        }

        private static RewardFixture CreateFixture()
        {
            var presenter = CreatePresenter();
            var hud = new GameObject("FirstChainHudController").AddComponent<FirstChainHudController>();
            hud.HudPresenter = presenter;
            hud.ApplyOfflineSnapshot();
            hud.ConfigurePrototypeEnemyWave(2, 100);

            var room = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            room.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            room.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            room.ExitPortal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            room.ExitPromptText = new GameObject("ExitPromptText").AddComponent<TextMesh>();
            room.MonsterMarkerRoot = CreateMonsterMarkers(4);
            room.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();

            var player = new GameObject("Player").transform;
            var exitPoint = new GameObject("ExitPoint").transform;
            exitPoint.position = new Vector3(4f, 0f, 0f);
            var rewardPanel = new GameObject("RewardChoicePanel").transform;
            var choiceTexts = new[]
            {
                CreateText("RewardChoice_0"),
                CreateText("RewardChoice_1"),
                CreateText("RewardChoice_2")
            };
            var enemies = new[]
            {
                CreateEnemy("TrainingEnemy_0", new Vector3(2.8f, 0f, 0f), player),
                CreateEnemy("TrainingEnemy_1", new Vector3(4.2f, 1.1f, 0f), player),
                CreateEnemy("TrainingEnemy_2", new Vector3(4.2f, -1.1f, 0f), player)
            };

            var controller = new GameObject("PrototypeRogueRewardController").AddComponent<PrototypeRogueRewardController>();
            controller.Player = player;
            controller.ExitPoint = exitPoint;
            controller.RoomScenePresenter = room;
            controller.HudController = hud;
            controller.RewardPanel = rewardPanel;
            controller.RewardChoiceTexts = choiceTexts;
            controller.ExitInteractDistance = 1.2f;
            controller.ConfigureEnemyWave(enemies);
            controller.StartRoom(1, 2);

            return new RewardFixture(controller, player, exitPoint, rewardPanel, choiceTexts, room, hud, presenter, enemies);
        }

        private static PrototypeHudPresenter CreatePresenter()
        {
            var presenter = new GameObject("HUD").AddComponent<PrototypeHudPresenter>();
            presenter.ConnectionText = CreateText("Connection");
            presenter.PlayerText = CreateText("Player");
            presenter.RegionText = CreateText("Region");
            presenter.DebugText = CreateText("Debug");
            presenter.RecentEventText = CreateText("Event");
            presenter.RogueDetailText = CreateText("Rogue");
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

        private static Transform CreateMonsterMarkers(int count)
        {
            var root = new GameObject("MonsterMarkerRoot").transform;
            for (int i = 0; i < count; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                marker.name = $"MonsterMarker_{i:00}";
                marker.transform.SetParent(root);
            }

            return root;
        }

        private static TrainingEnemyMotor CreateEnemy(string name, Vector3 spawnPosition, Transform player)
        {
            var enemy = new GameObject(name);
            enemy.transform.position = spawnPosition;
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player;
            return motor;
        }

        private readonly struct RewardFixture
        {
            public RewardFixture(
                PrototypeRogueRewardController controller,
                Transform player,
                Transform exitPoint,
                Transform rewardPanel,
                TextMesh[] choiceTexts,
                RogueRoomScenePresenter room,
                FirstChainHudController hud,
                PrototypeHudPresenter presenter,
                TrainingEnemyMotor[] enemies)
            {
                Controller = controller;
                Player = player;
                ExitPoint = exitPoint;
                RewardPanel = rewardPanel;
                ChoiceTexts = choiceTexts;
                Room = room;
                Hud = hud;
                Presenter = presenter;
                Enemies = enemies;
            }

            public PrototypeRogueRewardController Controller { get; }

            public Transform Player { get; }

            public Transform ExitPoint { get; }

            public Transform RewardPanel { get; }

            public TextMesh[] ChoiceTexts { get; }

            public RogueRoomScenePresenter Room { get; }

            public FirstChainHudController Hud { get; }

            public PrototypeHudPresenter Presenter { get; }

            public TrainingEnemyMotor[] Enemies { get; }
        }
    }
}
