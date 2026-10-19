using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

namespace SlimDX.Direct3D9;

using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

public sealed class Mesh : IDisposable
{
    private readonly Device device;
    private readonly MeshFlags flags;
    private MeshVertex[] vertices;
    private readonly int[] indices;
    private readonly int[] attributes;
    private readonly ExtendedMaterial[] materials;
    private VertexBuffer vertexBuffer;
    private VertexFormat vertexFormat;
    private int[] subsetStarts;
    private int[] subsetCounts;

    private Mesh(Device device, MeshFlags flags, MeshVertex[] vertices, int[] indices,
        int[] attributes, ExtendedMaterial[] materials, VertexFormat format)
    {
        this.device = device;
        this.flags = flags;
        this.vertices = vertices;
        this.indices = indices;
        this.attributes = attributes;
        this.materials = materials;
        vertexFormat = format;
    }

    public VertexFormat VertexFormat
    {
        get => vertexFormat;
        set { ThrowIfDisposed(); CheckFormat(value); vertexFormat = value; InvalidateBuffer(); }
    }
    public int VertexCount => vertices.Length;
    public int FaceCount => indices.Length / 3;
    public bool IsDisposed { get; private set; }
    public IReadOnlyList<MeshVertex> Vertices => Array.AsReadOnly(vertices);
    public IReadOnlyList<int> Indices => Array.AsReadOnly(indices);
    public IReadOnlyList<int> FaceAttributes => Array.AsReadOnly(attributes);
    public VertexBuffer VertexBuffer { get { EnsureBuffer(); return vertexBuffer; } }
    public IndexBuffer IndexBuffer => throw new NotSupportedException("Mesh draws expanded triangle streams; indexed buffer access is not supported. Use Indices for topology.");

    public static Mesh FromFile(Device device, string filename, MeshFlags flags)
    {
        ArgumentException.ThrowIfNullOrEmpty(filename);
        string path = ResolvePath(filename);
        var geometry = new XMeshGeometry(File.ReadAllText(path));
        var format = VertexFormat.Position;
        if (geometry.HasNormals) format |= VertexFormat.Normal;
        if (geometry.HasUV) format |= VertexFormat.Texture1;
        return new Mesh(device, flags, geometry.Vertices.ToArray(), geometry.Indices.ToArray(),
            geometry.Attributes.ToArray(), geometry.Materials.ToArray(), format);
    }

