using StickyFabric.Core;
using UnityEngine;

namespace StickyFabric
{
    /// <summary>
    /// Creates a procedural, subdivided quad at start-up and makes it behave like chewing gum:
    /// vertices stick to any collider that touches them, stretch with it, and spring back
    /// once they are pulled past the elastic limit or the object leaves.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class StickyFabricMesh : MonoBehaviour
    {
        [Header("Mesh")]
        [Tooltip("Quads per side. 10 subdivisions = 121 vertices, 30 = 961 vertices.")]
        [SerializeField, Range(FabricGrid.MinSubdivisions, 64)] private int subdivisions = 10;

        [Tooltip("Side length of the fabric, in local units.")]
        [SerializeField, Min(0.01f)] private float size = 1f;

        [Header("Contacts")]
        [Tooltip("Radius of the detection sphere placed on every vertex.")]
        [SerializeField, Min(0.001f)] private float contactRadius = 0.03f;

        [Tooltip("Layers the fabric can stick to.")]
        [SerializeField] private LayerMask contactLayers = 1;

        [Header("Elasticity")]
        [Tooltip("How fast released vertices return to rest (1/s).")]
        [SerializeField, Min(0f)] private float returnStiffness = 20f;

        [Tooltip("Maximum stretch, in local units, before a vertex lets go.")]
        [SerializeField, Min(0f)] private float elasticLimit = 0.5f;

        [Header("Gizmos")]
        [SerializeField] private Color gizmoColor = Color.magenta;

        private Mesh mesh;
        private FabricSolver solver;
        private PhysicsContactQuery contactQuery;
        private Vector3[] meshVertices;

        public int Subdivisions
        {
            get => subdivisions;
            set
            {
                int clamped = Mathf.Clamp(value, FabricGrid.MinSubdivisions, FabricGrid.MaxSubdivisions);
                if (clamped == subdivisions)
                {
                    return;
                }

                subdivisions = clamped;
                if (solver != null)
                {
                    Rebuild();
                }
            }
        }

        public float ContactRadius
        {
            get => contactRadius;
            set => contactRadius = Mathf.Max(0.001f, value);
        }

        public float ReturnStiffness
        {
            get => returnStiffness;
            set => returnStiffness = Mathf.Max(0f, value);
        }

        public float ElasticLimit
        {
            get => elasticLimit;
            set => elasticLimit = Mathf.Max(0f, value);
        }

        public LayerMask ContactLayers
        {
            get => contactLayers;
            set => contactLayers = value;
        }

        /// <summary>The simulation behind the mesh. <c>null</c> until the component has started.</summary>
        public FabricSolver Solver => solver;

        public int VertexCount => solver?.VertexCount ?? 0;

        public int GluedVertexCount => solver?.GluedCount ?? 0;

        private void Start()
        {
            Rebuild();
        }

        private void Update()
        {
            Simulate(Time.deltaTime);
        }

        /// <summary>Advances the fabric by <paramref name="deltaTime"/> seconds and uploads the mesh if it changed.</summary>
        public void Simulate(float deltaTime)
        {
            if (solver == null)
            {
                Rebuild();
            }

            solver.Settings = CurrentSettings();
            contactQuery.Configure(contactRadius, contactLayers);

            if (solver.Step(deltaTime, contactQuery))
            {
                UploadVertices();
            }
        }

        /// <summary>Recreates the procedural mesh and resets every vertex to rest.</summary>
        public void Rebuild()
        {
            FabricGrid grid = FabricGrid.Create(subdivisions, size);

            if (mesh == null)
            {
                mesh = new Mesh { name = "Sticky Fabric" };
                mesh.MarkDynamic();
            }
            else
            {
                mesh.Clear();
            }

            meshVertices = new Vector3[grid.Vertices.Length];
            var uvs = new Vector2[grid.Uvs.Length];
            for (int i = 0; i < meshVertices.Length; i++)
            {
                meshVertices[i] = grid.Vertices[i].ToUnity();
                uvs[i] = grid.Uvs[i].ToUnity();
            }

            mesh.SetVertices(meshVertices);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(grid.Triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            GetComponent<MeshFilter>().sharedMesh = mesh;
            if (TryGetComponent(out MeshCollider meshCollider))
            {
                meshCollider.sharedMesh = mesh;
            }

            solver = new FabricSolver(grid, CurrentSettings());
            contactQuery = new PhysicsContactQuery(transform, GetComponents<Collider>());
        }

        /// <summary>Releases every vertex and puts the mesh back at rest.</summary>
        public void ResetFabric()
        {
            if (solver == null)
            {
                return;
            }

            solver.Reset();
            UploadVertices();
        }

        private FabricSettings CurrentSettings()
        {
            FabricSettings settings = FabricSettings.Default;
            settings.ReturnStiffness = returnStiffness;
            settings.ElasticLimit = elasticLimit;
            return settings;
        }

        private void UploadVertices()
        {
            System.Numerics.Vector3[] positions = solver.Positions;
            for (int i = 0; i < positions.Length; i++)
            {
                meshVertices[i] = positions[i].ToUnity();
            }

            // One upload per frame, not one per vertex.
            mesh.SetVertices(meshVertices);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
        }

        private void OnValidate()
        {
            subdivisions = Mathf.Clamp(subdivisions, FabricGrid.MinSubdivisions, FabricGrid.MaxSubdivisions);
            if (Application.isPlaying && solver != null && solver.VertexCount != FabricGrid.VertexCountFor(subdivisions))
            {
                Rebuild();
            }
        }

        private void OnDestroy()
        {
            if (mesh != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(mesh);
                }
                else
                {
                    DestroyImmediate(mesh);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (solver == null)
            {
                return;
            }

            Gizmos.color = gizmoColor;
            System.Numerics.Vector3[] positions = solver.Positions;
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawSphere(transform.TransformPoint(positions[i].ToUnity()), contactRadius);
            }
        }
    }
}
