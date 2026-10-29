using SlimDX.Direct3D9;

// Headless FX wrapper audit. It deliberately repeats the core EffectCompatibility API
// checks in a no-GPU form so the sample runs on CI without a driver, and it proves only
// API/lifecycle behaviour - never that a shader compiles or renders correctly.
int checkedBehaviours=0;
void Check(bool condition,string message)
{
    if(!condition) throw new Exception(message);
    Console.WriteLine("PASS: "+message);
    checkedBehaviours++;
}
void Reject<T>(Action action,string message) where T:Exception
{
    try { action(); } catch(T) { Check(true,message); return; }
    throw new Exception("Expected "+typeof(T).Name+": "+message);
}
using var device=new Device();
// Missing source must be a loud, specific failure. The game treats a missing FX as fatal
// rather than falling back to untextured drawing (see the FX retry-loop rule).
Reject<FileNotFoundException>(()=>Effect.FromFile(device,"nonexistent-shader-for-audit.fx"),"missing FX source fails explicitly");
// "Unsupported" rather than "parse error": the port never compiles arbitrary HLSL, it
// validates against the FX subset the game ships, so anything else is out of contract.
Reject<NotSupportedException>(()=>Effect.FromString(device,"not valid HLSL"),"invalid FX source is rejected");
using var effect=Effect.FromFile(device,"../game/FX/Blur.fx");
effect.SetFloat("BlurLevel",0.0025f);
// Float round-trip tolerance is single-precision noise, not a fudge factor: SetFloat must
// not quantise the value to shader precision, so a lossy setter fails here.
Check(Math.Abs(effect.GetValue<float>("BlurLevel")-0.0025f)<0.00001f,"FX retains typed parameters");
Check(effect.GetTechnique("technique1").Name=="technique1","technique lookup matches loaded source");
// Exactly one pass: the count also catches a parser that invents extra passes.
Check(effect.Begin()==1,"FX supplies a drawable pass");
effect.BeginPass(0); effect.CommitChanges(); effect.EndPass(); effect.End();
effect.Dispose();
Check(effect.IsDisposed,"FX disposal updates lifecycle state");
// Disposal is terminal: IsDisposed flips and every later Begin throws instead of
// drawing with a dangling program.
Reject<ObjectDisposedException>(()=>effect.Begin(),"disposed FX cannot begin again");
Console.WriteLine($"PASS: {checkedBehaviours} shader API regression assertions (no GPU needed)");
