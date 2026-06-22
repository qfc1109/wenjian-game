using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;
using Wenjian.Client.UI;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class SkillIntentControllerTests
    {
        [Test]
        public void TryCastSkillQueuesCommandWithDiscreteAimDirection()
        {
            var hud = CreateLoggedInHudController();
            var controller = new GameObject("SkillIntentController").AddComponent<SkillIntentController>();
            controller.HudController = hud;
            controller.SkillId = 2001;

            bool queued = controller.TryCastSkill(new Vector2(0.8f, 0.1f));

            Assert.That(queued, Is.True);
            Assert.That(hud.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("SKILL 1000001 2001 1 0"));
        }

        [Test]
        public void TryCastSkillAppliesLocalPrototypeDamageWhenOffline()
        {
            var hud = CreateOfflineHudController();
            hud.ConfigurePrototypeEnemyWave(2, 100);
            var controller = new GameObject("SkillIntentController").AddComponent<SkillIntentController>();
            controller.HudController = hud;
            controller.SkillId = 2001;
            controller.CooldownSeconds = 0f;
            controller.PrototypeLocalSkillDamage = 50;

            bool cast = controller.TryCastSkill(Vector2.right);

            Assert.That(cast, Is.True);
            Assert.That(hud.TryDequeueOutgoingCommand(out _), Is.False);
            Assert.That(hud.CurrentSnapshot.EnemyHp, Is.EqualTo(50));
            Assert.That(hud.CurrentSnapshot.RogueMonsterCount, Is.EqualTo(2));
            Assert.That(hud.CurrentSnapshot.RecentEvent, Is.EqualTo("DAMAGE_EVENT hpDelta=-50"));
        }

        [Test]
        public void ToDiscreteAimFallsBackToDownWhenDirectionIsZero()
        {
            Vector2Int aim = SkillIntentController.ToDiscreteAim(Vector2.zero);

            Assert.That(aim, Is.EqualTo(new Vector2Int(0, -1)));
        }

        [Test]
        public void TryCastSkillPlaysAttackSpriteWhenCommandIsQueued()
        {
            var hud = CreateLoggedInHudController();
            var player = new GameObject("Player");
            var motor = player.AddComponent<LocalPlayerMotor>();
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(player.transform);
            var visualAnimator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            visualAnimator.SpriteRenderer = renderer;
            visualAnimator.AnimationSet = CreateAnimationSet();
            motor.VisualAnimator = visualAnimator;

            var controller = new GameObject("SkillIntentController").AddComponent<SkillIntentController>();
            controller.HudController = hud;
            controller.PlayerMotor = motor;
            controller.SkillId = 2001;

            bool queued = controller.TryCastSkill(Vector2.right);

            Assert.That(queued, Is.True);
            Assert.That(renderer.sprite.name, Is.EqualTo("AttackRight"));
        }

        [Test]
        public void TryCastSkillStartsCooldownAndBlocksRepeatedCasts()
        {
            var hud = CreateLoggedInHudController();
            var cooldownText = new GameObject("SkillCooldownText").AddComponent<TextMesh>();
            var controller = new GameObject("SkillIntentController").AddComponent<SkillIntentController>();
            controller.HudController = hud;
            controller.SkillId = 2001;
            controller.CooldownSeconds = 0.5f;
            controller.CooldownText = cooldownText;

            bool firstQueued = controller.TryCastSkill(Vector2.right);
            bool secondQueued = controller.TryCastSkill(Vector2.right);

            Assert.That(firstQueued, Is.True);
            Assert.That(secondQueued, Is.False);
            Assert.That(controller.CooldownRemainingSeconds, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(cooldownText.text, Is.EqualTo("Skill CD 0.5s"));
            Assert.That(hud.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("SKILL 1000001 2001 1 0"));
            Assert.That(hud.TryDequeueOutgoingCommand(out _), Is.False);
        }

        [Test]
        public void TickCooldownAllowsSkillAfterCooldownEnds()
        {
            var hud = CreateLoggedInHudController();
            var cooldownText = new GameObject("SkillCooldownText").AddComponent<TextMesh>();
            var controller = new GameObject("SkillIntentController").AddComponent<SkillIntentController>();
            controller.HudController = hud;
            controller.SkillId = 2001;
            controller.CooldownSeconds = 0.25f;
            controller.CooldownText = cooldownText;

            Assert.That(controller.TryCastSkill(Vector2.up), Is.True);
            hud.TryDequeueOutgoingCommand(out _);

            controller.TickCooldown(0.25f);

            Assert.That(controller.CooldownRemainingSeconds, Is.EqualTo(0f).Within(0.001f));
            Assert.That(cooldownText.text, Is.EqualTo("Skill Ready"));
            Assert.That(controller.TryCastSkill(Vector2.up), Is.True);
            Assert.That(hud.TryDequeueOutgoingCommand(out string command), Is.True);
            Assert.That(command, Is.EqualTo("SKILL 1000001 2001 0 1"));
        }

        private static FirstChainHudController CreateLoggedInHudController()
        {
            var hud = CreateOfflineHudController();
            hud.ApplyServerPayload(
                "type=LOGIN_OK code=OK playerId=1000001 regionId=1001 x=3200 y=2400 sessionToken=local-dev serverTimeMs=1700000000100");
            hud.TryDequeueOutgoingCommand(out _);
            return hud;
        }

        private static FirstChainHudController CreateOfflineHudController()
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
            hud.ApplyPrototypeNextRoom(1, 2);
            return hud;
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
