using System;
using System.Numerics;

namespace StickyFabric.Core
{
    /// <summary>
    /// Per-vertex adhesion and elasticity for the sticky fabric.
    /// <para>
    /// Every vertex is handled independently, without springs between neighbours:
    /// <list type="number">
    /// <item>If an object touches the vertex, the vertex is moved onto the object's closest surface point (glued).</item>
    /// <item>If that point is further than <see cref="FabricSettings.ElasticLimit"/> from the vertex's rest position,
    /// the adhesion breaks and the vertex is released.</item>
    /// <item>A released vertex returns to its rest position following
    /// <c>dx/dt = -k (x - rest)</c>, the thesis formula
    /// <c>force = k (distance - b) direction - b k (v - v_prev)</c> with <c>b = 0</c>.
    /// It is integrated exactly (<c>offset *= e^(-k dt)</c>), so the result does not depend on the frame rate.</item>
    /// </list>
    /// </para>
    /// </summary>
    public sealed class FabricSolver
    {
        private readonly Vector3[] restPositions;
        private readonly Vector3[] positions;
        private readonly VertexState[] states;
        private FabricSettings settings;

        public FabricSolver(FabricGrid grid, FabricSettings settings)
            : this(grid?.Vertices ?? throw new ArgumentNullException(nameof(grid)), settings)
        {
        }

        public FabricSolver(Vector3[] restPositions, FabricSettings settings)
        {
            if (restPositions == null)
            {
                throw new ArgumentNullException(nameof(restPositions));
            }

            settings.Validate();
            this.settings = settings;
            this.restPositions = (Vector3[])restPositions.Clone();
            positions = (Vector3[])restPositions.Clone();
            states = new VertexState[restPositions.Length];
        }

        public int VertexCount => positions.Length;

        /// <summary>Current vertex positions in local space. Read-only for callers; do not modify.</summary>
        public Vector3[] Positions => positions;

        public FabricSettings Settings
        {
            get => settings;
            set
            {
                value.Validate();
                settings = value;
            }
        }

        public Vector3 GetRestPosition(int index) => restPositions[index];

        public VertexState GetState(int index) => states[index];

        /// <summary>Number of vertices currently glued to another object.</summary>
        public int GluedCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < states.Length; i++)
                {
                    if (states[i] == VertexState.Glued)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        /// <summary>Largest distance of any vertex from its rest position.</summary>
        public float MaxDisplacement
        {
            get
            {
                float max = 0f;
                for (int i = 0; i < positions.Length; i++)
                {
                    max = MathF.Max(max, Vector3.Distance(positions[i], restPositions[i]));
                }

                return max;
            }
        }

        /// <summary>Puts every vertex back at rest.</summary>
        public void Reset()
        {
            Array.Copy(restPositions, positions, positions.Length);
            Array.Clear(states, 0, states.Length);
        }

        /// <summary>
        /// Advances the simulation by <paramref name="deltaTime"/> seconds.
        /// </summary>
        /// <returns><c>true</c> if any vertex moved, so the caller knows the mesh must be uploaded.</returns>
        public bool Step(float deltaTime, IContactQuery contacts)
        {
            if (contacts == null)
            {
                throw new ArgumentNullException(nameof(contacts));
            }

            if (deltaTime < 0f || float.IsNaN(deltaTime))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "Must be zero or positive.");
            }

            float decay = MathF.Exp(-settings.ReturnStiffness * deltaTime);
            float limitSq = settings.ElasticLimit * settings.ElasticLimit;
            float restEpsilonSq = settings.RestEpsilon * settings.RestEpsilon;
            bool changed = false;

            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 position = positions[i];
                Vector3 rest = restPositions[i];
                VertexState state = states[i];

                if (contacts.TryGetContact(i, position, out Vector3 contactPoint))
                {
                    if (state != VertexState.Broken)
                    {
                        if (Vector3.DistanceSquared(contactPoint, rest) <= limitSq)
                        {
                            states[i] = VertexState.Glued;
                            if (contactPoint != position)
                            {
                                positions[i] = contactPoint;
                                changed = true;
                            }

                            continue;
                        }

                        // Stretched past the elastic limit: the gum snaps.
                        state = VertexState.Broken;
                    }
                }
                else if (state == VertexState.Glued || state == VertexState.Broken)
                {
                    state = VertexState.Returning;
                }

                if (state == VertexState.Rest)
                {
                    continue;
                }

                Vector3 offset = position - rest;
                if (offset.LengthSquared() <= restEpsilonSq)
                {
                    if (position != rest)
                    {
                        positions[i] = rest;
                        changed = true;
                    }

                    // A broken vertex that is still touching stays broken until contact is lost.
                    states[i] = state == VertexState.Broken ? VertexState.Broken : VertexState.Rest;
                    continue;
                }

                Vector3 relaxed = rest + offset * decay;
                if (relaxed != position)
                {
                    positions[i] = relaxed;
                    changed = true;
                }

                states[i] = state;
            }

            return changed;
        }
    }
}
