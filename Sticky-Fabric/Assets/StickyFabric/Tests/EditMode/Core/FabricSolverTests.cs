using System;
using System.Numerics;
using NUnit.Framework;
using StickyFabric.Core;

namespace StickyFabric.Tests.Core
{
    public class FabricSolverTests
    {
        private const float Epsilon = 1e-5f;

        private static FabricSolver SingleVertexSolver(float stiffness = 20f, float limit = 0.5f)
        {
            FabricSettings settings = FabricSettings.Default;
            settings.ReturnStiffness = stiffness;
            settings.ElasticLimit = limit;
            return new FabricSolver(new[] { Vector3.Zero }, settings);
        }

        [Test]
        public void Step_WithoutContacts_KeepsEveryVertexAtRest()
        {
            var solver = new FabricSolver(FabricGrid.Create(5), FabricSettings.Default);

            bool changed = solver.Step(1f / 60f, new FixedContactQuery());

            Assert.IsFalse(changed);
            Assert.AreEqual(0f, solver.MaxDisplacement);
            Assert.AreEqual(0, solver.GluedCount);
        }

        [Test]
        public void Step_ContactWithinElasticLimit_GluesVertexToContactPoint()
        {
            FabricSolver solver = SingleVertexSolver(limit: 0.5f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.3f) };

            bool changed = solver.Step(1f / 60f, contact);

            Assert.IsTrue(changed);
            Assert.AreEqual(VertexState.Glued, solver.GetState(0));
            Assert.AreEqual(contact.Point, solver.Positions[0]);
            Assert.AreEqual(1, solver.GluedCount);
        }

        [Test]
        public void Step_GluedVertexFollowsTheMovingContact()
        {
            FabricSolver solver = SingleVertexSolver(limit: 1f);
            var contact = new FixedContactQuery { HasContact = true };

            for (int i = 1; i <= 5; i++)
            {
                contact.Point = new Vector3(0f, 0f, -0.1f * i);
                solver.Step(1f / 60f, contact);
                Assert.AreEqual(contact.Point, solver.Positions[0]);
            }
        }

        [Test]
        public void Step_ContactBeyondElasticLimit_BreaksAndSpringsBack()
        {
            FabricSolver solver = SingleVertexSolver(limit: 0.5f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.4f) };
            solver.Step(1f / 60f, contact);

            contact.Point = new Vector3(0f, 0f, -0.6f);
            solver.Step(1f / 60f, contact);

