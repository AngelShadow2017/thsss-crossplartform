// Effect compatibility layer
using System;

namespace SlimDX.Direct3D9
{
    public class Effect : IDisposable
    {
        public Effect(Device device) { }
        public Effect(Device device, byte[] data) { }
        public Effect(Device device, byte[] data, ShaderFlags flags) { }
        public EffectHandle Technique { get; set; }
        public bool IsDisposed => false;
        public void SetTechnique(EffectHandle technique) { Technique = technique; }
        public EffectHandle GetTechnique(int index) => new EffectHandle();
        public EffectHandle GetTechnique(string name) => new EffectHandle();
        public int Begin(int flags) { return 0; }
        public int Begin() { return 0; }
        public void BeginPass(int pass) { }
        public void CommitChanges() { }
        public void EndPass() { }
        public void End() { }
        public void SetValue<T>(EffectHandle parameter, T value) { }
        public void SetValue<T>(string name, T value) { }
        public T GetValue<T>(EffectHandle parameter) => default(T);
        public T GetValue<T>(string name) => default(T);
        public void SetTexture(EffectHandle parameter, Texture texture) { }
        public void SetTexture(string name, Texture texture) { }
        public void SetMatrix(EffectHandle parameter, Matrix matrix) { }
        public void SetMatrix(string name, Matrix matrix) { }
        public void SetVector(EffectHandle parameter, Vector4 vector) { }
        public void SetFloatArray(EffectHandle parameter, float[] values) { }
        public void SetFloatArray(string name, float[] values) { }
        public void SetInt(EffectHandle parameter, int value) { }
        public void SetInt(string name, int value) { }
        public void SetFloat(EffectHandle parameter, float value) { }
        public void SetFloat(string name, float value) { }
        public void SetBool(EffectHandle parameter, bool value) { }
        public void SetBool(string name, bool value) { }
        public void SetString(EffectHandle parameter, string value) { }
        public void SetString(string name, string value) { }
        public EffectHandle GetParameter(int index) => new EffectHandle();
        public EffectHandle GetParameter(string name) => new EffectHandle();
        public int GetParameterCount() => 0;
        public void OnResetDevice() { }
        public void OnLostDevice() { }
        public void Dispose() { }
        public static Effect FromFile(Device device, string filename) => new Effect(device);
        public static Effect FromFile(Device device, string filename, ShaderFlags flags) => new Effect(device);
        public static Effect FromString(Device device, string source) => new Effect(device);
        public static Effect FromMemory(Device device, byte[] data) => new Effect(device);
    }

    public struct EffectHandle
    {
        public string Name;
        public static EffectHandle Null = new EffectHandle();
        public static implicit operator EffectHandle(string name) => new EffectHandle { Name = name };
    }

    [Flags]
    public enum ShaderFlags
    {
        None = 0,
        Debug = 1,
        SkipValidation = 2,
        SkipOptimization = 4,
        PackMatrices = 8,
    }
}
