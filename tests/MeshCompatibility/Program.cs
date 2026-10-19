using Silk.NET.Windowing;
using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SlimDX;
using SlimDX.Direct3D9;
using NVector3 = System.Numerics.Vector3;
using Texture = SlimDX.Direct3D9.Texture;
using D3DColor = SlimDX.Direct3D9.Color4;

int assertions = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    assertions++;
    Console.WriteLine("PASS: " + name);
}
void Throws<T>(Action action, string name) where T : Exception
{
    try { action(); } catch (T) { Check(true,name); return; }
    throw new Exception("FAIL: expected " + typeof(T).Name + ": " + name);
}
var rootDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
while (rootDirectory != null && !Directory.Exists(Path.Combine(rootDirectory.FullName,"game","Model"))) rootDirectory = rootDirectory.Parent;
string root = rootDirectory != null ? Path.Combine(rootDirectory.FullName,"game") : throw new Exception("Cannot locate shipped game/Model directory.");
using var modelWorkspace = new ModelWorkspace(root);
string fixture = Path.Combine(rootDirectory.FullName,"tests/MeshCompatibility/Fixtures/concave.x");
using var mesh = Mesh.FromFile(null, fixture, MeshFlags.SystemMemory);
Check(mesh.VertexCount == 6 && mesh.FaceCount == 5,"concave polygon triangulated into four triangles plus second face");
Check(mesh.FaceAttributes.SequenceEqual(new[]{0,0,0,0,1}),"all triangulated faces retain material indices");
Check(mesh.GetMaterials().Length == 3 && mesh.GetMaterials()[0].TextureFileName == "red.bmp","materials, unused subsets and texture filename preserved");
Check(MathF.Abs(mesh.GetMaterials()[0].MaterialD3D.Specular.Green - 0.3f)<0.00001f,"material specular preserved");
Check(mesh.VertexFormat == (VertexFormat.Position|VertexFormat.Texture1),"missing normals are represented in vertex format");
float area = 0;
for(int f=0;f<4;f++)
{
    var a=mesh.Vertices[mesh.Indices[f*3]].Position;
    var b=mesh.Vertices[mesh.Indices[f*3+1]].Position;
    var c=mesh.Vertices[mesh.Indices[f*3+2]].Position;
    float signed=NVector3.Cross(b-a,c-a).Z/2;
    Check(signed>0,"triangulation preserves winding " + f);
    area+=signed;
    var center=(a+b+c)/3;
    Check(!(center.X>0 && center.Y>0),"triangulation stays outside concave cutout " + f);
}
Check(MathF.Abs(area-1.92f)<0.0001f,"triangulated polygon area equals source polygon area");
using var normalized=mesh.Clone(null,MeshFlags.SystemMemory,mesh.VertexFormat|VertexFormat.Normal);
normalized.ComputeNormals();
Check(normalized.Vertices.All(v=>v.Normal.Z>0.999f),"computed normals are normalized and match winding");
Check(normalized.Vertices.Select(v=>v.TextureCoordinate).SequenceEqual(mesh.Vertices.Select(v=>v.TextureCoordinate)),"clone preserves UV");
Throws<InvalidOperationException>(()=>mesh.ComputeNormals(),"normal generation requires Normal format");
Throws<NotSupportedException>(()=>mesh.Clone(null,MeshFlags.None,VertexFormat.Position|VertexFormat.Diffuse),"unsupported clone format rejected");
using var positionOnly=normalized.Clone(null,MeshFlags.None,VertexFormat.Position);
Check(positionOnly.VertexFormat == VertexFormat.Position && positionOnly.Vertices.All(v=>v.Normal==NVector3.Zero),"clone removes excluded attributes");
Throws<FileNotFoundException>(()=>Mesh.FromFile(null,"Model/no-such-mesh.x",MeshFlags.None),"missing file is explicit error");
void TextCase(string contents, Action<string> test)
{
    string path=Path.Combine(rootDirectory.FullName,"tests/MeshCompatibility/Fixtures/runtime.x");
    try { File.WriteAllText(path,contents); test(path); } finally { File.Delete(path); }
}
TextCase("bad data",p=>Throws<InvalidDataException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"corrupt header rejected"));
TextCase("xof 0303bin 0032",p=>Throws<NotSupportedException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"binary format explicitly unsupported"));
TextCase("xof 0303tzip0032",p=>Throws<NotSupportedException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"compressed format explicitly unsupported"));
TextCase("xof 0303txt 0032\nMesh { 3; 0;0;0;,1;0;0;,0;1;0;;1;3;0,1,9;; }",p=>Throws<InvalidDataException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"bad face index rejected"));
TextCase("xof 0303txt 0032\nMesh { 3; 0;0;0;",p=>Throws<InvalidDataException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"truncated geometry rejected"));
TextCase("xof 0303txt 0032\nAnimationSet { }",p=>Throws<NotSupportedException>(()=>Mesh.FromFile(null,p,MeshFlags.None),"unhandled objects never silently dropped"));
string seam="""
xof 0303txt 0032
Frame Outer {
 Frame Inner {
  Mesh { 3; 0;0;0;,1;0;0;,0;1;0;; 2; 3;0,1,2;,3;0,2,1;;
   MeshNormals { 2; 0;0;1;,0;0;-1;; 2; 3;0,0,0;,3;1,1,1;; }
   MeshTextureCoords { 3; 0;0;,1;0;,0;1;; }
  }
  FrameTransformMatrix { 2,0,0,0,0,3,0,0,0,0,4,0,1,0,0,1;; }
 }
 FrameTransformMatrix { 1,0,0,0,0,1,0,0,0,0,1,0,0,2,0,1;; }
}
""";
TextCase(seam,p=>{
    using var split=Mesh.FromFile(null,p,MeshFlags.None);
    Check(split.VertexCount==6,"separate normal indices split shared positions without losing hard edges");
    Check(split.Vertices[0].Position == new NVector3(1,2,0) && split.Vertices[1].Position==new NVector3(3,2,0),"nested frame transforms apply even after mesh declaration");
    Check(split.Vertices.Take(3).All(v=>v.Normal.Z==1) && split.Vertices.Skip(3).All(v=>v.Normal.Z==-1),"normal indices preserve opposite face normals");
});
foreach(var model in new[]{"stair","祭坛"})
{
    using var actual=Mesh.FromFile(null,"Model/"+model+".x",MeshFlags.Managed);
    Check(actual.FaceCount>100 && actual.VertexCount>100,model+" contains actual geometry");
    Check((actual.VertexFormat&VertexFormat.Normal)!=0 && (actual.VertexFormat&VertexFormat.Texture1)!=0,model+" has normals and UV");
    Check(actual.Indices.All(i=>i>=0 && i<actual.VertexCount),model+" index ranges valid");
    Check(actual.FaceAttributes.All(i=>i>=0 && i<actual.GetMaterials().Length),model+" material ranges valid");
    Check(actual.Vertices.All(v=>float.IsFinite(v.Position.X) && float.IsFinite(v.Normal.Z)),model+" transformed geometry is finite");
    Console.WriteLine($"MODEL {model}: vertices={actual.VertexCount}, triangles={actual.FaceCount}, materials={actual.GetMaterials().Length}, textures={string.Join(',',actual.GetMaterials().Select(m=>m.TextureFileName))}");
}
if(args.Contains("--cpu-only")) { Console.WriteLine($"PASS: {assertions} CPU assertions (GPU explicitly skipped)"); return; }

