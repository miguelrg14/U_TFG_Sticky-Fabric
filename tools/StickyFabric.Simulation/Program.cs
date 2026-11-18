using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using StickyFabric.Core;
using StickyFabric.Tests.Core;

namespace StickyFabric.Simulation
{
    /// <summary>
    /// Pulls a sphere out of the sticky fabric and checks that it behaves like gum:
    /// vertices stick, stretch no further than the elastic limit, let go and return to rest.
    /// Usage: dotnet run -- [--csv output-directory]
    /// </summary>
    public static class Program
    {
        private const float FrameTime = 1f / 60f;
        private const float SphereRadius = 0.25f;
        private const float PullSpeed = 1f;

        public static int Main(string[] args)
        {
            string csvDirectory = null;
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "--csv")
                {
                    csvDirectory = args[i + 1];
                }
            }

            bool ok = true;
            foreach (int subdivisions in new[] { 10, 30 })
            {
                ok &= Run(subdivisions, csvDirectory);
            }

            Console.WriteLine(ok ? "Simulation OK" : "Simulation FAILED");
            return ok ? 0 : 1;
        }

        private static bool Run(int subdivisions, string csvDirectory)
        {
            FabricSettings settings = FabricSettings.Default;
            var grid = FabricGrid.Create(subdivisions);
            var solver = new FabricSolver(grid, settings);
            var sphere = new SphereContactQuery(new Vector3(0f, 0f, 0.1f), SphereRadius);

            var timeline = new StringBuilder("time,sphere_z,max_displacement,glued,broken\n");
            var profile = new StringBuilder("time,x,z\n");
            int middleRow = subdivisions / 2;

            float maxDisplacement = 0f;
            int maxGlued = 0;
            bool limitRespected = true;
            var watch = new Stopwatch();
            int frames = 0;

            for (int frame = 0; frame < 240; frame++)
            {
                float time = frame * FrameTime;
                if (frame < 90)
                {
                    sphere.Center.Z -= PullSpeed * FrameTime;
                }

                watch.Start();
                solver.Step(FrameTime, sphere);
                watch.Stop();
                frames++;

                int broken = 0;
                for (int i = 0; i < solver.VertexCount; i++)
                {
                    VertexState state = solver.GetState(i);
                    if (state == VertexState.Broken)
                    {
                        broken++;
                    }

                    if (state == VertexState.Glued &&
                        Vector3.Distance(solver.Positions[i], solver.GetRestPosition(i)) > settings.ElasticLimit + 1e-5f)
                    {
                        limitRespected = false;
                    }
                }

                maxDisplacement = MathF.Max(maxDisplacement, solver.MaxDisplacement);
                maxGlued = Math.Max(maxGlued, solver.GluedCount);
                timeline.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:0.0000},{1:0.0000},{2:0.0000},{3},{4}",
                    time, sphere.Center.Z, solver.MaxDisplacement, solver.GluedCount, broken));

                if (frame % 6 == 0)
                {
                    for (int x = 0; x <= subdivisions; x++)
                    {
                        Vector3 p = solver.Positions[middleRow * (subdivisions + 1) + x];
                        profile.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0:0.0000},{1:0.0000},{2:0.0000}", time, p.X, p.Z));
                    }
                }
            }

            bool recovered = solver.MaxDisplacement == 0f && solver.GluedCount == 0;
            bool stuck = maxGlued > 0 && maxDisplacement > 0.1f;
            double microsecondsPerStep = watch.Elapsed.TotalMilliseconds * 1000.0 / frames;

            Console.WriteLine(string.Format(CultureInfo.InvariantCulture,
                "{0,4} vertices | max stretch {1:0.000} (limit {2:0.00}) | max glued {3,3} | limit respected: {4} | back at rest: {5} | {6:0.0} us/step (solver only)",
                grid.Vertices.Length, maxDisplacement, settings.ElasticLimit, maxGlued, limitRespected, recovered, microsecondsPerStep));

            if (csvDirectory != null)
            {
                Directory.CreateDirectory(csvDirectory);
                File.WriteAllText(Path.Combine(csvDirectory, $"timeline_{grid.Vertices.Length}.csv"), timeline.ToString());
                File.WriteAllText(Path.Combine(csvDirectory, $"profile_{grid.Vertices.Length}.csv"), profile.ToString());
            }

            return stuck && limitRespected && recovered;
        }
    }
}
