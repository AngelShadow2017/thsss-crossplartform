using System;
using System.Numerics;
using NumericsVector4 = System.Numerics.Vector4;

namespace SlimDX.Direct3D9;

/// <summary>
/// Matrix conventions for Device3D.
///
/// The game builds matrices with Direct3D's row-vector convention: a point is
/// transformed as `v * M` and the translation lives in M41..M43. OpenGL's GLSL
/// multiplies column vectors (`M * v`) and reads uniforms column-major, so the
/// only conversion needed is a transpose on upload; the shader then multiplies in
/// the natural order `projection * view * world`.
///
/// Getting either half wrong collapses every vertex: the translation ends up in
/// the clip-space W component and NDC becomes (0,0).
/// </summary>
internal static class WorldViewProjection
{
    /// <summary>Column-major upload for a row-vector (D3D) matrix.</summary>
    internal static float[] ColumnMajor(Matrix4x4 m) => new[]
    {
        m.M11, m.M12, m.M13, m.M14,
        m.M21, m.M22, m.M23, m.M24,
        m.M31, m.M32, m.M33, m.M34,
        m.M41, m.M42, m.M43, m.M44,
    };

    /// <summary>Row-vector (D3D) transform used by the game and the unit tests.</summary>
    internal static NumericsVector4 Project(Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection, NumericsVector4 point)
    {
        static NumericsVector4 Mul(NumericsVector4 v, Matrix4x4 m) => new(
            v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41,
            v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42,
            v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43,
            v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + v.W * m.M44);
        return Mul(Mul(Mul(point, world), view), projection);
    }

    /// <summary>Column-major matrices as stored by GL; multiplies like GLSL.</summary>
    internal static NumericsVector4 ProjectColumns(
        Matrix4x4 world, Matrix4x4 view, Matrix4x4 projection, NumericsVector4 point)
    {
        // Matches the shader: uWorld * uView * uProjection * vec4(position, 1).
        static NumericsVector4 Mul(Matrix4x4 m, NumericsVector4 v) => new(
            m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * v.W,
            m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * v.W,
            m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * v.W,
            m.M41 * v.X + m.M42 * v.Y + m.M43 * v.Z + m.M44 * v.W);
        return Mul(projection, Mul(view, Mul(world, point)));
    }
}
