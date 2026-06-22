using System;
using UnityEngine;

namespace Wenjian.Client.Presentation
{
    public enum TrainingEnemyAiState
    {
        Idle,
        Chase,
        Attack,
        Blocked
    }

    public readonly struct TrainingEnemyAttackEvent
    {
        public TrainingEnemyAttackEvent(Transform source, Transform target, Vector2 direction, int damage)
        {
            Source = source;
            Target = target;
            Direction = direction;
            Damage = damage;
        }

        public Transform Source { get; }

        public Transform Target { get; }

        public Vector2 Direction { get; }

        public int Damage { get; }
    }

    public sealed class TrainingEnemyMotor : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        [SerializeField]
        private float moveSpeedWorldUnitsPerSecond = 1.35f;

        [SerializeField]
        private float aggroRangeWorldUnits = 5.5f;

        [SerializeField]
        private float stopDistanceWorldUnits = 0.9f;

        [SerializeField]
        private float attackRangeWorldUnits = 1.1f;

        [SerializeField]
        private float attackCooldownSeconds = 0.8f;

        [SerializeField]
        private int attackDamage = 6;

        [SerializeField]
        private Transform attackRangePreview;

        [SerializeField]
        private float collisionPaddingWorldUnits = 0.18f;

        [SerializeField]
        private bool constrainToWorldBounds;

        [SerializeField]
        private Vector2 worldBoundsMin = new(-1000f, -1000f);

        [SerializeField]
        private Vector2 worldBoundsMax = new(1000f, 1000f);

        [SerializeField]
        private Rect[] obstacleRects = Array.Empty<Rect>();

        [SerializeField]
        private TextMesh stateText;

        private TrainingEnemyAiState currentState = TrainingEnemyAiState.Idle;
        private Vector2 lastMoveDirection = Vector2.left;
        private float attackCooldownRemainingSeconds;

        public event Action<TrainingEnemyAttackEvent> AttackTriggered;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public float MoveSpeedWorldUnitsPerSecond
        {
            get => moveSpeedWorldUnitsPerSecond;
            set => moveSpeedWorldUnitsPerSecond = Mathf.Max(0f, value);
        }

        public float AggroRangeWorldUnits
        {
            get => aggroRangeWorldUnits;
            set => aggroRangeWorldUnits = Mathf.Max(0f, value);
        }

        public float StopDistanceWorldUnits
        {
            get => stopDistanceWorldUnits;
            set => stopDistanceWorldUnits = Mathf.Max(0f, value);
        }

        public float AttackRangeWorldUnits
        {
            get => attackRangeWorldUnits;
            set => attackRangeWorldUnits = Mathf.Max(0f, value);
        }

        public float AttackCooldownSeconds
        {
            get => attackCooldownSeconds;
            set => attackCooldownSeconds = Mathf.Max(0f, value);
        }

        public int AttackDamage
        {
            get => attackDamage;
            set => attackDamage = Mathf.Max(0, value);
        }

        public Transform AttackRangePreview
        {
            get => attackRangePreview;
            set
            {
                attackRangePreview = value;
                SetActive(attackRangePreview, false);
            }
        }

        public float CollisionPaddingWorldUnits
        {
            get => collisionPaddingWorldUnits;
            set => collisionPaddingWorldUnits = Mathf.Max(0f, value);
        }

        public bool ConstrainToWorldBounds
        {
            get => constrainToWorldBounds;
            set => constrainToWorldBounds = value;
        }

        public Vector2 WorldBoundsMin => worldBoundsMin;

        public Vector2 WorldBoundsMax => worldBoundsMax;

        public int ObstacleRectCount => obstacleRects?.Length ?? 0;

        public TextMesh StateText
        {
            get => stateText;
            set
            {
                stateText = value;
                ApplyStateText();
            }
        }

        public TrainingEnemyAiState CurrentState => currentState;

        public Vector2 LastMoveDirection => lastMoveDirection;

        public float AttackCooldownRemainingSeconds => attackCooldownRemainingSeconds;

        private void Awake()
        {
            SetActive(attackRangePreview, false);
            ApplyStateText();
        }

        private void Update()
        {
            TickAI(Time.deltaTime);
        }

