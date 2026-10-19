using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

ApiChecks();
if (!args.Contains("--api-only")) GpuChecks.Run();

static void ApiChecks()
{
    var device = new Device();
    string path = Path.GetFullPath("../game/FX/Blur.fx");
    using var effect = Effect.FromFile(device, path);
    Equal("technique1", effect.GetTechnique(0).Name);
    Equal("technique1", effect.GetTechnique("technique1").Name);
    Equal(2, effect.GetParameterCount());
    Equal("BlurLevel", effect.GetParameter(0).Name);
    effect.SetFloat(effect.GetParameter("BlurLevel"), 0.75f);
    Equal(0.75f, effect.GetValue<float>("BlurLevel"));
    Throws<ArgumentException>(() => effect.SetFloat("BlurLevell", 0.75f));
    Throws<ArgumentException>(() => effect.SetInt("integer", 7));
    Throws<ArgumentException>(() => effect.SetBool("boolean", true));
    Throws<ArgumentException>(() => effect.SetString("text", "saved"));
    Throws<ArgumentException>(() => effect.SetMatrix("matrix", Matrix.Identity));
    Throws<ArgumentException>(() => effect.SetVector("vector", new Vector4(1,2,3,4)));
    Throws<ArgumentException>(() => effect.SetFloatArray("array", new[]{1f,2f}));
    effect.SetTexture("xColoredTexture", null); Equal<Texture>(null, effect.GetValue<Texture>("xColoredTexture"));
    Throws<ArgumentException>(() => effect.SetInt("BlurLevel", 1));
    Throws<InvalidCastException>(() => effect.GetValue<int>("BlurLevel"));
    Throws<ArgumentException>(() => effect.GetTechnique("missing"));
    Throws<ArgumentOutOfRangeException>(() => effect.GetTechnique(1));
    Throws<ArgumentOutOfRangeException>(() => effect.GetParameter(-1));
    Throws<ArgumentException>(() => effect.GetParameter("missing"));
    Throws<InvalidOperationException>(() => effect.BeginPass(0));
    Equal(1, effect.Begin());
    Throws<InvalidOperationException>(() => effect.Begin());
    Throws<ArgumentOutOfRangeException>(() => effect.BeginPass(1));
    effect.BeginPass(0); effect.CommitChanges();
    Throws<InvalidOperationException>(() => effect.BeginPass(0));
    using var nested = Effect.FromFile(device, "../game/FX/Filter.fx");
    nested.Begin(); nested.BeginPass(0);
    Throws<InvalidOperationException>(() => effect.EndPass());
    Throws<InvalidOperationException>(() => effect.Dispose());
    nested.EndPass(); nested.End(); effect.CommitChanges();
    Throws<InvalidOperationException>(() => effect.End());
    effect.EndPass(); effect.End();
    Throws<InvalidOperationException>(() => effect.EndPass());
    Throws<FileNotFoundException>(() => Effect.FromFile(device, "missing-effect-file.fx"));
    Throws<ArgumentException>(() => Effect.FromString(device, ""));
    Throws<NotSupportedException>(() => Effect.FromString(device, "invalid HLSL"));
    Throws<NotSupportedException>(() => Effect.FromString(device, File.ReadAllText(path).Replace("BlurLevel*50", "BlurLevel*49")));
    using var memory = Effect.FromMemory(device, File.ReadAllBytes(path));
    using var constructed = new Effect(device, File.ReadAllBytes(path), ShaderFlags.Debug);
    using var spaced = Effect.FromString(device, "// harmless comment\n" + File.ReadAllText(path));
    using var warp = Effect.FromFile(device, "../game/FX/Warp.fx"); Equal(6, warp.GetParameterCount());
    using var disposing = Effect.FromFile(device, path);
    disposing.Begin(); disposing.BeginPass(0); disposing.Dispose();
    Equal(true, disposing.IsDisposed); disposing.Dispose();
    Throws<ObjectDisposedException>(() => disposing.Begin());
    Throws<ObjectDisposedException>(() => disposing.SetFloat("BlurLevel", 0));
    Throws<ObjectDisposedException>(() => disposing.GetTechnique(0));
    Console.WriteLine("PASS API: sources, parameters, types, technique/count, nested lifecycle, disposal");
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}; got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name);
}

if (!args.Contains("--api-only")) SpriteIntegrationChecks.Run();
