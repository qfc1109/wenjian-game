using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
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
        public void QueueSkillCommandUsesLoggedInPlayerAndAimDirection()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool queued = controller.QueueSkillCommand(2001, new Vector2Int(1, 0));

            Assert.That(queued, Is.True);
            Assert.That(controller.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("SKILL 1000001 2001 1 0"));
        }

        [Test]
        public void QueueSkillCommandNotifiesOutgoingCommandQueued()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            string notifiedCommand = null;
            controller.OutgoingCommandQueued += command => notifiedCommand = command;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            controller.QueueSkillCommand(2001, new Vector2Int(1, 0));

            Assert.That(notifiedCommand, Is.EqualTo("SKILL 1000001 2001 1 0"));
        }

        [Test]
        public void ApplySkillAndDamageEventsDriveFeedbackAndEnemyHealth()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            var feedback = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            feedback.SwordQi = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            feedback.DamageText = new GameObject("DamageText").AddComponent<TextMesh>();
            controller.FeedbackPresenter = feedback;

            controller.ApplyServerPayload(
                "type=SKILL_EVENT code=OK casterId=1000001 skillId=2001 x=3200 y=2400 aimX=1 aimY=0");
            controller.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-12");

            Assert.That(feedback.LastSkillId, Is.EqualTo(2001));
            Assert.That(feedback.LastDamageDelta, Is.EqualTo(-12));
            Assert.That(controller.CurrentSnapshot.EnemyHp, Is.EqualTo(88));
            Assert.That(presenter.EnemyHealthFill.localScale.x, Is.EqualTo(0.88f).Within(0.001f));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: DAMAGE_EVENT hpDelta=-12"));
        }

        [Test]
        public void ApplyDamageEventToPlayerUpdatesPlayerHealthAndPlaysHitSprite()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            var visualAnimator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            visualAnimator.SpriteRenderer = renderer;
            visualAnimator.AnimationSet = CreateAnimationSet();
            visualAnimator.ApplyMovement(Vector2.left, isMoving: false, deltaTime: 0f);
            controller.PlayerVisualAnimator = visualAnimator;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool applied = controller.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=2000001 targetId=1000001 hpDelta=-18");

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.PlayerHp, Is.EqualTo(82));
            Assert.That(presenter.PlayerHealthFill.localScale.x, Is.EqualTo(0.82f).Within(0.001f));
            Assert.That(renderer.sprite.name, Is.EqualTo("HitLeft"));
        }

        [Test]
        public void ApplyPrototypeEnemyAttackUpdatesPlayerHealthAndRecentEvent()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            var visualAnimator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            visualAnimator.SpriteRenderer = renderer;
            visualAnimator.AnimationSet = CreateAnimationSet();
            controller.PlayerVisualAnimator = visualAnimator;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool applied = controller.ApplyPrototypeEnemyAttack(8);

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.PlayerHp, Is.EqualTo(92));
            Assert.That(presenter.PlayerHealthFill.localScale.x, Is.EqualTo(0.92f).Within(0.001f));
            Assert.That(renderer.sprite.name, Is.EqualTo("HitDown"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ENEMY_ATTACK hpDelta=-8"));
        }

        [Test]
        public void LethalEnemyDamageMarksRoomClearedAndOpensExit()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            var feedback = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            feedback.TargetTransform = new GameObject("EnemyVisual").transform;
            feedback.EnemyStateText = new GameObject("EnemyStateText").AddComponent<TextMesh>();
            var roomPresenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            roomPresenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            roomPresenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            roomPresenter.MonsterMarkerRoot = CreateMonsterMarkers(1);
            roomPresenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            controller.FeedbackPresenter = feedback;
            controller.RogueRoomScenePresenter = roomPresenter;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);
            controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 monsters=1 rewardPoolId=5001 entities=2");

            bool applied = controller.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-100");

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.EnemyHp, Is.EqualTo(0));
            Assert.That(controller.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(0));
            Assert.That(controller.CurrentSnapshot.RogueState, Is.EqualTo("Cleared"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ROOM_CLEARED"));
            Assert.That(roomPresenter.LastRoomState, Is.EqualTo("Cleared"));
            Assert.That(roomPresenter.ExitSeal.gameObject.activeSelf, Is.False);
            Assert.That(feedback.LastEnemyState, Is.EqualTo("Dead"));
        }

        [Test]
        public void PrototypeEnemyWaveRequiresAllEnemiesBeforeRoomCleared()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            controller.ConfigurePrototypeEnemyWave(2, 100);
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);
            controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 monsters=2 rewardPoolId=5001 entities=3");

            controller.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-100");

            Assert.That(controller.CurrentSnapshot.RogueState, Is.EqualTo("Rogue 4001"));
            Assert.That(controller.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(1));
            Assert.That(controller.CurrentSnapshot.EnemyHp, Is.EqualTo(100));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ENEMY_DEFEATED remaining=1"));

            controller.ApplyServerPayload(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000002 hpDelta=-100");

            Assert.That(controller.CurrentSnapshot.RogueState, Is.EqualTo("Cleared"));
            Assert.That(controller.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(0));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ROOM_CLEARED"));
        }

        [Test]
        public void QueueStartRogueCommandUsesLoggedInPlayerAndNotifiesSender()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            string notifiedCommand = null;
            controller.OutgoingCommandQueued += command => notifiedCommand = command;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool queued = controller.QueueStartRogueCommand(4001);

            Assert.That(queued, Is.True);
            Assert.That(notifiedCommand, Is.EqualTo("START_ROGUE 1000001 4001"));
            Assert.That(controller.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("START_ROGUE 1000001 4001"));
        }

        [Test]
        public void ApplyRogueStartUpdatesRoomSummary()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            bool applied = controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=8 rewardPoolId=5001 entities=9");

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.RogueInstanceId, Is.EqualTo(9000001L));
            Assert.That(controller.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(8));
            Assert.That(presenter.RegionText.text, Is.EqualTo("Region 1001  Rogue Rogue 4001"));
            Assert.That(presenter.DebugText.text, Is.EqualTo("Tick 0  Pos 1500,1500  Ent 9"));
            Assert.That(presenter.RogueDetailText.text, Is.EqualTo("Rogue 4001  Inst 9000001  Map 2001  Monsters 8  Pool 5001"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ROGUE_START instance=9000001 monsters=8"));
        }

        [Test]
        public void ApplyRogueStartAndFinishDriveRoomSceneFeedback()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            var roomPresenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            roomPresenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            roomPresenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            roomPresenter.MonsterMarkerRoot = CreateMonsterMarkers(3);
            roomPresenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            controller.RogueRoomScenePresenter = roomPresenter;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);

            controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=3 rewardPoolId=5001 entities=9");

            Assert.That(roomPresenter.LastRoomState, Is.EqualTo("Rogue 4001"));
            Assert.That(roomPresenter.ExitSeal.gameObject.activeSelf, Is.True);
            Assert.That(roomPresenter.MonsterMarkerRoot.GetChild(2).gameObject.activeSelf, Is.True);

            controller.ApplyServerPayload(
                "type=ROGUE_FINISH code=OK instanceId=9000001 success=true itemId=6001 count=3");

            Assert.That(roomPresenter.LastRoomState, Is.EqualTo("Finished"));
            Assert.That(roomPresenter.ExitSeal.gameObject.activeSelf, Is.False);
            Assert.That(roomPresenter.RoomStatusText.text, Is.EqualTo("Reward 6001 x3"));
        }

        [Test]
        public void QueueFinishRogueCommandUsesCurrentInstanceAndNotifiesSender()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            string notifiedCommand = null;
            controller.OutgoingCommandQueued += command => notifiedCommand = command;
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);
            controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=8 rewardPoolId=5001 entities=9");

            bool queued = controller.QueueFinishRogueCommand();

            Assert.That(queued, Is.True);
            Assert.That(notifiedCommand, Is.EqualTo("FINISH_ROGUE 1000001 9000001"));
            Assert.That(controller.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("FINISH_ROGUE 1000001 9000001"));
        }

        [Test]
        public void ApplyRogueFinishUpdatesRewardSummary()
        {
            var presenter = CreatePresenter();
            var controller = CreateController(presenter);
            controller.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            controller.TryDequeueOutgoingCommand(out _);
            controller.ApplyServerPayload(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=8 rewardPoolId=5001 entities=9");

            bool applied = controller.ApplyServerPayload(
                "type=ROGUE_FINISH code=OK instanceId=9000001 success=true itemId=6001 count=3");

            Assert.That(applied, Is.True);
            Assert.That(controller.CurrentSnapshot.RewardItemId, Is.EqualTo(6001));
            Assert.That(controller.CurrentSnapshot.RewardCount, Is.EqualTo(3));
            Assert.That(presenter.RegionText.text, Is.EqualTo("Region 1001  Rogue Finished"));
            Assert.That(presenter.RewardText.text, Is.EqualTo("Reward item 6001 x3"));
            Assert.That(presenter.RecentEventText.text, Is.EqualTo("Event: ROGUE_FINISH item=6001 x3"));
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
            presenter.RogueDetailText = CreateText("Rogue");
            presenter.RewardText = CreateText("Reward");
            presenter.PlayerHealthFill = new GameObject("PlayerHealthFill").transform;
            presenter.EnemyHealthFill = new GameObject("EnemyHealthFill").transform;
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

        private static PlayerSpriteAnimationSet CreateAnimationSet()
        {
            return new PlayerSpriteAnimationSet
            {
                IdleDown = CreateSprite("IdleDown"),
                IdleUp = CreateSprite("IdleUp"),
                IdleLeft = CreateSprite("IdleLeft"),
                IdleRight = CreateSprite("IdleRight"),
                WalkDownA = CreateSprite("WalkDownA"),
                WalkDownB = CreateSprite("WalkDownB"),
                WalkUpA = CreateSprite("WalkUpA"),
                WalkUpB = CreateSprite("WalkUpB"),
                WalkLeftA = CreateSprite("WalkLeftA"),
                WalkLeftB = CreateSprite("WalkLeftB"),
                WalkRightA = CreateSprite("WalkRightA"),
                WalkRightB = CreateSprite("WalkRightB"),
                AttackDown = CreateSprite("AttackDown"),
                AttackUp = CreateSprite("AttackUp"),
                AttackLeft = CreateSprite("AttackLeft"),
                AttackRight = CreateSprite("AttackRight"),
                HitDown = CreateSprite("HitDown"),
                HitUp = CreateSprite("HitUp"),
                HitLeft = CreateSprite("HitLeft"),
                HitRight = CreateSprite("HitRight")
            };
        }

        private static Sprite CreateSprite(string name)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            sprite.name = name;
            return sprite;
        }
    }
}
