using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class PlayerSpriteAnimatorTests
    {
        [Test]
        public void ApplyMovementSelectsDirectionalIdleAndWalkSprites()
        {
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            var animator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            animator.SpriteRenderer = renderer;
            animator.AnimationSet = CreateAnimationSet();
            animator.WalkFrameSeconds = 0.1f;

            animator.ApplyMovement(Vector2.up, isMoving: false, deltaTime: 0f);

            Assert.That(renderer.sprite.name, Is.EqualTo("IdleUp"));

            animator.ApplyMovement(Vector2.right, isMoving: true, deltaTime: 0f);

            Assert.That(renderer.sprite.name, Is.EqualTo("WalkRightA"));

            animator.ApplyMovement(Vector2.right, isMoving: true, deltaTime: 0.11f);

            Assert.That(renderer.sprite.name, Is.EqualTo("WalkRightB"));
        }

        [Test]
        public void AttackAndHitTemporarilyOverrideMovementSprite()
        {
            var renderer = new GameObject("CharacterSprite").AddComponent<SpriteRenderer>();
            var animator = renderer.gameObject.AddComponent<PlayerSpriteAnimator>();
            animator.SpriteRenderer = renderer;
            animator.AnimationSet = CreateAnimationSet();
            animator.AttackHoldSeconds = 0.2f;
            animator.HitHoldSeconds = 0.2f;

            animator.ApplyMovement(Vector2.left, isMoving: false, deltaTime: 0f);
            animator.PlayAttack(Vector2.right);

            Assert.That(renderer.sprite.name, Is.EqualTo("AttackRight"));

            animator.ApplyMovement(Vector2.down, isMoving: true, deltaTime: 0.1f);

            Assert.That(renderer.sprite.name, Is.EqualTo("AttackRight"));

            animator.TickVisual(0.2f);

            Assert.That(renderer.sprite.name, Is.EqualTo("WalkDownA"));

            animator.PlayHit();

            Assert.That(renderer.sprite.name, Is.EqualTo("HitDown"));

            animator.TickVisual(0.2f);

            Assert.That(renderer.sprite.name, Is.EqualTo("WalkDownA"));
        }

        private static PlayerSpriteAnimationSet CreateAnimationSet()
        {
            return new PlayerSpriteAnimationSet
            {
                IdleDown = CreateSprite("IdleDown", Color.blue),
                IdleUp = CreateSprite("IdleUp", Color.cyan),
                IdleLeft = CreateSprite("IdleLeft", Color.green),
                IdleRight = CreateSprite("IdleRight", Color.magenta),
                WalkDownA = CreateSprite("WalkDownA", Color.gray),
                WalkDownB = CreateSprite("WalkDownB", Color.white),
                WalkUpA = CreateSprite("WalkUpA", Color.gray),
                WalkUpB = CreateSprite("WalkUpB", Color.white),
                WalkLeftA = CreateSprite("WalkLeftA", Color.gray),
                WalkLeftB = CreateSprite("WalkLeftB", Color.white),
                WalkRightA = CreateSprite("WalkRightA", Color.gray),
                WalkRightB = CreateSprite("WalkRightB", Color.white),
                AttackDown = CreateSprite("AttackDown", Color.red),
                AttackUp = CreateSprite("AttackUp", Color.red),
                AttackLeft = CreateSprite("AttackLeft", Color.red),
                AttackRight = CreateSprite("AttackRight", Color.red),
                HitDown = CreateSprite("HitDown", Color.yellow),
                HitUp = CreateSprite("HitUp", Color.yellow),
                HitLeft = CreateSprite("HitLeft", Color.yellow),
                HitRight = CreateSprite("HitRight", Color.yellow)
            };
        }

        private static Sprite CreateSprite(string name, Color color)
        {
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    texture.SetPixel(x, y, color);
                }
            }

            texture.Apply();
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            sprite.name = name;
            return sprite;
        }
    }
}
