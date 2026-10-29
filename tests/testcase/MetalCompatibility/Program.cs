using System.Reflection;
using System.Text.RegularExpressions;
using THSSS.Graphics;
using THSSS.Graphics.Metal;

// Metal shader fixture. It takes the shaders the game actually ships, runs them through the
// real tool chain (GLSL -> SPIR-V -> MSL via glslang/spirv-cross) and checks the produced
// MSL looks like a translation unit. It does not execute MSL: there is no Metal device here,
// so this proves translation only, never rendering.
var game=typeof(IRenderBackend).Assembly;
int checks=0;
// --export <dir> writes the generated MSL keyed by the content hash so the packaging step
// (tools/make-dist.pl) can ship exactly the cache the runtime expects.
string? export=args.Length==2&&args[0]=="--export"?Path.GetFullPath(args[1]):null;
if(export!=null)Directory.CreateDirectory(export);
void Translate(string source,bool vertex,string name)
{
    if(vertex)
    {
        // The project's GLSL leaves attribute locations implicit; SPIR-V requires explicit
        // locations, so they are assigned in declaration order before translating.
        int location=0;
        source=Regex.Replace(source,@"(?m)^(\s*)in (\w+) (\w+);",m=>$"{m.Groups[1]}layout(location = {location++}) in {m.Groups[2]} {m.Groups[3]};");
    }
    string msl=MetalSourceCompiler.Translate(source,vertex);
    // Sanity markers of a real MSL translation rather than an error string or the input GLSL.
    if(!msl.Contains("metal_stdlib")||!msl.Contains(vertex?"vertex ":"fragment ")||!msl.Contains("main0"))throw new Exception("Invalid MSL: "+name);
    if(export!=null)File.WriteAllText(Path.Combine(export,MetalSourceCompiler.CacheKey(source,vertex)),msl);
    checks++;Console.WriteLine("PASS real GLSL -> SPIR-V -> MSL: "+name);
}
// Sources are read from the repository, not from constants: a shader edit must fail here.
string root=Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,"../../../../../../"));
foreach(var(file,vertexName,fragmentName)in new[]{("Sprite.cs","SpriteVertexSource","SpriteFragmentSource"),("Device3D.cs","vertex","fragment")})
{
    string text=File.ReadAllText(Path.Combine(root,"src/Platform/Compat",file));
    foreach(var(symbol,isVertex)in new[]{(vertexName,true),(fragmentName,false)})
    {
        var match=Regex.Match(text,"const string "+symbol+"\\s*=\\s*@\"(.*?)\";",RegexOptions.Singleline);
        if(!match.Success)throw new Exception("Missing actual shader: "+symbol);
        Translate(match.Groups[1].Value,isVertex,file+":"+symbol);
    }
}
// Effect shaders are generated per effect kind, so each variant is translated too; a kind
// that produces broken GLSL is caught here instead of at first draw on a Mac.
var effect=game.GetType("SlimDX.Direct3D9.EffectShaders",true)!;
var kind=game.GetType("SlimDX.Direct3D9.EffectKind",true)!;
foreach(var value in Enum.GetValues(kind))
{
    var source=(ShaderSource)effect.GetMethod("BackendSource",BindingFlags.Static|BindingFlags.NonPublic)!.Invoke(null,new[]{value})!;
    Translate(source.Vertex,true,"effect vertex:"+value);Translate(source.Fragment,false,"effect fragment:"+value);
}
Console.WriteLine($"PASS MetalCompatibility: {checks} shader translation checks (real tools, not Metal runtime)");
