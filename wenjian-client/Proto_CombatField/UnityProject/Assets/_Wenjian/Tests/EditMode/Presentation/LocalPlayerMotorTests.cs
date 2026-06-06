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
    }
}
