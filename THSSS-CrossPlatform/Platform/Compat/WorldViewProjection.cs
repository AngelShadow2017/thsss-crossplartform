using System.Numerics;

namespace SlimDX.Direct3D9;

/// <summary>
/// Clip-space maths for Device3D. D3D (and the game) use row vectors with the
/// translation in the last row (M41..M43); GLSL uses column vectors. Feeding a
/// row-vector matrix to `uProjection * uView * uWorld * pos` therefore requires
/// transposing it during upload, otherwise the translation lands in the clip-space
/// W component and every vertex collapses to (0,0) — which is exactly what a
/// black 3D background looks like.
/// </summary>
internal static class WorldViewProjection
{
    /// <summary>Column-major floats for glUniformMatrix4fv (transpose = false).</summary>
    internal static float[] ToColumnMajor(Matrix4x4 m) => new[]
    {
        m.M11, m.M21, m.M31, m.M41,
        m.M12, m.M22, m.M32, m.M42,
        m.M13, m.M23, m.M33, m.M43,
        m.M14, m.M24, m.M34, m.M44,
    };
}
