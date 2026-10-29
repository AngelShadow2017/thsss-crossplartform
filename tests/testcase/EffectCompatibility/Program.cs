using System.Text;
using SlimDX;
using SlimDX.Direct3D9;

// API half of the Effect contract: no GPU and no shader output is involved. These
// checks pin the exception type each misuse produced, so game code that relied on
// D3D9's managed wrapper keeps failing in the same place. Rendering through an
// effect is covered separately by GpuChecks/SpriteIntegrationChecks; --api-only
// runs just this half on machines with no usable GL context.
ApiChecks();
if (!args.Contains("--api-only")) GpuChecks.Run();

static void ApiChecks()
{
    var device = new Device();
    // Paths stay relative to the working directory exactly as the game resolves them
    // (the runner starts in src/), so a moved or renamed FX file fails here first.
    string path = Path.GetFullPath("../game/FX/Blur.fx");
    using var effect = Effect.FromFile(device, path);
    Equal("technique1", effect.GetTechnique(0).Name);
    Equal("technique1", effect.GetTechnique("technique1").Name);
    // Blur.fx declares exactly one technique and two parameters (BlurLevel +
    // xColoredTexture). Counting them guards against a loader that invents or drops
    // entries; only recognised parameters count, so an ignored annotation would show up.
    Equal(2, effect.GetParameterCount());
    Equal("BlurLevel", effect.GetParameter(0).Name);
    effect.SetFloat(effect.GetParameter("BlurLevel"), 0.75f);
    Equal(0.75f, effect.GetValue<float>("BlurLevel"));
    // D3D9 Set* reported E_FAIL for unknown names and type mismatches; the port turns
    // that into ArgumentException. Wrong-type writes must not silently overwrite the
    // existing float, and GetValue<T> of the wrong type is an InvalidCastException
    // rather than a default value.
    Throws<ArgumentException>(() => effect.SetFloat("BlurLevell", 0.75f));
    Throws<ArgumentException>(() => effect.SetInt("integer", 7));
    Throws<ArgumentException>(() => effect.SetBool("boolean", true));
    Throws<ArgumentException>(() => effect.SetString("text", "saved"));
    Throws<ArgumentException>(() => effect.SetMatrix("matrix", Matrix.Identity));
    Throws<ArgumentException>(() => effect.SetVector("vector", new Vector4(1,2,3,4)));
    Throws<ArgumentException>(() => effect.SetFloatArray("array", new[]{1f,2f}));
    // Unbinding a texture with null is legal, not an error: the render path depends on it.
    effect.SetTexture("xColoredTexture", null); Equal<Texture>(null, effect.GetValue<Texture>("xColoredTexture"));
    Throws<ArgumentException>(() => effect.SetInt("BlurLevel", 1));
    Throws<InvalidCastException>(() => effect.GetValue<int>("BlurLevel"));
    Throws<ArgumentException>(() => effect.GetTechnique("missing"));
    Throws<ArgumentOutOfRangeException>(() => effect.GetTechnique(1));
    Throws<ArgumentOutOfRangeException>(() => effect.GetParameter(-1));
    Throws<ArgumentException>(() => effect.GetParameter("missing"));
    // Begin/BeginPass/EndPass/End is a strict state machine: a pass cannot be entered
    // before Begin, Begin cannot nest on the same effect, and a second BeginPass without
    // EndPass is refused. This mirrors the D3D runtime, which returned an error HRESULT.
    Throws<InvalidOperationException>(() => effect.BeginPass(0));
    Equal(1, effect.Begin());
    Throws<InvalidOperationException>(() => effect.Begin());
    Throws<ArgumentOutOfRangeException>(() => effect.BeginPass(1));
    effect.BeginPass(0); effect.CommitChanges();
    Throws<InvalidOperationException>(() => effect.BeginPass(0));
    // A second effect with its own active pass must not let the first one end it: the
    // pass state is per-effect, not global, and Dispose during an active pass is refused
    // so no dangling program is left bound.
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
    // The port parses only the FX subset the game ships. Empty source is a caller bug
    // (ArgumentException); source it cannot compile or validate is "unsupported"
    // (NotSupportedException). The BlurLevel*49 case proves the body is parsed, not just
    // the header: a one-character numeric edit is rejected as unverifiable.
    Throws<NotSupportedException>(() => Effect.FromString(device, "invalid HLSL"));
    Throws<NotSupportedException>(() => Effect.FromString(device, File.ReadAllText(path).Replace("BlurLevel*50", "BlurLevel*49")));
    using var memory = Effect.FromMemory(device, File.ReadAllBytes(path));
    using var constructed = new Effect(device, File.ReadAllBytes(path), ShaderFlags.Debug);
    using var spaced = Effect.FromString(device, "// harmless comment\n" + File.ReadAllText(path));
    // Warp.fx ships six parameters; a different count means the parser lost one or
    // invented one. Comment-only leading text is accepted, as HLSL allows it.
    using var warp = Effect.FromFile(device, "../game/FX/Warp.fx"); Equal(6, warp.GetParameterCount());
    using var disposing = Effect.FromFile(device, path);
    disposing.Begin(); disposing.BeginPass(0); disposing.Dispose();
    // Disposal is idempotent, and afterwards every entry point is ObjectDisposedException
    // rather than a crash or a silent no-op - the same contract the managed wrapper had.
    Equal(true, disposing.IsDisposed); disposing.Dispose();
    Throws<ObjectDisposedException>(() => disposing.Begin());
    Throws<ObjectDisposedException>(() => disposing.SetFloat("BlurLevel", 0));
    Throws<ObjectDisposedException>(() => disposing.GetTechnique(0));
    Console.WriteLine("PASS API: sources, parameters, types, technique/count, nested lifecycle, disposal");
}

// Both helpers fail loudly with expected-vs-actual, so a mismatch names the contract
// that broke instead of only reporting "false".
static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new Exception($"Expected {expected}; got {actual}");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); } catch (T) { return; }
    throw new Exception("Expected " + typeof(T).Name);
}

// GPU-side half: sprite/effect integration through real draw calls. Kept out of
// ApiChecks so the pure API contract can be audited without a GL context at all.
if (!args.Contains("--api-only")) SpriteIntegrationChecks.Run();
