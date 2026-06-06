using UnityEngine;

namespace Wenjian.Client.Presentation
{
    public sealed class CameraFollow2D : MonoBehaviour
    {
        [SerializeField]
        private Transform target;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        private void LateUpdate()
        {
            SnapToTarget();
        }

        public void SnapToTarget()
        {
            if (target == null)
            {
                return;
            }

            Vector3 position = transform.position;
            position.x = target.position.x;
            position.y = target.position.y;
            transform.position = position;
        }
    }
}