var options=WindowOptions.Default;
options.Size=new Silk.NET.Maths.Vector2D<int>(128,128);
options.IsVisible=false;
using var window=Window.Create(options);
window.Initialize();
using var gl=window.CreateOpenGL();
using var device=new Device(gl,128,128) { GameWidth=128,GameHeight=128 };
using var target=new Texture(device,128,128,1,Usage.RenderTarget,Format.A8R8G8B8,Pool.Default);
using var surface=target.GetSurfaceLevel(0);
device.SetRenderTarget(0,surface);
device.SetRenderState(RenderState.CullMode,Cull.None);
device.SetRenderState(RenderState.Lighting,false);
device.SetRenderState(RenderState.ZEnable,false);
device.SetRenderState(RenderState.AlphaBlendEnable,false);
device.SetTransform(TransformState.World,Matrix.Identity);
device.SetTransform(TransformState.View,Matrix.Identity);
device.SetTransform(TransformState.Projection,Matrix.Identity);
using var whiteImage=new Image<Rgba32>(1,1);
whiteImage[0,0]=new Rgba32(255,255,255,255);
using var white=Texture.FromImage(device,whiteImage);
device.SetTexture(0,white);
byte[] Pixels()
{
    var bytes=new byte[128*128*4];
    unsafe { fixed(byte* p=bytes) gl.ReadPixels(0,0,128,128,Silk.NET.OpenGL.PixelFormat.Rgba,PixelType.UnsignedByte,p); }
    return bytes;
}
void Clear() => device.Clear(ClearFlags.Target,System.Drawing.Color.Black,1,0);
void CheckColor(int x,int y,int channel,string name)
{
    var pixels=Pixels(); int start=(y*128+x)*4;
    Check(pixels[start+channel]>200 && pixels[start+(channel+1)%3]<30,name);
}
foreach(bool normal in new[]{false,true})
{
    using var loaded=Mesh.FromFile(device,fixture,MeshFlags.Managed);
    using var gpuMesh=loaded.Clone(device,MeshFlags.Managed,loaded.VertexFormat|(normal?VertexFormat.Normal:VertexFormat.None));
    if(normal) gpuMesh.ComputeNormals();
    Clear(); device.Material=new Material { Diffuse=new D3DColor(1f,0f,0f,1f) }; gpuMesh.DrawSubset(0);
    Check(gpuMesh.VertexBuffer.Stride==(normal?32:20),"explicit mesh stream stride "+(normal?32:20));
    CheckColor(30,90,0,"real concave subset rendered, normals="+normal);
    var pixel=Pixels(); Check(pixel[(90*128+90)*4]==0,"concave cutout stays clear, normals="+normal);
    device.Material=new Material { Diffuse=new D3DColor(0f,1f,0f,1f) }; gpuMesh.DrawSubset(1);
    CheckColor(60,45,1,"second subset keeps caller material, normals="+normal);
    var before=Pixels(); gpuMesh.DrawSubset(2); Check(before.SequenceEqual(Pixels()),"empty subset is no-op");
    Throws<ArgumentOutOfRangeException>(()=>gpuMesh.DrawSubset(3),"invalid subset rejected");
    gpuMesh.Dispose(); Check(gpuMesh.IsDisposed,"mesh disposed flag");
    Throws<ObjectDisposedException>(()=>gpuMesh.DrawSubset(0),"disposed mesh cannot draw");
}
foreach(string model in new[]{"stair","祭坛"})
{
    using var actual=Mesh.FromFile(device,"Model/"+model+".x",MeshFlags.Managed);
    var min=actual.Vertices.Select(v=>v.Position).Aggregate(NVector3.Min);
    var max=actual.Vertices.Select(v=>v.Position).Aggregate(NVector3.Max);
    var center=(min+max)/2;
    float scale=1.4f/MathF.Max(MathF.Max(max.X-min.X,max.Y-min.Y),max.Z-min.Z);
    device.SetTransform(TransformState.Projection,Matrix.OrthoLH(2,2,-5,5));
    device.SetTransform(TransformState.World,Matrix.Translation(-center.X,-center.Y,-center.Z)*Matrix.Scaling(scale,scale,scale)*Matrix.RotationX(0.5f)*Matrix.RotationY(0.6f));
    Clear(); device.Material=new Material { Diffuse=new D3DColor(1f,1f,1f,1f) };
    for(int i=0;i<actual.GetMaterials().Length;i++) actual.DrawSubset(i);
    var pixels=Pixels(); int lit=Enumerable.Range(0,128*128).Count(i=>pixels[i*4]>20);
    Check(lit>50,model+" real model draws nonempty geometry: pixels="+lit);
    Check(gl.GetError()==GLEnum.NoError,model+" GPU draw has no GL error");
}
device.SetRenderTarget(0,null);
Console.WriteLine($"PASS: {assertions} mesh compatibility assertions including GPU");

