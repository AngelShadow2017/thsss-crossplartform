// Matrix compatibility layer
using System;

namespace SlimDX
{
    public struct Matrix
    {
        public float M11, M12, M13, M14;
        public float M21, M22, M23, M24;
        public float M31, M32, M33, M34;
        public float M41, M42, M43, M44;

        public static Matrix Identity => new Matrix
        {
            M11 = 1, M22 = 1, M33 = 1, M44 = 1
        };

        /// <summary>Row-major view for System.Numerics based consumers (3D pipeline).</summary>
        public System.Numerics.Matrix4x4 ToSystemMatrix() => new(
            M11, M12, M13, M14,
            M21, M22, M23, M24,
            M31, M32, M33, M34,
            M41, M42, M43, M44);

        public static Matrix Translation(float x, float y, float z)
        {
            var m = Identity;
            m.M41 = x; m.M42 = y; m.M43 = z;
            return m;
        }

        public static Matrix Translation(Vector3 v) => Translation(v.X, v.Y, v.Z);

        public static Matrix Scaling(float x, float y, float z)
        {
            var m = Identity;
            m.M11 = x; m.M22 = y; m.M33 = z;
            return m;
        }

        public static Matrix Scaling(Vector3 v) => Scaling(v.X, v.Y, v.Z);

        public static Matrix RotationX(float angle)
        {
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            var m = Identity;
            m.M22 = c; m.M23 = s;
            m.M32 = -s; m.M33 = c;
            return m;
        }

        public static Matrix RotationY(float angle)
        {
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            var m = Identity;
            m.M11 = c; m.M13 = -s;
            m.M31 = s; m.M33 = c;
            return m;
        }

        public static Matrix RotationZ(float angle)
        {
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle);
            var m = Identity;
            m.M11 = c; m.M12 = s;
            m.M21 = -s; m.M22 = c;
            return m;
        }

        public static Matrix RotationAxis(Vector3 axis, float angle)
        {
            Vector3 n = Vector3.Normalize(axis);
            float c = (float)Math.Cos(angle), s = (float)Math.Sin(angle), t = 1 - c;
            var m = Identity;
            m.M11 = t * n.X * n.X + c;      m.M12 = t * n.X * n.Y + s * n.Z; m.M13 = t * n.X * n.Z - s * n.Y;
            m.M21 = t * n.X * n.Y - s * n.Z; m.M22 = t * n.Y * n.Y + c;      m.M23 = t * n.Y * n.Z + s * n.X;
            m.M31 = t * n.X * n.Z + s * n.Y; m.M32 = t * n.Y * n.Z - s * n.X; m.M33 = t * n.Z * n.Z + c;
            return m;
        }

        public static Matrix RotationYawPitchRoll(float yaw, float pitch, float roll)
        {
            return RotationY(yaw) * RotationX(pitch) * RotationZ(roll);
        }

