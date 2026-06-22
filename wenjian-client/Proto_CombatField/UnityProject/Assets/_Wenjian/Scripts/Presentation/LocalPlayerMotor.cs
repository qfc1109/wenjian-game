using UnityEngine;

namespace Wenjian.Client.Presentation
{
    public sealed class LocalPlayerMotor : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeedWorldUnitsPerSecond = 4f;

        [SerializeField]
        private Vector2 lastMoveDirection = Vector2.down;

        [SerializeField]
        private PlayerSpriteAnimator visualAnimator;

        [SerializeField]
        private bool constrainToWorldBounds;

        [SerializeField]
        private Vector2 worldBoundsMin = new(-1000f, -1000f);

        [SerializeField]
        private Vector2 worldBoundsMax = new(1000f, 1000f);

        public float MoveSpeedWorldUnitsPerSecond
        {
            get => moveSpeedWorldUnitsPerSecond;
            set => moveSpeedWorldUnitsPerSecond = Mathf.Max(0f, value);
        }

        public Vector2 LastMoveDirection => lastMoveDirection;

        public bool ConstrainToWorldBounds
        {
            get => constrainToWorldBounds;
            set => constrainToWorldBounds = value;
        }

        public Vector2 WorldBoundsMin => worldBoundsMin;

        public Vector2 WorldBoundsMax => worldBoundsMax;

        public PlayerSpriteAnimator VisualAnimator
        {
            get => visualAnimator;
            set => visualAnimator = value;
        }

        private void Awake()
        {
            visualAnimator ??= GetComponentInChildren<PlayerSpriteAnimator>();
        }

        private void Update()
        {
            ApplyMovement(ReadLegacyMovementInput(), Time.deltaTime);
        }

        public void ApplyMovement(Vector2 input, float deltaTime)
        {
            Vector2 direction = NormalizeInput(input);
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                visualAnimator?.ApplyMovement(lastMoveDirection, isMoving: false, deltaTime: safeDeltaTime);
                return;
            }

            lastMoveDirection = direction;
            Vector3 delta = new Vector3(direction.x, direction.y, 0f) * moveSpeedWorldUnitsPerSecond * safeDeltaTime;
            transform.position += delta;
            ClampToWorldBounds();
            visualAnimator?.ApplyMovement(direction, isMoving: true, deltaTime: safeDeltaTime);
        }

        public void SetWorldBounds(Vector2 min, Vector2 max)
        {
            worldBoundsMin = new Vector2(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
            worldBoundsMax = new Vector2(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
            constrainToWorldBounds = true;
            ClampToWorldBounds();
        }

        public void ClearWorldBounds()
        {
            constrainToWorldBounds = false;
        }

        public void PlayAttack(Vector2 aimDirection)
        {
            visualAnimator?.PlayAttack(aimDirection.sqrMagnitude <= Mathf.Epsilon ? lastMoveDirection : aimDirection);
        }

        public void PlayHit()
        {
            visualAnimator?.PlayHit();
        }

        private static Vector2 NormalizeInput(Vector2 input)
        {
            if (input.sqrMagnitude > 1f)
            {
                return input.normalized;
            }

            return input;
        }

        private void ClampToWorldBounds()
        {
            if (!constrainToWorldBounds)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = Mathf.Clamp(position.x, worldBoundsMin.x, worldBoundsMax.x);
            position.y = Mathf.Clamp(position.y, worldBoundsMin.y, worldBoundsMax.y);
            transform.position = position;
        }

        private static Vector2 ReadLegacyMovementInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }
}
