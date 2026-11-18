using UnityEngine;

namespace StickyFabric.Demo
{
    /// <summary>Draws a sphere gizmo on an otherwise invisible marker, such as a waypoint.</summary>
    public sealed class PositionGizmo : MonoBehaviour
    {
        [SerializeField] private Color color = Color.white;
        [SerializeField, Min(0f)] private float radius = 0.1f;

        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}