        public void TickAI(float deltaTime)
        {
            float safeDelta = Mathf.Max(0f, deltaTime);
            TickAttackCooldown(safeDelta);
            if (target == null || safeDelta <= 0f)
            {
                SetState(TrainingEnemyAiState.Idle);
                SetActive(attackRangePreview, false);
                return;
            }

            Vector2 currentPosition = transform.position;
            Vector2 targetPosition = target.position;
            Vector2 toTarget = targetPosition - currentPosition;
            float distance = toTarget.magnitude;
            if (distance <= attackRangeWorldUnits && distance > Mathf.Epsilon)
            {
                Vector2 attackDirection = toTarget / distance;
                lastMoveDirection = attackDirection;
                UpdateAttackPreview(attackDirection);
                SetState(TrainingEnemyAiState.Attack);
                if (attackCooldownRemainingSeconds <= Mathf.Epsilon && attackDamage > 0)
                {
                    attackCooldownRemainingSeconds = attackCooldownSeconds;
                    AttackTriggered?.Invoke(new TrainingEnemyAttackEvent(transform, target, attackDirection, attackDamage));
                }

                return;
            }

            SetActive(attackRangePreview, false);
            if (moveSpeedWorldUnitsPerSecond <= 0f
                || distance > aggroRangeWorldUnits
                || distance <= stopDistanceWorldUnits
                || distance <= Mathf.Epsilon)
            {
                SetState(TrainingEnemyAiState.Idle);
                return;
            }

            Vector2 desiredDirection = toTarget / distance;
            float stepDistance = Mathf.Min(moveSpeedWorldUnitsPerSecond * safeDelta, distance - stopDistanceWorldUnits);
            if (stepDistance <= Mathf.Epsilon)
            {
                SetState(TrainingEnemyAiState.Idle);
                return;
            }

            Vector2 nextPosition = ClampToWorldBounds(currentPosition + desiredDirection * stepDistance);
            if (IsBlocked(nextPosition))
            {
                if (!TryFindAvoidancePosition(currentPosition, desiredDirection, stepDistance, out nextPosition))
                {
                    SetState(TrainingEnemyAiState.Blocked);
                    return;
                }
            }

            Vector2 delta = nextPosition - currentPosition;
            if (delta.sqrMagnitude > Mathf.Epsilon)
            {
                lastMoveDirection = delta.normalized;
            }

            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
            SetState(TrainingEnemyAiState.Chase);
        }

        public void SetWorldBounds(Vector2 min, Vector2 max)
        {
            worldBoundsMin = new Vector2(Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y));
            worldBoundsMax = new Vector2(Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
            constrainToWorldBounds = true;
            Vector2 clamped = ClampToWorldBounds(transform.position);
            transform.position = new Vector3(clamped.x, clamped.y, transform.position.z);
        }

        public void ClearWorldBounds()
        {
            constrainToWorldBounds = false;
        }

        public void SetObstacleRects(params Rect[] rects)
        {
            obstacleRects = rects == null ? Array.Empty<Rect>() : (Rect[])rects.Clone();
        }

        private bool TryFindAvoidancePosition(Vector2 currentPosition, Vector2 desiredDirection, float stepDistance, out Vector2 nextPosition)
        {
            Vector2 perpendicular = new(-desiredDirection.y, desiredDirection.x);
            Vector2[] directions =
            {
                perpendicular,
                -perpendicular,
                (desiredDirection + perpendicular * 0.75f).normalized,
                (desiredDirection - perpendicular * 0.75f).normalized,
                desiredDirection
            };

            foreach (Vector2 direction in directions)
            {
                if (direction.sqrMagnitude <= Mathf.Epsilon)
                {
                    continue;
                }

                Vector2 candidate = ClampToWorldBounds(currentPosition + direction * stepDistance);
                if (!IsBlocked(candidate))
                {
                    nextPosition = candidate;
                    return true;
                }
            }

            nextPosition = currentPosition;
            return false;
        }

        private Vector2 ClampToWorldBounds(Vector2 position)
        {
            if (!constrainToWorldBounds)
            {
                return position;
            }

            return new Vector2(
                Mathf.Clamp(position.x, worldBoundsMin.x, worldBoundsMax.x),
                Mathf.Clamp(position.y, worldBoundsMin.y, worldBoundsMax.y));
        }

        private bool IsBlocked(Vector2 position)
        {
            if (obstacleRects == null)
            {
                return false;
            }

            foreach (Rect rect in obstacleRects)
            {
                if (Inflate(rect, collisionPaddingWorldUnits).Contains(position))
                {
                    return true;
                }
            }

            return false;
        }

        private void SetState(TrainingEnemyAiState state)
        {
            currentState = state;
            ApplyStateText();
        }

        private void ApplyStateText()
        {
            if (stateText != null)
            {
                stateText.text = currentState.ToString();
            }
        }

        private static Rect Inflate(Rect rect, float amount)
        {
            if (amount <= 0f)
            {
                return rect;
            }

            return new Rect(rect.xMin - amount, rect.yMin - amount, rect.width + amount * 2f, rect.height + amount * 2f);
        }

        private void TickAttackCooldown(float deltaTime)
        {
            if (attackCooldownRemainingSeconds <= 0f)
            {
                return;
            }

            attackCooldownRemainingSeconds = Mathf.Max(0f, attackCooldownRemainingSeconds - deltaTime);
        }

        private void UpdateAttackPreview(Vector2 direction)
        {
            if (attackRangePreview == null)
            {
                return;
            }

            attackRangePreview.gameObject.SetActive(true);
            attackRangePreview.localPosition = new Vector3(direction.x * attackRangeWorldUnits * 0.5f, direction.y * attackRangeWorldUnits * 0.5f, -0.08f);
            attackRangePreview.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            attackRangePreview.localScale = new Vector3(Mathf.Max(0.18f, attackRangeWorldUnits), 0.34f, 1f);
        }

        private static void SetActive(Transform target, bool active)
        {
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }
    }
}
