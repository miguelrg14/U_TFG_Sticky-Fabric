using System;
using System.Numerics;
using NUnit.Framework;
using StickyFabric.Core;

namespace StickyFabric.Tests.Core
{
    public class FabricGridTests
    {
        [TestCase(1, 4, 2)]
        [TestCase(2, 9, 8)]
        [TestCase(10, 121, 200)]
        [TestCase(30, 961, 1800)]
        public void Create_ProducesExpectedVertexAndTriangleCounts(int subdivisions, int vertices, int triangles)
        {
            FabricGrid grid = FabricGrid.Create(subdivisions);

            Assert.AreEqual(vertices, grid.Vertices.Length);
            Assert.AreEqual(vertices, grid.Uvs.Length);
            Assert.AreEqual(triangles * 3, grid.Triangles.Length);
            Assert.AreEqual(vertices, FabricGrid.VertexCountFor(subdivisions));
            Assert.AreEqual(triangles, FabricGrid.TriangleCountFor(subdivisions));
        }

        [Test]
        public void Create_CentresGridOnOriginInXYPlane()
        {
            FabricGrid grid = FabricGrid.Create(4, 2f);

            float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;
            foreach (Vector3 v in grid.Vertices)
            {
                Assert.AreEqual(0f, v.Z);
                minX = Math.Min(minX, v.X);
                maxX = Math.Max(maxX, v.X);
                minY = Math.Min(minY, v.Y);
                maxY = Math.Max(maxY, v.Y);
            }

            Assert.AreEqual(-1f, minX, 1e-6f);
            Assert.AreEqual(1f, maxX, 1e-6f);
            Assert.AreEqual(-1f, minY, 1e-6f);
            Assert.AreEqual(1f, maxY, 1e-6f);
        }

        [Test]
        public void Create_AllTrianglesAreValidAndShareTheSameWinding()
        {
            FabricGrid grid = FabricGrid.Create(6);
            int[] tris = grid.Triangles;

            float? firstSign = null;
            for (int i = 0; i < tris.Length; i += 3)
            {
                foreach (int index in new[] { tris[i], tris[i + 1], tris[i + 2] })
                {
                    Assert.That(index, Is.InRange(0, grid.Vertices.Length - 1));
                }

                Vector3 a = grid.Vertices[tris[i]];
                Vector3 b = grid.Vertices[tris[i + 1]];
                Vector3 c = grid.Vertices[tris[i + 2]];
                float normalZ = Vector3.Cross(b - a, c - a).Z;

                Assert.AreNotEqual(0f, normalZ, "Degenerate triangle at index " + i);
                float sign = Math.Sign(normalZ);
                firstSign ??= sign;
                Assert.AreEqual(firstSign.Value, sign, "Inconsistent winding at index " + i);
            }
        }

        [Test]
        public void Create_UvsCoverUnitSquare()
        {
            FabricGrid grid = FabricGrid.Create(3);

            Assert.AreEqual(new Vector2(0f, 0f), grid.Uvs[0]);
            Assert.AreEqual(new Vector2(1f, 1f), grid.Uvs[grid.Uvs.Length - 1]);
        }

        [TestCase(0)]
        [TestCase(-3)]
        [TestCase(FabricGrid.MaxSubdivisions + 1)]
        public void Create_RejectsInvalidSubdivisions(int subdivisions)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FabricGrid.Create(subdivisions));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void Create_RejectsInvalidSize(float size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => FabricGrid.Create(4, size));
        }
    }
}