        public static Matrix LookAtLH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 z = Vector3.Normalize(target - eye);
            Vector3 x = Vector3.Normalize(Vector3.Cross(up, z));
            Vector3 y = Vector3.Cross(z, x);
            var m = Identity;
            m.M11 = x.X; m.M12 = y.X; m.M13 = z.X;
            m.M21 = x.Y; m.M22 = y.Y; m.M23 = z.Y;
            m.M31 = x.Z; m.M32 = y.Z; m.M33 = z.Z;
            m.M41 = -Vector3.Dot(x, eye);
            m.M42 = -Vector3.Dot(y, eye);
            m.M43 = -Vector3.Dot(z, eye);
            return m;
        }

        public static Matrix LookAtRH(Vector3 eye, Vector3 target, Vector3 up)
        {
            Vector3 z = Vector3.Normalize(eye - target);
            Vector3 x = Vector3.Normalize(Vector3.Cross(up, z));
            Vector3 y = Vector3.Cross(z, x);
            var m = Identity;
            m.M11 = x.X; m.M12 = y.X; m.M13 = z.X;
            m.M21 = x.Y; m.M22 = y.Y; m.M23 = z.Y;
            m.M31 = x.Z; m.M32 = y.Z; m.M33 = z.Z;
            m.M41 = -Vector3.Dot(x, eye);
            m.M42 = -Vector3.Dot(y, eye);
            m.M43 = -Vector3.Dot(z, eye);
            return m;
        }

        public static Matrix PerspectiveFovLH(float fov, float aspect, float znear, float zfar)
        {
            float yScale = 1.0f / (float)Math.Tan(fov / 2.0f);
            float xScale = yScale / aspect;
            var m = new Matrix();
            m.M11 = xScale;
            m.M22 = yScale;
            m.M33 = zfar / (zfar - znear);
            m.M34 = 1.0f;
            m.M43 = -znear * zfar / (zfar - znear);
            return m;
        }

        public static Matrix PerspectiveFovRH(float fov, float aspect, float znear, float zfar)
        {
            float yScale = 1.0f / (float)Math.Tan(fov / 2.0f);
            float xScale = yScale / aspect;
            var m = new Matrix();
            m.M11 = xScale;
            m.M22 = yScale;
            m.M33 = zfar / (znear - zfar);
            m.M34 = -1.0f;
            m.M43 = znear * zfar / (znear - zfar);
            return m;
        }

        public static Matrix OrthoLH(float width, float height, float znear, float zfar)
        {
            var m = new Matrix();
            m.M11 = 2.0f / width;
            m.M22 = 2.0f / height;
            m.M33 = 1.0f / (zfar - znear);
            m.M43 = -znear / (zfar - znear);
            m.M44 = 1.0f;
            return m;
        }

        public static Matrix OrthoRH(float width, float height, float znear, float zfar)
        {
            var m = new Matrix();
            m.M11 = 2.0f / width;
            m.M22 = 2.0f / height;
            m.M33 = -1.0f / (zfar - znear);
            m.M43 = -znear / (zfar - znear);
            m.M44 = 1.0f;
            return m;
        }

        public static Matrix OrthoOffCenterLH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            var m = new Matrix();
            m.M11 = 2.0f / (right - left);
            m.M22 = 2.0f / (top - bottom);
            m.M33 = 1.0f / (zfar - znear);
            m.M41 = -(right + left) / (right - left);
            m.M42 = -(top + bottom) / (top - bottom);
            m.M43 = -znear / (zfar - znear);
            m.M44 = 1.0f;
            return m;
        }

        public static Matrix OrthoOffCenterRH(float left, float right, float bottom, float top, float znear, float zfar)
        {
            var m = new Matrix();
            m.M11 = 2.0f / (right - left);
            m.M22 = 2.0f / (top - bottom);
            m.M33 = -1.0f / (zfar - znear);
            m.M41 = -(right + left) / (right - left);
            m.M42 = -(top + bottom) / (top - bottom);
            m.M43 = -znear / (zfar - znear);
            m.M44 = 1.0f;
            return m;
        }

        public static Matrix Transformation2D(Vector2 scalingCenter, float scalingRotation, Vector2 scaling, Vector2 rotationCenter, float rotation, Vector2 translation)
        {
            return Translation(-scalingCenter.X, -scalingCenter.Y, 0)
                * RotationZ(-scalingRotation)
                * Scaling(scaling.X, scaling.Y, 1)
                * RotationZ(scalingRotation)
                * Translation(scalingCenter.X - rotationCenter.X, scalingCenter.Y - rotationCenter.Y, 0)
                * RotationZ(rotation)
                * Translation(rotationCenter.X + translation.X, rotationCenter.Y + translation.Y, 0);
        }

        public static Matrix Multiply(Matrix a, Matrix b)
        {
            Matrix result;
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            result.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            result.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            result.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            result.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;
            result.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            result.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            result.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            result.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;
            result.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            result.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            result.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            result.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;
            return result;
        }

        public static Matrix Invert(Matrix m)
        {
            // Simplified inverse for common cases
            return Identity; // TODO: implement proper inverse
        }

        public static Matrix Transpose(Matrix m)
        {
            Matrix result;
            result.M11 = m.M11; result.M12 = m.M21; result.M13 = m.M31; result.M14 = m.M41;
            result.M21 = m.M12; result.M22 = m.M22; result.M23 = m.M32; result.M24 = m.M42;
            result.M31 = m.M13; result.M32 = m.M23; result.M33 = m.M33; result.M34 = m.M43;
            result.M41 = m.M14; result.M42 = m.M24; result.M43 = m.M34; result.M44 = m.M44;
            return result;
        }

        public static Matrix operator *(Matrix a, Matrix b) => Multiply(a, b);

        /// <summary>
        /// Upload the transpose of this D3D row-vector matrix for GLSL column vectors.
        /// Its column-major storage equals our row-major fields. Use transpose: false.
        /// </summary>
        public float[] ToOpenGLMatrix()
        {
            return new float[]
            {
                M11, M12, M13, M14,
                M21, M22, M23, M24,
                M31, M32, M33, M34,
                M41, M42, M43, M44,
            };
        }
    }
}
