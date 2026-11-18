using System;
using System.Numerics;

namespace StickyFabric.Core
{
    /// <summary>
    /// Square, subdivided grid in the local XY plane, centred on the origin.
    /// This is the procedural mesh the sticky fabric is made of.
    /// </summary>
    public sealed class FabricGrid
    {
        public const int MinSubdivisions = 1;
        public const int MaxSubdivisions = 128;

        public int Subdivisions { get; }
        public float Size { get; }

        /// <summary>Rest (undeformed) vertex positions in local space.</summary>
        public Vector3[] Vertices { get; }

        /// <summary>Triangle index list, three indices per triangle.</summary>
        public int[] Triangles { get; }

        /// <summary>Texture coordinates in the [0, 1] range.</summary>
        public Vector2[] Uvs { get; }

        private FabricGrid(int subdivisions, float size, Vector3[] vertices, int[] triangles, Vector2[] uvs)
        {
            Subdivisions = subdivisions;
            Size = size;
            Vertices = vertices;
            Triangles = triangles;
            Uvs = uvs;
        }

        public static int VertexCountFor(int subdivisions) => (subdivisions + 1) * (subdivisions + 1);

        public static int TriangleCountFor(int subdivisions) => subdivisions * subdivisions * 2;

        /// <summary>
        /// Builds a grid with <paramref name="subdivisions"/> quads per side.
        /// 10 subdivisions give 121 vertices, 30 give 961 vertices.
        /// </summary>
        public static FabricGrid Create(int subdivisions, float size = 1f)
        {
            if (subdivisions < MinSubdivisions || subdivisions > MaxSubdivisions)
            {
                throw new ArgumentOutOfRangeException(nameof(subdivisions), subdivisions,
                    $"Subdivisions must be between {MinSubdivisions} and {MaxSubdivisions}.");
            }

            if (!(size > 0f) || float.IsInfinity(size))
            {
                throw new ArgumentOutOfRangeException(nameof(size), size, "Size must be a positive, finite number.");
            }

            int rowLength = subdivisions + 1;
            var vertices = new Vector3[VertexCountFor(subdivisions)];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[TriangleCountFor(subdivisions) * 3];

            float step = 1f / subdivisions;

            for (int y = 0; y <= subdivisions; y++)
            {
                for (int x = 0; x <= subdivisions; x++)
                {
                    int index = y * rowLength + x;
                    float u = x * step;
                    float v = y * step;
                    vertices[index] = new Vector3((u - 0.5f) * size, (v - 0.5f) * size, 0f);
                    uvs[index] = new Vector2(u, v);
                }
            }

            int t = 0;
            for (int y = 0; y < subdivisions; y++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    int start = y * rowLength + x;

                    triangles[t++] = start;
                    triangles[t++] = start + 1;
                    triangles[t++] = start + rowLength;

                    triangles[t++] = start + 1;
                    triangles[t++] = start + rowLength + 1;
                    triangles[t++] = start + rowLength;
                }
            }

            return new FabricGrid(subdivisions, size, vertices, triangles, uvs);
        }
    }
}
