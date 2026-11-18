using System.Numerics;
using StickyFabric.Core;

namespace StickyFabric.Tests.Core
{
    /// <summary>
    /// Analytic stand-in for a sphere collider, mirroring Unity's behaviour:
    /// a vertex inside the sphere gets its own position back, one just outside gets the surface point.
    /// </summary>
    public sealed class SphereContactQuery : IContactQuery
    {
        public Vector3 Center;
        public float Radius;
        public float ProbeRadius;

        public SphereContactQuery(Vector3 center, float radius, float probeRadius = 0.03f)
        {
            Center = center;
            Radius = radius;
            ProbeRadius = probeRadius;
        }

        public bool TryGetContact(int vertexIndex, Vector3 localPosition, out Vector3 localContactPoint)
        {
            Vector3 toVertex = localPosition - Center;
            float distance = toVertex.Length();

            if (distance > Radius + ProbeRadius)
            {
                localContactPoint = localPosition;
                return false;
            }

            localContactPoint = distance <= Radius || distance < 1e-6f
                ? localPosition
                : Center + toVertex / distance * Radius;
            return true;
        }
    }

    /// <summary>Contact query that always reports the same point for every vertex, or nothing.</summary>
    public sealed class FixedContactQuery : IContactQuery
    {
        public bool HasContact;
        public Vector3 Point;

        public bool TryGetContact(int vertexIndex, Vector3 localPosition, out Vector3 localContactPoint)
        {
            localContactPoint = HasContact ? Point : localPosition;
            return HasContact;
        }
    }
}
