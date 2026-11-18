using UnityEngine;

namespace StickyFabric.Demo
{
    /// <summary>
    /// Moves the object back and forth between two points at a constant speed.
    /// Used to push a sphere through the fabric and pull it out again.
    /// </summary>
    public sealed class PingPongMover : MonoBehaviour
    {
        [SerializeField] private Transform pointA;
        [SerializeField] private Transform pointB;
        [SerializeField, Min(0f)] private float speed = 1f;

        private bool movingToB;

        public float Speed
        {
            get => speed;
            set => speed = Mathf.Max(0f, value);
        }

        public void Configure(Transform a, Transform b, float metersPerSecond)
        {
            pointA = a;
            pointB = b;
            Speed = metersPerSecond;
            movingToB = false;
        }

        private void Update()
        {
            if (pointA == null || pointB == null)
            {
                return;
            }

            Vector3 target = movingToB ? pointB.position : pointA.position;
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

            if ((transform.position - target).sqrMagnitude < 1e-8f)
            {
                movingToB = !movingToB;
            }
        }
    }
}
