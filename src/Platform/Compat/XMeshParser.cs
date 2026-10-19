using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

namespace SlimDX.Direct3D9;

using Vector3 = System.Numerics.Vector3;
using Vector2 = System.Numerics.Vector2;

internal sealed class XMeshPart
{
    internal Vector3[] Positions = Array.Empty<Vector3>();
    internal Vector3[] Normals = Array.Empty<Vector3>();
    internal Vector2[] UV = Array.Empty<Vector2>();
    internal int[][] Faces = Array.Empty<int[]>();
    internal int[][] NormalFaces = Array.Empty<int[]>();
    internal int[] FaceMaterials = Array.Empty<int>();
    internal readonly List<object> Materials = new();
}

internal sealed class XMeshFrame
{
    internal Matrix4x4 Transform = Matrix4x4.Identity;
    internal readonly List<XMeshFrame> Children = new();
    internal readonly List<XMeshPart> Meshes = new();
}

internal sealed class XMeshParser
{
    private readonly XTextTokens input;
    internal readonly Dictionary<string, ExtendedMaterial> NamedMaterials = new();
    internal readonly XMeshFrame Root = new();

    internal XMeshParser(string text)
    {
        input = new XTextTokens(text);
        ReadScope(Root, false);
    }

    private void ReadScope(XMeshFrame frame, bool nested)
    {
        while (!input.End && input.Peek != "}")
        {
            string type = input.Take();
            switch (type)
            {
                case "template": input.SkipTemplate(); break;
                case "Material": ReadMaterial(); break;
                case "Frame":
                    input.OpenObject();
                    var child = new XMeshFrame();
                    ReadScope(child, true);
                    frame.Children.Add(child);
                    break;
                case "FrameTransformMatrix": frame.Transform = ReadTransform(); break;
                case "Mesh": frame.Meshes.Add(ReadMesh()); break;
                default: throw new NotSupportedException($"Unsupported .x object '{type}'.");
            }
        }
        if (nested) input.Expect("}");
        else if (!input.End) throw new InvalidDataException("Unexpected closing brace in .x file.");
    }

    private Matrix4x4 ReadTransform()
    {
        input.OpenObject();
        var m = new float[16];
        for (int i = 0; i < m.Length; i++) m[i] = input.Number();
        input.Expect("}");
        return new Matrix4x4(m[0],m[1],m[2],m[3],m[4],m[5],m[6],m[7],m[8],m[9],m[10],m[11],m[12],m[13],m[14],m[15]);
    }

    private XMeshPart ReadMesh()
    {
        input.OpenObject();
        var mesh = new XMeshPart { Positions = ReadVectors(), Faces = ReadFaces() };
        bool normals = false, uv = false, materials = false;
        while (input.Peek != "}")
        {
            string type = input.Take();
            input.OpenObject();
            switch (type)
            {
                case "MeshNormals" when !normals:
                    mesh.Normals = ReadVectors(); mesh.NormalFaces = ReadFaces(); normals = true; break;
                case "MeshTextureCoords" when !uv:
                    mesh.UV = new Vector2[input.Integer()];
                    for (int i = 0; i < mesh.UV.Length; i++) mesh.UV[i] = new Vector2(input.Number(), input.Number());
                    uv = true; break;
                case "MeshMaterialList" when !materials:
                    ReadMaterialList(mesh); materials = true; break;
                default: throw new NotSupportedException($"Unsupported or duplicate .x mesh data '{type}'.");
            }
            input.Expect("}");
        }
        input.Expect("}");
        Validate(mesh, normals, uv, materials);
        return mesh;
    }

    private Vector3[] ReadVectors()
    {
        var values = new Vector3[input.Integer()];
        for (int i = 0; i < values.Length; i++) values[i] = new Vector3(input.Number(), input.Number(), input.Number());
        return values;
    }

    private int[][] ReadFaces()
    {
        var faces = new int[input.Integer()][];
        for (int i = 0; i < faces.Length; i++)
        {
            faces[i] = new int[input.Integer()];
            if (faces[i].Length < 3) throw new InvalidDataException("A .x face needs at least three corners.");
            for (int j = 0; j < faces[i].Length; j++) faces[i][j] = input.Integer();
        }
        return faces;
    }

    private void ReadMaterialList(XMeshPart mesh)
    {
        int count = input.Integer();
        mesh.FaceMaterials = new int[input.Integer()];
        for (int i = 0; i < mesh.FaceMaterials.Length; i++) mesh.FaceMaterials[i] = input.Integer();
        while (input.Peek != "}")
        {
            if (input.Peek == "{")
            {
                input.Take(); mesh.Materials.Add(input.Take()); input.Expect("}");
            }
            else
            {
                input.Expect("Material"); mesh.Materials.Add(ReadMaterial());
            }
        }
        if (mesh.Materials.Count != count) throw new InvalidDataException(".x material count does not match entries.");
    }

    private ExtendedMaterial ReadMaterial()
    {
        string name = input.OpenObject();
        var diffuse = ReadColor(true);
        var material = new ExtendedMaterial { Material = new Material {
            Diffuse = diffuse, Ambient = diffuse, Power = input.Number(), Specular = ReadColor(false), Emissive = ReadColor(false) } };
        if (input.Peek != "}")
        {
            input.Expect("TextureFilename"); input.OpenObject();
            material.TextureFilename = input.String(); input.Expect("}");
        }
        input.Expect("}");
        if (name.Length > 0 && !NamedMaterials.TryAdd(name, material))
            throw new InvalidDataException($"Duplicate .x material '{name}'.");
        return material;
    }

    private Color4 ReadColor(bool alpha)
    {
        float r = input.Number(), g = input.Number(), b = input.Number();
        return new Color4(r, g, b, alpha ? input.Number() : 1f);
    }

    private static void Validate(XMeshPart mesh, bool normals, bool uv, bool materials)
    {
        if (uv && mesh.UV.Length != mesh.Positions.Length) throw new InvalidDataException(".x UV count differs from vertex count.");
        if (normals && mesh.NormalFaces.Length != mesh.Faces.Length) throw new InvalidDataException(".x normal face count differs from face count.");
        if (materials && mesh.FaceMaterials.Length != mesh.Faces.Length && mesh.FaceMaterials.Length != 1)
            throw new InvalidDataException(".x face material count differs from face count.");
        for (int f = 0; f < mesh.Faces.Length; f++)
        {
            if (normals && mesh.NormalFaces[f].Length != mesh.Faces[f].Length)
                throw new InvalidDataException(".x normal face corners differ from face corners.");
            foreach (int index in mesh.Faces[f])
                if (index >= mesh.Positions.Length) throw new InvalidDataException(".x vertex index out of range.");
            if (normals) foreach (int index in mesh.NormalFaces[f])
                if (index >= mesh.Normals.Length) throw new InvalidDataException(".x normal index out of range.");
            if (materials && mesh.FaceMaterials[mesh.FaceMaterials.Length == 1 ? 0 : f] >= mesh.Materials.Count)
                throw new InvalidDataException(".x material index out of range.");
        }
    }
}
