using System;

namespace StickyFabric.Core
{
    /// <summary>
    /// Tuning values for <see cref="FabricSolver"/>.
    /// </summary>
    public struct FabricSettings
    {
        /// <summary>
        /// How fast a released vertex returns to its rest position, in 1/seconds (the "k" of the thesis).
        /// Higher values give a snappier, less gummy recovery.
        /// </summary>
        public float ReturnStiffness;

        /// <summary>
        /// Maximum distance, in local units, a vertex can be stretched away from its rest position
        /// before the adhesion breaks.
        /// </summary>
        public float ElasticLimit;

        /// <summary>
        /// Distance below which a returning vertex snaps back to its exact rest position.
        /// </summary>
        public float RestEpsilon;

        public static FabricSettings Default => new FabricSettings
        {
            ReturnStiffness = 20f,
            ElasticLimit = 0.5f,
            RestEpsilon = 1e-4f,
        };

        public void Validate()
        {
            if (!(ReturnStiffness >= 0f) || float.IsInfinity(ReturnStiffness))
            {
                throw new ArgumentOutOfRangeException(nameof(ReturnStiffness), ReturnStiffness, "Must be zero or positive.");
            }

            if (!(ElasticLimit >= 0f) || float.IsInfinity(ElasticLimit))
            {
                throw new ArgumentOutOfRangeException(nameof(ElasticLimit), ElasticLimit, "Must be zero or positive.");
            }

            if (!(RestEpsilon > 0f) || float.IsInfinity(RestEpsilon))
            {
                throw new ArgumentOutOfRangeException(nameof(RestEpsilon), RestEpsilon, "Must be positive.");
            }
        }
    }
}
