using System;
using Silk.NET.OpenAL;

unsafe
{
    var loader = ALContext.GetApi();
    var device = loader.OpenDevice(null);
    Console.WriteLine($"[probe] OpenDevice(null) => {(device == null ? "NULL" : "ok")}");
    if (device == null) return;

    var context = loader.CreateContext(device, null);
    Console.WriteLine($"[probe] CreateContext => {(context == null ? "NULL" : "ok")}");
    if (context == null) return;

    Console.WriteLine($"[probe] MakeContextCurrent => {loader.MakeContextCurrent(context)}");
    var al = AL.GetApi();
    Console.WriteLine($"[probe] GetError after context => {al.GetError()}");
    Console.WriteLine($"[probe] Renderer => {al.GetStateProperty(StateString.Renderer) ?? "(null)"}");
    Console.WriteLine($"[probe] GenBuffer => {al.GenBuffer()}, error={al.GetError()}");
}
