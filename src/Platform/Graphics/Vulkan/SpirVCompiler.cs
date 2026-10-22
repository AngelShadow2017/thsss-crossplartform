using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace THSSS.Graphics.Vulkan;

/// <summary>
/// Turns the project's single GLSL source into Vulkan SPIR-V. The GL dialect forbids
/// explicit output locations and explicit descriptor bindings, while Vulkan requires
/// both, so this pass adds exactly those qualifiers and nothing else. The GL code keeps
/// compiling the untouched source, which is what lets one shader serve both backends.
/// </summary>
internal static partial class SpirVCompiler
{
    [GeneratedRegex(@"^(\s*)out (vec4 VertexColour|vec3 VertexSpecular|vec2 TexCoord|float FogDepth|vec4 FragColor|vec4 Color)\s*;", RegexOptions.Multiline)]
    private static partial Regex OutputDecl();

    [GeneratedRegex(@"^(\s*)in (vec4 VertexColour|vec3 VertexSpecular|vec2 TexCoord|float FogDepth|vec4 Color)\s*;", RegexOptions.Multiline)]
    private static partial Regex InputDecl();

    private static readonly Dictionary<string, int> OutputLocations = new()
    {
        ["vec4 FragColor"] = 0,
        ["vec4 Color"] = 5,
        ["vec4 VertexColour"] = 1,
        ["vec3 VertexSpecular"] = 2,
        ["vec2 TexCoord"] = 3,
        ["float FogDepth"] = 4,
        ["vec4 Color"] = 5,
    };

    private static readonly Dictionary<string, int> InputLocations = new()
    {
        ["vec4 VertexColour"] = 1,
        ["vec3 VertexSpecular"] = 2,
        ["vec2 TexCoord"] = 3,
        ["float FogDepth"] = 4,
        ["vec4 Color"] = 5,
    };

    /// <summary>Rewrites GL GLSL 330 into the Vulkan dialect; no semantic changes.</summary>
    internal static string ToVulkanGlsl(string source, bool vertexStage = false)
    {
        var result = source.Replace("#version 330 core", "#version 450 core", StringComparison.Ordinal);
        result = OutputDecl().Replace(result, m =>
            $"{m.Groups[1].Value}layout (location = {OutputLocations[m.Groups[2].Value]}) out {m.Groups[2].Value};");
        result = InputDecl().Replace(result, m =>
            $"{m.Groups[1].Value}layout (location = {InputLocations[m.Groups[2].Value]}) in {m.Groups[2].Value};");
        // Descriptor bindings: block 0, texture 1. Both are stable for every pipeline.
        result = result.Replace("layout (std140) uniform ", "layout (std140, binding = 0) uniform ", StringComparison.Ordinal);
        result = result.Replace("uniform sampler2D ", "layout (binding = 1) uniform sampler2D ", StringComparison.Ordinal);
        if (vertexStage)
        {
            result = Regex.Replace(result, @"\bvoid\s+main\s*\(\s*\)", "void thsss_gl_main()");
            result += "\nvoid main() { thsss_gl_main(); gl_Position.z = (gl_Position.z + gl_Position.w) * 0.5; }\n";
        }
        return result;
    }

    /// <summary>
    /// Invokes glslangValidator, which must be on PATH. Publishing precompiled SPIR-V is a
    /// later step; until then a missing compiler is an explicit, actionable error instead
    /// of a silent fallback.
    /// </summary>
    internal static byte[] Compile(string glsl, bool vertexStage)
    {
        string temp = Path.Combine(Path.GetTempPath(), "thsss-spirv-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(temp);
        try
        {
            string input = Path.Combine(temp, vertexStage ? "stage.vert" : "stage.frag");
            string output = input + ".spv";
            File.WriteAllText(input, ToVulkanGlsl(glsl, vertexStage));
            var start = new ProcessStartInfo("glslangValidator")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
            };
            foreach (var arg in new[] { "-V", "--target-env", "vulkan1.3", input, "-o", output })
                start.ArgumentList.Add(arg);
            using var process = Process.Start(start)
                ?? throw new PlatformNotSupportedException("glslangValidator could not be started.");
            string log = process.StandardOutput.ReadToEnd() + process.StandardError.ReadToEnd();
            process.WaitForExit();
            if (process.ExitCode != 0 || !File.Exists(output))
                throw new InvalidOperationException(
                    $"SPIR-V compilation failed for the {(vertexStage ? "vertex" : "fragment")} stage: " + log.Trim());
            return File.ReadAllBytes(output);
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            throw new PlatformNotSupportedException(
                "The Vulkan renderer needs glslangValidator on PATH to compile shaders.", ex);
        }
        finally
        {
            try { Directory.Delete(temp, true); } catch (IOException) { }
        }
    }
}
