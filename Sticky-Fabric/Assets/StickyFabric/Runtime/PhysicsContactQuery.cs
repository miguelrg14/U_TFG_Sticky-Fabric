using StickyFabric.Core;
using UnityEngine;

namespace StickyFabric
{
    /// <summary>
    /// Unity physics implementation of <see cref="IContactQuery"/>:
    /// an overlap sphere on each vertex, then the closest surface point of the nearest collider.
    /// </summary>
    internal sealed class PhysicsContactQuery : IContactQuery
    {
        private const int MaxOverlaps = 8;

        private readonly Transform space;
        private readonly Collider[] ownColliders;
        private readonly Collider[] overlaps = new Collider[MaxOverlaps];
        private float radius = 0.03f;
        private int layerMask = 1;

        public PhysicsContactQuery(Transform space, Collider[] ownColliders)
        {
            this.space = space;
            this.ownColliders = ownColliders ?? System.Array.Empty<Collider>();
        }

        public void Configure(float sphereRadius, LayerMask layers)
        {
            radius = sphereRadius;
            layerMask = layers;
        }

        public bool TryGetContact(int vertexIndex, System.Numerics.Vector3 localPosition, out System.Numerics.Vector3 localContactPoint)
        {
            // TransformPoint takes the fabric's rotation and scale into account, not only its position.
            Vector3 worldPosition = space.TransformPoint(localPosition.ToUnity());
            int count = Physics.OverlapSphereNonAlloc(worldPosition, radius, overlaps, layerMask, QueryTriggerInteraction.Ignore);

            bool found = false;
            float bestDistanceSq = float.PositiveInfinity;
            Vector3 bestPoint = worldPosition;

            for (int i = 0; i < count; i++)
            {
                Collider other = overlaps[i];
                if (IsOwnCollider(other))
                {
                    continue;
                }

                Vector3 point = other.ClosestPoint(worldPosition);
                float distanceSq = (point - worldPosition).sqrMagnitude;
                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestPoint = point;
                    found = true;
                }
            }

            localContactPoint = found ? space.InverseTransformPoint(bestPoint).ToNumerics() : localPosition;
            return found;
        }

        private bool IsOwnCollider(Collider candidate)
        {
            for (int i = 0; i < ownColliders.Length; i++)
            {
                if (ownColliders[i] == candidate)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
