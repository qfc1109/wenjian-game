using UnityEngine;
using Wenjian.Client.Net;
using System;

namespace Wenjian.Client.Presentation
{
    public sealed class PrototypeCombatLoopController : MonoBehaviour
    {
        [SerializeField]
        private TrainingEnemyMotor enemyMotor;

        [SerializeField]
        private TrainingEnemyMotor[] enemyMotors = Array.Empty<TrainingEnemyMotor>();

        [SerializeField]
        private FirstChainHudController hudController;

        private bool subscribed;

        public TrainingEnemyMotor EnemyMotor
        {
            get => enemyMotor;
            set
            {
                if (enemyMotor == value)
                {
                    return;
                }

                Unsubscribe();
                enemyMotor = value;
                enemyMotors = value == null ? Array.Empty<TrainingEnemyMotor>() : new[] { value };
                SubscribeIfActive();
            }
        }

        public TrainingEnemyMotor[] EnemyMotors
        {
            get => enemyMotors;
            set
            {
                Unsubscribe();
                enemyMotors = value ?? Array.Empty<TrainingEnemyMotor>();
                enemyMotor = enemyMotors.Length > 0 ? enemyMotors[0] : null;
                SubscribeIfActive();
            }
        }

        public FirstChainHudController HudController
        {
            get => hudController;
            set => hudController = value;
        }

        public void SetEnemies(params TrainingEnemyMotor[] enemies)
        {
            EnemyMotors = enemies;
        }

        private void OnEnable()
        {
            SubscribeIfActive();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void SubscribeIfActive()
        {
            if (!isActiveAndEnabled || subscribed)
            {
                return;
            }

            if ((enemyMotors == null || enemyMotors.Length == 0) && enemyMotor != null)
            {
                enemyMotors = new[] { enemyMotor };
            }

            if (enemyMotors == null)
            {
                return;
            }

            foreach (TrainingEnemyMotor motor in enemyMotors)
            {
                if (motor != null)
                {
                    motor.AttackTriggered += HandleEnemyAttack;
                }
            }

            subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!subscribed)
            {
                subscribed = false;
                return;
            }

            if (enemyMotors != null)
            {
                foreach (TrainingEnemyMotor motor in enemyMotors)
                {
                    if (motor != null)
                    {
                        motor.AttackTriggered -= HandleEnemyAttack;
                    }
                }
            }

            subscribed = false;
        }

        private void HandleEnemyAttack(TrainingEnemyAttackEvent attackEvent)
        {
            hudController?.ApplyPrototypeEnemyAttack(attackEvent.Damage);
        }
    }
}
