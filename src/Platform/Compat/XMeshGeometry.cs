using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

namespace SlimDX.Direct3D9;

using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

public readonly struct MeshVertex
{
    public Vector3 Position { get; }
    public Vector3 Normal { get; }
    public Vector2 TextureCoordinate { get; }
    internal MeshVertex(Vector3 position, Vector3 normal, Vector2 uv)
    { Position = position; Normal = normal; TextureCoordinate = uv; }
}

internal sealed class XMeshGeometry
{
    internal readonly List<MeshVertex> Vertices = new();
    internal readonly List<int> Indices = new();
    internal readonly List<int> Attributes = new();
    internal readonly List<ExtendedMaterial> Materials = new();
    private readonly Dictionary<string, int> namedIndices = new();
    private readonly XMeshParser parser;
    internal bool HasNormals { get; private set; } = true;
    internal bool HasUV { get; private set; }

    internal XMeshGeometry(string text)
    {
        parser = new XMeshParser(text);
        AddFrame(parser.Root, Matrix4x4.Identity);
        if (Indices.Count == 0) throw new InvalidDataException("The .x file contains no mesh faces.");
    }

    private void AddFrame(XMeshFrame frame, Matrix4x4 parent)
    {
        var transform = frame.Transform * parent;
        foreach (var part in frame.Meshes) AddPart(part, transform);
        foreach (var child in frame.Children) AddFrame(child, transform);
    }

    private int MaterialIndex(object entry)
    {
        if (entry is string name)
        {
            if (namedIndices.TryGetValue(name, out int existing)) return existing;
            if (!parser.NamedMaterials.TryGetValue(name, out var material)) throw new InvalidDataException($"Unknown .x material '{name}'.");
            int index = Materials.Count;
            namedIndices.Add(name, index); Materials.Add(material);
            return index;
        }
        Materials.Add((ExtendedMaterial)entry);
        return Materials.Count - 1;
    }

    private void AddPart(XMeshPart part, Matrix4x4 transform)
    {
        if (!Matrix4x4.Invert(transform, out var inverse)) throw new InvalidDataException("Singular .x frame transform.");
        var normalTransform = Matrix4x4.Transpose(inverse);
        int[] materials = part.Materials.Select(MaterialIndex).ToArray();
        if (materials.Length == 0)
            materials = new[] { MaterialIndex(new ExtendedMaterial { Material = new Material { Diffuse = new Color4(1f,1f,1f,1f), Ambient = new Color4(1f,1f,1f,1f) } }) };
        bool normals = part.Normals.Length > 0;
        if (part.Faces.Length > 0) HasNormals &= normals;
        HasUV |= part.UV.Length > 0;
        int start = Vertices.Count;
        foreach (var p in part.Positions)
            Vertices.Add(new MeshVertex(Vector3.Transform(p, transform), Vector3.Zero, Vector2.Zero));
        var assigned = new bool[part.Positions.Length];
        var remap = new Dictionary<(int, int), int>();
        for (int f = 0; f < part.Faces.Length; f++)
        {
            var face = part.Faces[f];
            var corners = new int[face.Length];
            for (int c = 0; c < face.Length; c++)
                corners[c] = AddCorner(part, face[c], normals ? part.NormalFaces[f][c] : -1, start, normalTransform, remap, assigned);
            int material = materials[part.FaceMaterials.Length == 0 ? 0 : part.FaceMaterials[part.FaceMaterials.Length == 1 ? 0 : f]];
            foreach (int c in Triangulate(face.Select(i => part.Positions[i]).ToArray())) Indices.Add(corners[c]);
            for (int i = 0; i < face.Length - 2; i++) Attributes.Add(material);
        }
    }

    private int AddCorner(XMeshPart part, int p, int n, int start, Matrix4x4 normalTransform,
        Dictionary<(int, int), int> remap, bool[] assigned)
    {
        if (remap.TryGetValue((p, n), out int existing)) return existing;
        Vector3 normal = n < 0 ? Vector3.Zero : Vector3.TransformNormal(part.Normals[n], normalTransform);
        if (normal.LengthSquared() > 0) normal = Vector3.Normalize(normal);
        var vertex = new MeshVertex(Vertices[start + p].Position, normal, part.UV.Length == 0 ? Vector2.Zero : part.UV[p]);
        int index = assigned[p] ? Vertices.Count : start + p;
        if (assigned[p]) Vertices.Add(vertex); else Vertices[index] = vertex;
        assigned[p] = true; remap.Add((p, n), index);
        return index;
    }

    internal static int[] Triangulate(Vector3[] points)
    {
        if (points.Length == 3) return new[] { 0, 1, 2 };
        Vector3 normal = Vector3.Zero;
        for (int i = 0; i < points.Length; i++) normal += Vector3.Cross(points[i], points[(i + 1) % points.Length]);
        var abs = Vector3.Abs(normal);
        int axis = abs.X > abs.Y && abs.X > abs.Z ? 0 : abs.Y > abs.Z ? 1 : 2;
        var flat = points.Select(p => axis == 0 ? new Vector2(p.Y,p.Z) : axis == 1 ? new Vector2(p.Z,p.X) : new Vector2(p.X,p.Y)).ToArray();
        float area = 0;
        for (int i = 0; i < flat.Length; i++) area += Cross(flat[i], flat[(i + 1) % flat.Length]);
        if (area == 0) throw new InvalidDataException("Cannot triangulate a degenerate .x polygon.");
        float sign = MathF.Sign(area);
        var remaining = Enumerable.Range(0, points.Length).ToList();
        var result = new List<int>();
        while (remaining.Count > 3)
        {
            bool found = false;
            for (int i = 0; i < remaining.Count; i++)
            {
                int a = remaining[(i + remaining.Count - 1) % remaining.Count], b = remaining[i], c = remaining[(i + 1) % remaining.Count];
                if (Cross(flat[b] - flat[a], flat[c] - flat[b]) * sign <= 0) continue;
                if (remaining.Any(p => p != a && p != b && p != c && Inside(flat[p],flat[a],flat[b],flat[c],sign))) continue;
                result.AddRange(new[] { a,b,c }); remaining.RemoveAt(i); found = true; break;
            }
            if (!found) throw new InvalidDataException("Cannot triangulate a self-intersecting or degenerate .x polygon.");
        }
        result.AddRange(remaining);
        return result.ToArray();
    }

    private static float Cross(Vector2 a, Vector2 b) => a.X * b.Y - a.Y * b.X;
    private static bool Inside(Vector2 p, Vector2 a, Vector2 b, Vector2 c, float sign) =>
        Cross(b-a,p-a)*sign >= 0 && Cross(c-b,p-b)*sign >= 0 && Cross(a-c,p-c)*sign >= 0;
}
