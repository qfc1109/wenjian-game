using UnityEngine;

namespace Wenjian.Client.Presentation
{
    public sealed class LocalPlayerMotor : MonoBehaviour
    {
        [SerializeField]
        private float moveSpeedWorldUnitsPerSecond = 4f;

        [SerializeField]
        private Vector2 lastMoveDirection = Vector2.down;

        public float MoveSpeedWorldUnitsPerSecond
        {
            get => moveSpeedWorldUnitsPerSecond;
            set => moveSpeedWorldUnitsPerSecond = Mathf.Max(0f, value);
        }

        public Vector2 LastMoveDirection => lastMoveDirection;

        private void Update()
        {
            ApplyMovement(ReadLegacyMovementInput(), Time.deltaTime);
        }

        public void ApplyMovement(Vector2 input, float deltaTime)
        {
            Vector2 direction = NormalizeInput(input);
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return;
            }

            lastMoveDirection = direction;
            float safeDeltaTime = Mathf.Max(0f, deltaTime);
            Vector3 delta = new Vector3(direction.x, direction.y, 0f) * moveSpeedWorldUnitsPerSecond * safeDeltaTime;
            transform.position += delta;
        }

        private static Vector2 NormalizeInput(Vector2 input)
        {
            if (input.sqrMagnitude > 1f)
            {
                return input.normalized;
            }

            return input;
        }

        private static Vector2 ReadLegacyMovementInput()
        {
            return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }
    }
}
