namespace StickyFabric.Core
{
    /// <summary>
    /// Adhesion state of a single fabric vertex.
    /// </summary>
    public enum VertexState
    {
        /// <summary>At its rest position, nothing attached.</summary>
        Rest = 0,

        /// <summary>Stuck to the surface of a touching object.</summary>
        Glued = 1,

        /// <summary>
        /// Stretched past the elastic limit. It springs back and will not re-attach
        /// until it has lost contact at least once.
        /// </summary>
        Broken = 2,

        /// <summary>Released and springing back towards its rest position.</summary>
        Returning = 3,
    }
}
