using SlimDX.Direct3D9;

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
Reject<FileNotFoundException>(()=>Effect.FromFile(device,"nonexistent-shader-for-audit.fx"),"missing FX source fails explicitly");
Reject<NotSupportedException>(()=>Effect.FromString(device,"not valid HLSL"),"invalid FX source is rejected");
using var effect=Effect.FromFile(device,"../game/FX/Blur.fx");
effect.SetFloat("BlurLevel",0.0025f);
Check(Math.Abs(effect.GetValue<float>("BlurLevel")-0.0025f)<0.00001f,"FX retains typed parameters");
Check(effect.GetTechnique("technique1").Name=="technique1","technique lookup matches loaded source");
Check(effect.Begin()==1,"FX supplies a drawable pass");
effect.BeginPass(0); effect.CommitChanges(); effect.EndPass(); effect.End();
effect.Dispose();
Check(effect.IsDisposed,"FX disposal updates lifecycle state");
Reject<ObjectDisposedException>(()=>effect.Begin(),"disposed FX cannot begin again");
Console.WriteLine($"PASS: {checkedBehaviours} shader API regression assertions (no GPU needed)");
