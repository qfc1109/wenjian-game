using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class TrainingEnemyMotorTests
    {
        [Test]
        public void TickAIChasesTargetWhenInsideAggroRange()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var enemy = new GameObject("TrainingEnemy");
            enemy.transform.position = new Vector3(3f, 0f, 0f);
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player.transform;
            motor.MoveSpeedWorldUnitsPerSecond = 1f;
            motor.AggroRangeWorldUnits = 5f;
            motor.StopDistanceWorldUnits = 0.5f;

            motor.TickAI(1f);

            Assert.That(enemy.transform.position.x, Is.EqualTo(2f).Within(0.001f));
            Assert.That(enemy.transform.position.y, Is.EqualTo(0f).Within(0.001f));
            Assert.That(motor.CurrentState, Is.EqualTo(TrainingEnemyAiState.Chase));
            Assert.That(motor.LastMoveDirection.x, Is.LessThan(0f));
        }

        [Test]
        public void TickAIStaysIdleWhenTargetIsOutsideAggroRange()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var enemy = new GameObject("TrainingEnemy");
            enemy.transform.position = new Vector3(8f, 0f, 0f);
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player.transform;
            motor.MoveSpeedWorldUnitsPerSecond = 1f;
            motor.AggroRangeWorldUnits = 5f;

            motor.TickAI(1f);

            Assert.That(enemy.transform.position.x, Is.EqualTo(8f).Within(0.001f));
            Assert.That(motor.CurrentState, Is.EqualTo(TrainingEnemyAiState.Idle));
        }

        [Test]
        public void TickAIAvoidsObstacleRectInsteadOfMovingThroughIt()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(-1f, 0f, 0f);
            var enemy = new GameObject("TrainingEnemy");
            enemy.transform.position = new Vector3(1.4f, 0f, 0f);
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player.transform;
            motor.MoveSpeedWorldUnitsPerSecond = 1f;
            motor.AggroRangeWorldUnits = 5f;
            motor.StopDistanceWorldUnits = 0.2f;
            motor.CollisionPaddingWorldUnits = 0f;
            motor.SetObstacleRects(new Rect(0f, -0.5f, 1f, 1f));

            motor.TickAI(1f);

            Vector2 enemyPosition = enemy.transform.position;
            Assert.That(enemyPosition.x, Is.EqualTo(1.4f).Within(0.001f));
            Assert.That(Mathf.Abs(enemyPosition.y), Is.GreaterThan(0.5f));
            Assert.That(new Rect(0f, -0.5f, 1f, 1f).Contains(enemyPosition), Is.False);
            Assert.That(motor.CurrentState, Is.EqualTo(TrainingEnemyAiState.Chase));
        }

        [Test]
        public void TickAIClampsMovementToWorldBounds()
        {
            var player = new GameObject("Player");
            player.transform.position = new Vector3(5f, 0f, 0f);
            var enemy = new GameObject("TrainingEnemy");
            enemy.transform.position = new Vector3(0.8f, 0f, 0f);
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player.transform;
            motor.MoveSpeedWorldUnitsPerSecond = 3f;
            motor.AggroRangeWorldUnits = 8f;
            motor.SetWorldBounds(new Vector2(-1f, -1f), new Vector2(1f, 1f));

            motor.TickAI(1f);

            Assert.That(enemy.transform.position.x, Is.EqualTo(1f).Within(0.001f));
            Assert.That(enemy.transform.position.y, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void TickAITriggersAttackWhenTargetIsInsideAttackRangeAndRespectsCooldown()
        {
            var player = new GameObject("Player");
            player.transform.position = Vector3.zero;
            var enemy = new GameObject("TrainingEnemy");
            enemy.transform.position = new Vector3(0.8f, 0f, 0f);
            var attackPreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var motor = enemy.AddComponent<TrainingEnemyMotor>();
            motor.Target = player.transform;
            motor.AttackRangeWorldUnits = 1f;
            motor.AttackCooldownSeconds = 0.5f;
            motor.AttackDamage = 8;
            motor.AttackRangePreview = attackPreview.transform;
            int attacks = 0;
            TrainingEnemyAttackEvent lastAttack = default;
            motor.AttackTriggered += attack =>
            {
                attacks++;
                lastAttack = attack;
            };

            motor.TickAI(0.1f);
            motor.TickAI(0.1f);

            Assert.That(attacks, Is.EqualTo(1));
            Assert.That(lastAttack.Damage, Is.EqualTo(8));
            Assert.That(lastAttack.Target, Is.EqualTo(player.transform));
            Assert.That(lastAttack.Direction.x, Is.LessThan(0f));
            Assert.That(motor.CurrentState, Is.EqualTo(TrainingEnemyAiState.Attack));
            Assert.That(motor.AttackCooldownRemainingSeconds, Is.GreaterThan(0f));
            Assert.That(attackPreview.activeSelf, Is.True);

            motor.TickAI(0.5f);

            Assert.That(attacks, Is.EqualTo(2));
        }
    }
}
