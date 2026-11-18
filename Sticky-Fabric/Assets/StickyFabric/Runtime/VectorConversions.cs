using System.Runtime.CompilerServices;

namespace StickyFabric
{
    /// <summary>Conversions between System.Numerics (core solver) and UnityEngine vectors.</summary>
    public static class VectorConversions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector3 ToUnity(this System.Numerics.Vector3 v) => new UnityEngine.Vector3(v.X, v.Y, v.Z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static System.Numerics.Vector3 ToNumerics(this UnityEngine.Vector3 v) => new System.Numerics.Vector3(v.x, v.y, v.z);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static UnityEngine.Vector2 ToUnity(this System.Numerics.Vector2 v) => new UnityEngine.Vector2(v.X, v.Y);
    }
}
