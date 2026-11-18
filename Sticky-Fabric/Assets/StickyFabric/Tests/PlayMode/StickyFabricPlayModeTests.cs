using System.Collections;
using NUnit.Framework;
using StickyFabric.Core;
using StickyFabric.Demo;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace StickyFabric.Tests
{
    public class StickyFabricPlayModeTests
    {
        private const float FrameTime = 1f / 60f;

        [UnityTest]
        public IEnumerator SpherePullingAway_StretchesTheFabricAndItRecovers()
        {
            var fabricObject = new GameObject("Sticky Fabric (test)");
            var fabric = fabricObject.AddComponent<StickyFabricMesh>();
            fabric.enabled = false; // Driven manually below for a deterministic test.
            fabric.ElasticLimit = 0.5f;
            fabric.Rebuild();

            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.localScale = Vector3.one * 0.5f; // radius 0.25
            sphere.transform.position = new Vector3(0f, 0f, 0.1f);

            float maxDisplacement = 0f;
            int maxGlued = 0;

            for (int frame = 0; frame < 90; frame++)
            {
                sphere.transform.position += Vector3.back * FrameTime;
                Physics.SyncTransforms();
                fabric.Simulate(FrameTime);

                maxDisplacement = Mathf.Max(maxDisplacement, fabric.Solver.MaxDisplacement);
                maxGlued = Mathf.Max(maxGlued, fabric.GluedVertexCount);

                for (int i = 0; i < fabric.VertexCount; i++)
                {
                    if (fabric.Solver.GetState(i) == VertexState.Glued)
                    {
                        float stretch = System.Numerics.Vector3.Distance(fabric.Solver.Positions[i], fabric.Solver.GetRestPosition(i));
                        Assert.That(stretch, Is.LessThanOrEqualTo(fabric.ElasticLimit + 1e-4f));
                    }
                }

                yield return null;
            }

            Assert.That(maxGlued, Is.GreaterThan(0), "The sphere should glue some vertices.");
            Assert.That(maxDisplacement, Is.GreaterThan(0.1f), "The fabric should be visibly stretched.");

            for (int frame = 0; frame < 300; frame++)
            {
                Physics.SyncTransforms();
                fabric.Simulate(FrameTime);
            }

            Assert.AreEqual(0f, fabric.Solver.MaxDisplacement, "The fabric should be back at rest.");

            // The rendered mesh must match the solver.
            Mesh mesh = fabricObject.GetComponent<MeshFilter>().sharedMesh;
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Assert.AreEqual(fabric.Solver.GetRestPosition(i).ToUnity(), vertices[i]);
            }

            Object.Destroy(sphere);
            Object.Destroy(fabricObject);
        }

        [UnityTest]
        public IEnumerator DemoScene_LoadsAndTheSphereSticksToTheFabric()
        {
            SceneManager.LoadScene("StickyFabricDemo");
            yield return null;
            yield return null;

            var fabric = Object.FindAnyObjectByType<StickyFabricMesh>();
            Assert.IsNotNull(fabric, "The demo scene should contain a sticky fabric.");
            Assert.AreEqual(121, fabric.VertexCount);
            Assert.IsNotNull(Object.FindAnyObjectByType<DemoHud>(), "The demo HUD should spawn in the demo scene.");
            Assert.IsNotNull(Object.FindAnyObjectByType<PingPongMover>(), "The demo scene should contain the moving sphere.");

            // The sphere starts 1.5 units from the fabric and moves at 1 unit/s.
            float timeout = Time.time + 10f;
            int maxGlued = 0;
            while (Time.time < timeout && maxGlued == 0)
            {
                maxGlued = Mathf.Max(maxGlued, fabric.GluedVertexCount);
                yield return null;
            }

            Assert.That(maxGlued, Is.GreaterThan(0), "The moving sphere should stick to the fabric.");
        }
    }
}
