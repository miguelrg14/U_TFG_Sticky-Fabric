using System.Collections.Generic;
using UnityEngine;

namespace StickyFabric.Demo
{
    /// <summary>
    /// Debug helper for the camera: draws the ray under the mouse and outlines the mesh triangle it hits.
    /// The triangle lookup follows the <c>RaycastHit.triangleIndex</c> example from the Unity documentation.
    /// Lines are drawn with <see cref="Debug.DrawLine(Vector3, Vector3, Color)"/>, so enable Gizmos in the Game view.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class MouseTriangleDebugger : MonoBehaviour
    {
        [SerializeField] private LayerMask raycastLayers = ~0;
        [SerializeField] private Color rayColor = Color.green;
        [SerializeField] private Color triangleColor = Color.white;

        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();
        private Camera cachedCamera;

        private void Awake()
        {
            cachedCamera = GetComponent<Camera>();
        }

        private void Update()
        {
            Ray ray = cachedCamera.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers, QueryTriggerInteraction.Ignore))
            {
                return;
            }

            Debug.DrawLine(ray.origin, hit.point, rayColor);

            if (!(hit.collider is MeshCollider meshCollider) || meshCollider.sharedMesh == null || hit.triangleIndex < 0)
            {
                return;
            }

            Mesh mesh = meshCollider.sharedMesh;
            mesh.GetVertices(vertices);
            mesh.GetTriangles(triangles, 0);

            int first = hit.triangleIndex * 3;
            if (first + 2 >= triangles.Count)
            {
                return;
            }

            Transform hitTransform = hit.collider.transform;
            Vector3 p0 = hitTransform.TransformPoint(vertices[triangles[first]]);
            Vector3 p1 = hitTransform.TransformPoint(vertices[triangles[first + 1]]);
            Vector3 p2 = hitTransform.TransformPoint(vertices[triangles[first + 2]]);

            Debug.DrawLine(p0, p1, triangleColor);
            Debug.DrawLine(p1, p2, triangleColor);
            Debug.DrawLine(p2, p0, triangleColor);
        }
    }
}
