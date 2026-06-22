using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class LocalPlayerMotorTests
    {
        [Test]
        public void ApplyMovementNormalizesDiagonalInput()
        {
            var actor = new GameObject("player");
            var motor = actor.AddComponent<LocalPlayerMotor>();
            motor.MoveSpeedWorldUnitsPerSecond = 5f;

            motor.ApplyMovement(new Vector2(1f, 1f), 1f);

            const float expected = 3.5355f;
            Assert.That(actor.transform.position.x, Is.EqualTo(expected).Within(0.001f));
            Assert.That(actor.transform.position.y, Is.EqualTo(expected).Within(0.001f));
            Assert.That(motor.LastMoveDirection.x, Is.EqualTo(0.7071f).Within(0.001f));
            Assert.That(motor.LastMoveDirection.y, Is.EqualTo(0.7071f).Within(0.001f));
        }

        [Test]
        public void ApplyMovementKeepsLastDirectionWhenInputIsZero()
        {
            var actor = new GameObject("player");
            var motor = actor.AddComponent<LocalPlayerMotor>();

            motor.ApplyMovement(Vector2.right, 0.1f);
            motor.ApplyMovement(Vector2.zero, 1f);

            Assert.That(actor.transform.position.x, Is.EqualTo(0.4f).Within(0.001f));
            Assert.That(actor.transform.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(motor.LastMoveDirection, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void ApplyMovementClampsToConfiguredWorldBounds()
        {
            var actor = new GameObject("player");
            actor.transform.position = new Vector3(0.85f, 0.9f, 0f);
            var motor = actor.AddComponent<LocalPlayerMotor>();
            motor.MoveSpeedWorldUnitsPerSecond = 5f;
            motor.SetWorldBounds(new Vector2(-1f, -1f), new Vector2(1f, 1f));

            motor.ApplyMovement(new Vector2(1f, 1f), 1f);

            Assert.That(actor.transform.position.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(actor.transform.position.y, Is.EqualTo(1f).Within(0.001f));
            Assert.That(motor.LastMoveDirection.x, Is.EqualTo(0.7071f).Within(0.001f));
            Assert.That(motor.LastMoveDirection.y, Is.EqualTo(0.7071f).Within(0.001f));
            Assert.That(motor.ConstrainToWorldBounds, Is.True);
        }

        [Test]
        public void ApplyMovementUpdatesVisualAnimatorWithMovingAndIdleState()
        {
            var actor = new GameObject("player");
            var motor = actor.AddComponent<LocalPlayerMotor>();
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            renderer.transform.SetParent(actor.transform);
            var visualAnimator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            visualAnimator.SpriteRenderer = renderer;
            visualAnimator.AnimationSet = CreateAnimationSet();
            motor.VisualAnimator = visualAnimator;

            motor.ApplyMovement(Vector2.left, 0.1f);

            Assert.That(renderer.sprite.name, Is.EqualTo("WalkLeftA"));

            motor.ApplyMovement(Vector2.zero, 0.1f);

            Assert.That(renderer.sprite.name, Is.EqualTo("IdleLeft"));
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
