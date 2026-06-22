using System;
using UnityEngine;

namespace Wenjian.Client.Presentation
{
    public enum PlayerVisualDirection
    {
        Down,
        Up,
        Left,
        Right
    }

    public enum PlayerVisualState
    {
        Idle,
        Walk,
        Attack,
        Hit
    }

    [Serializable]
    public sealed class PlayerSpriteAnimationSet
    {
        public Sprite IdleDown;
        public Sprite IdleUp;
        public Sprite IdleLeft;
        public Sprite IdleRight;

        public Sprite WalkDownA;
        public Sprite WalkDownB;
        public Sprite WalkUpA;
        public Sprite WalkUpB;
        public Sprite WalkLeftA;
        public Sprite WalkLeftB;
        public Sprite WalkRightA;
        public Sprite WalkRightB;

        public Sprite AttackDown;
        public Sprite AttackUp;
        public Sprite AttackLeft;
        public Sprite AttackRight;

        public Sprite HitDown;
        public Sprite HitUp;
        public Sprite HitLeft;
        public Sprite HitRight;

        public Sprite GetIdle(PlayerVisualDirection direction)
        {
            return direction switch
            {
                PlayerVisualDirection.Up => IdleUp ?? IdleDown,
                PlayerVisualDirection.Left => IdleLeft ?? IdleDown,
                PlayerVisualDirection.Right => IdleRight ?? IdleDown,
                _ => IdleDown
            } ?? FirstAvailable();
        }

        public Sprite GetWalk(PlayerVisualDirection direction, int frameIndex)
        {
            bool secondFrame = frameIndex % 2 != 0;
            return direction switch
            {
                PlayerVisualDirection.Up => secondFrame ? WalkUpB ?? WalkUpA : WalkUpA ?? WalkUpB,
                PlayerVisualDirection.Left => secondFrame ? WalkLeftB ?? WalkLeftA : WalkLeftA ?? WalkLeftB,
                PlayerVisualDirection.Right => secondFrame ? WalkRightB ?? WalkRightA : WalkRightA ?? WalkRightB,
                _ => secondFrame ? WalkDownB ?? WalkDownA : WalkDownA ?? WalkDownB
            } ?? GetIdle(direction);
        }

        public Sprite GetAttack(PlayerVisualDirection direction)
        {
            return direction switch
            {
                PlayerVisualDirection.Up => AttackUp ?? GetIdle(direction),
                PlayerVisualDirection.Left => AttackLeft ?? GetIdle(direction),
                PlayerVisualDirection.Right => AttackRight ?? GetIdle(direction),
                _ => AttackDown ?? GetIdle(direction)
            };
        }

        public Sprite GetHit(PlayerVisualDirection direction)
        {
            return direction switch
            {
                PlayerVisualDirection.Up => HitUp ?? GetIdle(direction),
                PlayerVisualDirection.Left => HitLeft ?? GetIdle(direction),
                PlayerVisualDirection.Right => HitRight ?? GetIdle(direction),
                _ => HitDown ?? GetIdle(direction)
            };
        }

        private Sprite FirstAvailable()
        {
            return IdleDown ?? IdleUp ?? IdleLeft ?? IdleRight
                ?? WalkDownA ?? WalkDownB ?? WalkUpA ?? WalkUpB
                ?? WalkLeftA ?? WalkLeftB ?? WalkRightA ?? WalkRightB
                ?? AttackDown ?? AttackUp ?? AttackLeft ?? AttackRight
                ?? HitDown ?? HitUp ?? HitLeft ?? HitRight;
        }
    }

    public sealed class PlayerSpriteAnimator : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;

        [SerializeField]
        private PlayerSpriteAnimationSet animationSet = new();

        [SerializeField]
        private float walkFrameSeconds = 0.16f;

        [SerializeField]
        private float attackHoldSeconds = 0.18f;

        [SerializeField]
        private float hitHoldSeconds = 0.24f;

