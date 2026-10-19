// Cross-platform math types compatible with SlimDX math types
using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SlimDX;

/// <summary>2D vector compatible with SlimDX.Vector2</summary>
public struct Vector2 : IEquatable<Vector2>
{
    public float X;
    public float Y;

    public Vector2(float x, float y) { X = x; Y = y; }
    public Vector2(float value) { X = value; Y = value; }

    public static Vector2 Zero => new(0, 0);
    public static Vector2 One => new(1, 1);
    public static Vector2 UnitX => new(1, 0);
    public static Vector2 UnitY => new(0, 1);

    public float Length() => MathF.Sqrt(X * X + Y * Y);
    public float LengthSquared() => X * X + Y * Y;

    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; }
    }

    public static Vector2 Normalize(Vector2 value)
    {
        float len = value.Length();
        return len > 0 ? new Vector2(value.X / len, value.Y / len) : Zero;
    }

    public static float Dot(Vector2 a, Vector2 b) => a.X * b.X + a.Y * b.Y;
    public static float Distance(Vector2 a, Vector2 b) => (a - b).Length();

    public static Vector2 Lerp(Vector2 a, Vector2 b, float t)
        => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);

    public static Vector2 TransformCoordinate(Vector2 vec, Matrix mat)
    {
        float x = vec.X * mat.M11 + vec.Y * mat.M21 + mat.M41;
        float y = vec.X * mat.M12 + vec.Y * mat.M22 + mat.M42;
        float w = vec.X * mat.M14 + vec.Y * mat.M24 + mat.M44;
        if (MathF.Abs(w) > 1e-10f) { x /= w; y /= w; }
        return new Vector2(x, y);
    }

    public static Vector2 Transform(Vector2 vec, Matrix mat)
    {
        float x = vec.X * mat.M11 + vec.Y * mat.M21 + mat.M41;
        float y = vec.X * mat.M12 + vec.Y * mat.M22 + mat.M42;
        return new Vector2(x, y);
    }

    public static Vector2 operator +(Vector2 a, Vector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static Vector2 operator -(Vector2 a, Vector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static Vector2 operator *(Vector2 a, float s) => new(a.X * s, a.Y * s);
    public static Vector2 operator *(float s, Vector2 a) => new(a.X * s, a.Y * s);
    public static Vector2 operator *(Vector2 a, Vector2 b) => new(a.X * b.X, a.Y * b.Y);
    public static Vector2 operator /(Vector2 a, float s) => new(a.X / s, a.Y / s);
    public static Vector2 operator -(Vector2 a) => new(-a.X, -a.Y);
    public static bool operator ==(Vector2 a, Vector2 b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

    public bool Equals(Vector2 other) => this == other;
    public override bool Equals(object obj) => obj is Vector2 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";

    public static implicit operator System.Numerics.Vector2(Vector2 v) => new(v.X, v.Y);
    public static implicit operator Vector2(System.Numerics.Vector2 v) => new(v.X, v.Y);
}

/// <summary>3D vector compatible with SlimDX.Vector3</summary>
public struct Vector3 : IEquatable<Vector3>
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }
    public Vector3(float value) { X = value; Y = value; Z = value; }
    public Vector3(Vector2 v, float z) { X = v.X; Y = v.Y; Z = z; }

    public static Vector3 Zero => new(0, 0, 0);
    public static Vector3 One => new(1, 1, 1);
    public static Vector3 UnitX => new(1, 0, 0);
    public static Vector3 UnitY => new(0, 1, 0);
    public static Vector3 UnitZ => new(0, 0, 1);

    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z);
    public float LengthSquared() => X * X + Y * Y + Z * Z;

    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; Z /= len; }
    }

    public static Vector3 Normalize(Vector3 v)
    {
        float len = v.Length();
        return len > 0 ? new Vector3(v.X / len, v.Y / len, v.Z / len) : Zero;
    }

    public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    public static Vector3 Cross(Vector3 a, Vector3 b)
        => new(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X);

    public static float Distance(Vector3 a, Vector3 b) => (a - b).Length();

    public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
        => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t, a.Z + (b.Z - a.Z) * t);

    public static Vector3 Transform(Vector3 v, Matrix m)
    {
        float x = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41;
        float y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42;
        float z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43;
        return new Vector3(x, y, z);
    }

    public static Vector3 TransformCoordinate(Vector3 v, Matrix m)
    {
        float x = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + m.M41;
        float y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + m.M42;
        float z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + m.M43;
        float w = v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + m.M44;
        if (MathF.Abs(w) > 1e-10f) { x /= w; y /= w; z /= w; }
        return new Vector3(x, y, z);
    }

    public static Vector3 TransformNormal(Vector3 v, Matrix m)
    {
        float x = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31;
        float y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32;
        float z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33;
        return new Vector3(x, y, z);
    }

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3 operator *(Vector3 a, float s) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3 operator *(float s, Vector3 a) => new(a.X * s, a.Y * s, a.Z * s);
    public static Vector3 operator *(Vector3 a, Vector3 b) => new(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
    public static Vector3 operator /(Vector3 a, float s) => new(a.X / s, a.Y / s, a.Z / s);
    public static Vector3 operator -(Vector3 a) => new(-a.X, -a.Y, -a.Z);
    public static bool operator ==(Vector3 a, Vector3 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);

    public bool Equals(Vector3 other) => this == other;
    public override bool Equals(object obj) => obj is Vector3 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public override string ToString() => $"({X}, {Y}, {Z})";

    public static implicit operator System.Numerics.Vector3(Vector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator Vector3(System.Numerics.Vector3 v) => new(v.X, v.Y, v.Z);
}

/// <summary>4D vector compatible with SlimDX.Vector4</summary>
public struct Vector4 : IEquatable<Vector4>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Vector4(float x, float y, float z, float w) { X = x; Y = y; Z = z; W = w; }
    public Vector4(float value) { X = value; Y = value; Z = value; W = value; }
    public Vector4(Vector3 v, float w) { X = v.X; Y = v.Y; Z = v.Z; W = w; }
    public Vector4(Vector2 v, float z, float w) { X = v.X; Y = v.Y; Z = z; W = w; }

    public static Vector4 Zero => new(0, 0, 0, 0);
    public static Vector4 One => new(1, 1, 1, 1);

    public float Length() => MathF.Sqrt(X * X + Y * Y + Z * Z + W * W);
    public float LengthSquared() => X * X + Y * Y + Z * Z + W * W;

    public void Normalize()
    {
        float len = Length();
        if (len > 0) { X /= len; Y /= len; Z /= len; W /= len; }
    }

    public static Vector4 Normalize(Vector4 v)
    {
        float len = v.Length();
        return len > 0 ? new Vector4(v.X / len, v.Y / len, v.Z / len, v.W / len) : Zero;
    }

    public static float Dot(Vector4 a, Vector4 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

    public static Vector4 Transform(Vector4 v, Matrix m)
    {
        float x = v.X * m.M11 + v.Y * m.M21 + v.Z * m.M31 + v.W * m.M41;
        float y = v.X * m.M12 + v.Y * m.M22 + v.Z * m.M32 + v.W * m.M42;
        float z = v.X * m.M13 + v.Y * m.M23 + v.Z * m.M33 + v.W * m.M43;
        float w = v.X * m.M14 + v.Y * m.M24 + v.Z * m.M34 + v.W * m.M44;
        return new Vector4(x, y, z, w);
    }

    public static Vector4 Lerp(Vector4 a, Vector4 b, float t)
        => new(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t,
               a.Z + (b.Z - a.Z) * t, a.W + (b.W - a.W) * t);

    public static Vector4 operator +(Vector4 a, Vector4 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
    public static Vector4 operator -(Vector4 a, Vector4 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    public static Vector4 operator *(Vector4 a, float s) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static Vector4 operator *(float s, Vector4 a) => new(a.X * s, a.Y * s, a.Z * s, a.W * s);
    public static Vector4 operator -(Vector4 a) => new(-a.X, -a.Y, -a.Z, -a.W);
    public static bool operator ==(Vector4 a, Vector4 b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    public static bool operator !=(Vector4 a, Vector4 b) => !(a == b);

    public bool Equals(Vector4 other) => this == other;
    public override bool Equals(object obj) => obj is Vector4 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z, W);
    public override string ToString() => $"({X}, {Y}, {Z}, {W})";
}
