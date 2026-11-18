using NUnit.Framework;
using UnityEngine;

namespace StickyFabric.Tests
{
    public class StickyFabricMeshTests
    {
        private GameObject fabricObject;

        [SetUp]
        public void SetUp()
        {
            fabricObject = new GameObject("Sticky Fabric (test)");
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(fabricObject);
        }

        [Test]
        public void Rebuild_CreatesProceduralMeshWithDefaultTopology()
        {
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();

            fabric.Rebuild();

            Mesh mesh = fabricObject.GetComponent<MeshFilter>().sharedMesh;
            Assert.IsNotNull(mesh);
            Assert.AreEqual(121, mesh.vertexCount);
            Assert.AreEqual(200 * 3, mesh.triangles.Length);
            Assert.AreEqual(121, fabric.VertexCount);
            Assert.AreEqual(0, fabric.GluedVertexCount);
        }

        [Test]
        public void Subdivisions_ChangingValueRebuildsTheMesh()
        {
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();
            fabric.Rebuild();

            fabric.Subdivisions = 30;

            Assert.AreEqual(961, fabricObject.GetComponent<MeshFilter>().sharedMesh.vertexCount);
            Assert.AreEqual(961, fabric.VertexCount);
        }

        [Test]
        public void Subdivisions_IsClampedToValidRange()
        {
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();
            fabric.Rebuild();

            fabric.Subdivisions = 0;

            Assert.AreEqual(1, fabric.Subdivisions);
            Assert.AreEqual(4, fabric.VertexCount);
        }

        [Test]
        public void Rebuild_AssignsTheMeshToAnExistingMeshCollider()
        {
            var meshCollider = fabricObject.AddComponent<MeshCollider>();
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();

            fabric.Rebuild();

            Assert.AreSame(fabricObject.GetComponent<MeshFilter>().sharedMesh, meshCollider.sharedMesh);
        }

        [Test]
        public void Simulate_WithNothingAround_LeavesTheMeshAtRest()
        {
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();
            fabric.Rebuild();

            fabric.Simulate(1f / 60f);

            Assert.AreEqual(0f, fabric.Solver.MaxDisplacement);
        }
    }
}