    private static string ResolvePath(string filename)
    {
        if (File.Exists(filename)) return filename;
        string directory = Path.GetDirectoryName(filename);
        if (string.IsNullOrEmpty(directory)) directory = ".";
        if (Directory.Exists(directory))
        {
            string name = Path.GetFileName(filename);
            var matches = Directory.EnumerateFiles(directory).Where(p => string.Equals(Path.GetFileName(p), name, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length == 1) return matches[0];
        }
        throw new FileNotFoundException("DirectX mesh file not found.", filename);
    }

    public static Mesh FromFile(Device device, string filename, out ExtendedMaterial[] materials)
    {
        var mesh = FromFile(device, filename, MeshFlags.Managed);
        materials = mesh.GetMaterials();
        return mesh;
    }

    public static Mesh FromFile(Device device, string filename, MeshFlags flags, Device adjacency) => FromFile(device, filename, flags);
    public ExtendedMaterial[] GetMaterials() { ThrowIfDisposed(); return (ExtendedMaterial[])materials.Clone(); }

    public Mesh Clone(Device device, MeshFlags flags, VertexFormat format)
    {
        ThrowIfDisposed(); CheckFormat(format);
        var copy = vertices.Select(v => new MeshVertex(v.Position,
            (format & VertexFormat.Normal) != 0 ? v.Normal : Vector3.Zero,
            (format & VertexFormat.Texture1) != 0 ? v.TextureCoordinate : Vector2.Zero)).ToArray();
        return new Mesh(device, flags, copy, (int[])indices.Clone(), (int[])attributes.Clone(), (ExtendedMaterial[])materials.Clone(), format);
    }

    public Mesh Clone(Device device, MeshFlags flags, VertexElement[] declaration)
    {
        ArgumentNullException.ThrowIfNull(declaration);
        var format = VertexFormat.None;
        int offset = 0;
        foreach (var element in declaration)
        {
            if (element.Type == DeclarationType.Unused) break;
            if (element.Stream != 0 || element.Offset != offset || element.UsageIndex != 0)
                throw new NotSupportedException("Mesh declarations require tightly packed stream zero elements.");
            switch (element.Usage)
            {
                case DeclarationUsage.Position when element.Type == DeclarationType.Float3 && offset == 0:
                    format |= VertexFormat.Position; offset += 12; break;
                case DeclarationUsage.Normal when element.Type == DeclarationType.Float3 && offset == 12:
                    format |= VertexFormat.Normal; offset += 12; break;
                case DeclarationUsage.TextureCoordinate when element.Type == DeclarationType.Float2 && (format & VertexFormat.Texture1) == 0:
                    format |= VertexFormat.Texture1; offset += 8; break;
                default: throw new NotSupportedException("Unsupported mesh vertex declaration.");
            }
        }
        return Clone(device, flags, format);
    }

    private static void CheckFormat(VertexFormat format)
    {
        const VertexFormat supported = VertexFormat.Position | VertexFormat.Normal | VertexFormat.Texture1;
        if ((format & VertexFormat.Position) == 0 || (format & ~supported) != 0)
            throw new NotSupportedException($"Unsupported mesh vertex format '{format}'. Expected Position with optional Normal and Texture1.");
    }

    public void ComputeNormals()
    {
        ThrowIfDisposed();
        if ((vertexFormat & VertexFormat.Normal) == 0) throw new InvalidOperationException("Clone the mesh with VertexFormat.Normal before computing normals.");
        var normals = new Vector3[vertices.Length];
        for (int i = 0; i < indices.Length; i += 3)
        {
            int a = indices[i], b = indices[i+1], c = indices[i+2];
            var normal = Vector3.Cross(vertices[b].Position - vertices[a].Position, vertices[c].Position - vertices[a].Position);
            normals[a] += normal; normals[b] += normal; normals[c] += normal;
        }
        for (int i = 0; i < vertices.Length; i++)
        {
            var n = normals[i].LengthSquared() == 0 ? Vector3.Zero : Vector3.Normalize(normals[i]);
            vertices[i] = new MeshVertex(vertices[i].Position, n, vertices[i].TextureCoordinate);
        }
        InvalidateBuffer();
    }

    public void DrawSubset(int i)
    {
        ThrowIfDisposed();
        if (i < 0 || i >= materials.Length) throw new ArgumentOutOfRangeException(nameof(i));
        EnsureBuffer();
        if (subsetCounts[i] == 0) return;
        device.SetFVF(vertexFormat);
        device.SetStreamSource(0, vertexBuffer, 0, Stride);
        device.DrawPrimitives(PrimitiveType.TriangleList, subsetStarts[i], subsetCounts[i]);
    }

    private int Stride => 12 + ((vertexFormat & VertexFormat.Normal) != 0 ? 12 : 0) + ((vertexFormat & VertexFormat.Texture1) != 0 ? 8 : 0);

    private void EnsureBuffer()
    {
        ThrowIfDisposed();
        if (vertexBuffer != null) return;
        if (device == null) throw new InvalidOperationException("A graphics device is required to draw a mesh.");
        subsetStarts = new int[materials.Length]; subsetCounts = new int[materials.Length];
        var data = new List<float>(indices.Length * Stride / 4);
        for (int subset = 0; subset < materials.Length; subset++)
        {
            subsetStarts[subset] = data.Count * 4 / Stride;
            for (int face = 0; face < attributes.Length; face++)
            {
                if (attributes[face] != subset) continue;
                subsetCounts[subset]++;
                for (int c = 0; c < 3; c++) AppendVertex(data, vertices[indices[face * 3 + c]]);
            }
        }
        var pool = (flags & MeshFlags.SystemMemory) != 0 ? Pool.SystemMemory : Pool.Managed;
        vertexBuffer = new VertexBuffer(device, data.Count * 4, Usage.WriteOnly, vertexFormat, pool);
        vertexBuffer.SetData(data.ToArray());
    }

    private void AppendVertex(List<float> data, MeshVertex v)
    {
        data.AddRange(new[] { v.Position.X, v.Position.Y, v.Position.Z });
        if ((vertexFormat & VertexFormat.Normal) != 0) data.AddRange(new[] { v.Normal.X, v.Normal.Y, v.Normal.Z });
        if ((vertexFormat & VertexFormat.Texture1) != 0) data.AddRange(new[] { v.TextureCoordinate.X, v.TextureCoordinate.Y });
    }

    private void InvalidateBuffer() { vertexBuffer?.Dispose(); vertexBuffer = null; }
    private void ThrowIfDisposed() { ObjectDisposedException.ThrowIf(IsDisposed, this); }
    public void Dispose() { if (IsDisposed) return; InvalidateBuffer(); IsDisposed = true; }
}
