using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using THSSS.Graphics.Vulkan;

namespace THSSS.Graphics.Metal;

/// <summary>Cached MSL is portable across Intel/Apple Silicon; Metal compiles it on the target.</summary>
public static class MetalSourceCompiler
{
    public static string CacheKey(string glsl,bool vertex)
        => (vertex?"vert-":"frag-")+Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(glsl))).ToLowerInvariant()+".msl";

    public static string Translate(string glsl,bool vertex)
    {
        string key=CacheKey(glsl,vertex);
        string packaged=Path.Combine(AppContext.BaseDirectory,"MetalShaders",key);
        if(File.Exists(packaged))return File.ReadAllText(packaged);
        string directory=Path.Combine(Path.GetTempPath(),"thsss-metal-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        try
        {
            string input=Path.Combine(directory,"stage.spv");
            File.WriteAllBytes(input,SpirVCompiler.Compile(glsl,vertex));
            var start=new ProcessStartInfo("spirv-cross"){RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};
            foreach(string arg in new[]{input,"--msl","--msl-version","20100","--msl-decoration-binding","--flip-vert-y"})start.ArgumentList.Add(arg);
            using var process=Process.Start(start)??throw new InvalidOperationException("Cannot start spirv-cross.");
            var output=process.StandardOutput.ReadToEndAsync();var errors=process.StandardError.ReadToEndAsync();
            if(!process.WaitForExit(30000)){process.Kill(true);throw new TimeoutException("spirv-cross exceeded 30 seconds.");}
            string msl=output.GetAwaiter().GetResult();
            if(process.ExitCode!=0||string.IsNullOrWhiteSpace(msl))throw new InvalidOperationException("Metal shader translation failed: "+errors.GetAwaiter().GetResult());
            return msl;
        }
        catch(System.ComponentModel.Win32Exception ex)
        {throw new PlatformNotSupportedException("Metal shaders were not packaged. Build the MetalShaders cache with tools/export-metal-shaders.sh, or install glslangValidator and spirv-cross.",ex);}
        finally{Directory.Delete(directory,true);}
    }
}