        private PlayerVisualDirection currentDirection = PlayerVisualDirection.Down;
        private PlayerVisualDirection overrideDirection = PlayerVisualDirection.Down;
        private PlayerVisualState overrideState = PlayerVisualState.Idle;
        private bool isMoving;
        private float walkTimer;
        private float overrideTimer;

        public SpriteRenderer SpriteRenderer
        {
            get => spriteRenderer;
            set => spriteRenderer = value;
        }

        public PlayerSpriteAnimationSet AnimationSet
        {
            get => animationSet;
            set => animationSet = value ?? new PlayerSpriteAnimationSet();
        }

        public float WalkFrameSeconds
        {
            get => walkFrameSeconds;
            set => walkFrameSeconds = Mathf.Max(0.01f, value);
        }

        public float AttackHoldSeconds
        {
            get => attackHoldSeconds;
            set => attackHoldSeconds = Mathf.Max(0.01f, value);
        }

        public float HitHoldSeconds
        {
            get => hitHoldSeconds;
            set => hitHoldSeconds = Mathf.Max(0.01f, value);
        }

        public PlayerVisualDirection CurrentDirection => currentDirection;

        public PlayerVisualState CurrentState => overrideTimer > 0f ? overrideState : isMoving ? PlayerVisualState.Walk : PlayerVisualState.Idle;

        private void Awake()
        {
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            ApplyCurrentSprite();
        }

        public void ApplyMovement(Vector2 direction, bool isMoving, float deltaTime)
        {
            float safeDelta = Mathf.Max(0f, deltaTime);
            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                currentDirection = ToVisualDirection(direction);
            }

            if (this.isMoving != isMoving || !isMoving)
            {
                walkTimer = 0f;
            }

            this.isMoving = isMoving;
            if (isMoving && overrideTimer <= 0f)
            {
                walkTimer += safeDelta;
            }

            TickOverrideTimer(safeDelta);
            ApplyCurrentSprite();
        }

        public void PlayAttack(Vector2 direction)
        {
            if (direction.sqrMagnitude > Mathf.Epsilon)
            {
                overrideDirection = ToVisualDirection(direction);
                currentDirection = overrideDirection;
            }
            else
            {
                overrideDirection = currentDirection;
            }

            overrideState = PlayerVisualState.Attack;
            overrideTimer = attackHoldSeconds;
            ApplyCurrentSprite();
        }

        public void PlayHit()
        {
            overrideDirection = currentDirection;
            overrideState = PlayerVisualState.Hit;
            overrideTimer = hitHoldSeconds;
            ApplyCurrentSprite();
        }

        public void TickVisual(float deltaTime)
        {
            float safeDelta = Mathf.Max(0f, deltaTime);
            if (isMoving && overrideTimer <= 0f)
            {
                walkTimer += safeDelta;
            }

            TickOverrideTimer(safeDelta);
            ApplyCurrentSprite();
        }

        public static PlayerVisualDirection ToVisualDirection(Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return PlayerVisualDirection.Down;
            }

            return Mathf.Abs(direction.x) > Mathf.Abs(direction.y)
                ? direction.x < 0f ? PlayerVisualDirection.Left : PlayerVisualDirection.Right
                : direction.y < 0f ? PlayerVisualDirection.Down : PlayerVisualDirection.Up;
        }

        private void TickOverrideTimer(float deltaTime)
        {
            if (overrideTimer <= 0f)
            {
                return;
            }

            overrideTimer -= deltaTime;
            if (overrideTimer <= 0f)
            {
                overrideTimer = 0f;
                overrideState = PlayerVisualState.Idle;
            }
        }

        private void ApplyCurrentSprite()
        {
            if (spriteRenderer == null || animationSet == null)
            {
                return;
            }

            Sprite sprite = CurrentState switch
            {
                PlayerVisualState.Attack => animationSet.GetAttack(overrideDirection),
                PlayerVisualState.Hit => animationSet.GetHit(overrideDirection),
                PlayerVisualState.Walk => animationSet.GetWalk(currentDirection, Mathf.FloorToInt(walkTimer / walkFrameSeconds)),
                _ => animationSet.GetIdle(currentDirection)
            };

            if (sprite != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
    }
}
