using System.Numerics;

namespace StickyFabric.Core
{
    /// <summary>
    /// Answers "is something touching this vertex, and where is its surface?".
    /// The Unity implementation uses physics overlap queries; tests use analytic shapes.
    /// </summary>
    public interface IContactQuery
    {
        /// <param name="vertexIndex">Index of the vertex being tested.</param>
        /// <param name="localPosition">Current vertex position in the fabric's local space.</param>
        /// <param name="localContactPoint">Closest point on the touching surface, in local space.</param>
        /// <returns><c>true</c> if a surface is in contact with the vertex.</returns>
        bool TryGetContact(int vertexIndex, Vector3 localPosition, out Vector3 localContactPoint);
    }
}