            Assert.AreEqual(VertexState.Broken, solver.GetState(0));
            float distance = solver.Positions[0].Length();
            Assert.That(distance, Is.LessThan(0.4f), "A broken vertex must move back towards rest.");
            Assert.That(distance, Is.GreaterThan(0f));
        }

        [Test]
        public void Step_BrokenVertex_DoesNotReattachUntilContactIsLost()
        {
            FabricSolver solver = SingleVertexSolver(limit: 0.5f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.4f) };
            solver.Step(1f / 60f, contact);
            contact.Point = new Vector3(0f, 0f, -0.9f);
            solver.Step(1f / 60f, contact);
            Assert.AreEqual(VertexState.Broken, solver.GetState(0));

            // Contact comes back within the limit, but the vertex is still in the same touch: it stays released.
            contact.Point = new Vector3(0f, 0f, -0.2f);
            solver.Step(1f / 60f, contact);
            Assert.AreEqual(VertexState.Broken, solver.GetState(0));

            // Once contact is lost, the next touch can glue it again.
            contact.HasContact = false;
            solver.Step(1f / 60f, contact);
            Assert.AreEqual(VertexState.Returning, solver.GetState(0));

            contact.HasContact = true;
            solver.Step(1f / 60f, contact);
            Assert.AreEqual(VertexState.Glued, solver.GetState(0));
        }

        [Test]
        public void Step_ReleasedVertex_ConvergesExactlyToRest()
        {
            FabricSolver solver = SingleVertexSolver(stiffness: 20f, limit: 1f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0.2f, 0.1f, -0.5f) };
            solver.Step(1f / 60f, contact);

            contact.HasContact = false;
            for (int i = 0; i < 600; i++)
            {
                solver.Step(1f / 60f, contact);
            }

            Assert.AreEqual(Vector3.Zero, solver.Positions[0]);
            Assert.AreEqual(VertexState.Rest, solver.GetState(0));
            Assert.IsFalse(solver.Step(1f / 60f, contact), "A vertex at rest should not report changes.");
        }

        [Test]
        public void Step_ReturnFollowsExponentialDecay()
        {
            const float k = 10f;
            const float start = 0.4f;
            const float dt = 0.05f;
            FabricSolver solver = SingleVertexSolver(stiffness: k, limit: 1f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, start) };
            solver.Step(dt, contact);

            contact.HasContact = false;
            solver.Step(dt, contact);

            float expected = start * MathF.Exp(-k * dt);
            Assert.AreEqual(expected, solver.Positions[0].Z, Epsilon);
        }

        [Test]
        public void Step_ReturnIsIndependentOfFrameRate()
        {
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.45f) };
            FabricSolver slow = SingleVertexSolver();
            FabricSolver fast = SingleVertexSolver();
            slow.Step(0.01f, contact);
            fast.Step(0.01f, contact);
            contact.HasContact = false;

            // 0.1 s simulated as 2 frames at 20 FPS vs 20 frames at 200 FPS.
            for (int i = 0; i < 2; i++)
            {
                slow.Step(0.05f, contact);
            }

            for (int i = 0; i < 20; i++)
            {
                fast.Step(0.005f, contact);
            }

            Assert.AreEqual(slow.Positions[0].Z, fast.Positions[0].Z, 1e-4f);
        }

        [Test]
        public void Step_ZeroDeltaTime_DoesNotMoveReleasedVertices()
        {
            FabricSolver solver = SingleVertexSolver(limit: 1f);
            var contact = new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.3f) };
            solver.Step(0.02f, contact);
            contact.HasContact = false;

            solver.Step(0f, contact);

            Assert.AreEqual(-0.3f, solver.Positions[0].Z, Epsilon);
        }

        [Test]
        public void Reset_PutsEveryVertexBackAtRest()
        {
            FabricSolver solver = SingleVertexSolver(limit: 1f);
            solver.Step(0.02f, new FixedContactQuery { HasContact = true, Point = new Vector3(0f, 0f, -0.3f) });

            solver.Reset();

            Assert.AreEqual(Vector3.Zero, solver.Positions[0]);
            Assert.AreEqual(VertexState.Rest, solver.GetState(0));
        }

        [Test]
        public void SpherePullingAway_StretchesFabricWithinLimitThenFabricRecovers()
        {
            FabricSettings settings = FabricSettings.Default;
            settings.ElasticLimit = 0.5f;
            var solver = new FabricSolver(FabricGrid.Create(10), settings);
            var sphere = new SphereContactQuery(new Vector3(0f, 0f, 0.1f), radius: 0.25f);
            const float dt = 1f / 60f;

            float maxDuringPull = 0f;
            int maxGlued = 0;

            // The sphere sits in the fabric, then moves away along -Z at 1 unit/s for 1.5 s.
            for (int frame = 0; frame < 90; frame++)
            {
                sphere.Center.Z -= dt;
                solver.Step(dt, sphere);
                maxDuringPull = MathF.Max(maxDuringPull, solver.MaxDisplacement);
                maxGlued = Math.Max(maxGlued, solver.GluedCount);

                for (int i = 0; i < solver.VertexCount; i++)
                {
                    if (solver.GetState(i) == VertexState.Glued)
                    {
                        float stretch = Vector3.Distance(solver.Positions[i], solver.GetRestPosition(i));
                        Assert.That(stretch, Is.LessThanOrEqualTo(settings.ElasticLimit + Epsilon),
                            "A glued vertex must never be stretched past the elastic limit.");
                    }
                }
            }

            Assert.That(maxGlued, Is.GreaterThan(0), "The sphere should have glued some vertices.");
            Assert.That(maxDuringPull, Is.GreaterThan(0.1f), "The fabric should have been visibly stretched.");

            for (int frame = 0; frame < 300; frame++)
            {
                solver.Step(dt, sphere);
            }

            Assert.AreEqual(0f, solver.MaxDisplacement, "The fabric should be back at rest.");
            Assert.AreEqual(0, solver.GluedCount);
        }

        [Test]
        public void Constructor_RejectsInvalidArguments()
        {
            Assert.Throws<ArgumentNullException>(() => new FabricSolver((FabricGrid)null, FabricSettings.Default));
            Assert.Throws<ArgumentNullException>(() => new FabricSolver((Vector3[])null, FabricSettings.Default));

            FabricSettings bad = FabricSettings.Default;
            bad.ElasticLimit = -1f;
            Assert.Throws<ArgumentOutOfRangeException>(() => new FabricSolver(FabricGrid.Create(2), bad));
        }

        [Test]
        public void Step_RejectsInvalidArguments()
        {
            FabricSolver solver = SingleVertexSolver();

            Assert.Throws<ArgumentNullException>(() => solver.Step(0.1f, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => solver.Step(-0.1f, new FixedContactQuery()));
            Assert.Throws<ArgumentOutOfRangeException>(() => solver.Step(float.NaN, new FixedContactQuery()));
        }
    }
}