// Verify the game's own Model_Mesh wrapper, including shipped texture paths/case.
device.SetRenderTarget(0,surface);
device.SetRenderState(RenderState.Lighting,false);
foreach(string model in new[]{"Stair","祭坛"})
{
    var wrapper=new Shooting.Model_Mesh(device,model);
    using var geometry=Mesh.FromFile(null,"Model/"+model+".x",MeshFlags.Managed);
    var min=geometry.Vertices.Select(v=>v.Position).Aggregate(NVector3.Min);
    var max=geometry.Vertices.Select(v=>v.Position).Aggregate(NVector3.Max);
    var center=(min+max)/2;
    float scale=1.4f/MathF.Max(MathF.Max(max.X-min.X,max.Y-min.Y),max.Z-min.Z);
    device.SetTransform(TransformState.World,Matrix.Translation(-center.X,-center.Y,-center.Z)*Matrix.Scaling(scale,scale,scale)*Matrix.RotationX(0.5f)*Matrix.RotationY(0.6f));
    Clear(); wrapper.SetModel(); wrapper.Draw();
    var pixels=Pixels();
    int visible=Enumerable.Range(0,128*128).Count(i=>pixels[i*4]>10 || pixels[i*4+1]>10 || pixels[i*4+2]>10);
    Check(visible>50,"game Model_Mesh wrapper loads real textures and draws "+model+": pixels="+visible);
    wrapper.Dispose();
}
device.SetRenderTarget(0,null);
