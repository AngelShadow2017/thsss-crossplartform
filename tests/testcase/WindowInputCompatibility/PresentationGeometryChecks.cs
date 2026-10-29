using THSSS.Graphics;

internal static class PresentationGeometryChecks
{
    internal static void Run()
    {
        foreach (var (width, height) in new[] { (640,480), (800,600), (1000,750), (1280,960), (1600,1200), (2000,1500) })
            Checks.Check(PresentationGeometry.Fit(width,height) == new PixelViewport(0,0,width,height), $"4:3 client/framebuffer fit {width}x{height}");
        Checks.Check(PresentationGeometry.Fit(1920,1080) == new PixelViewport(240,0,1440,1080), "Fullscreen pillarbox remains 4:3");
        Checks.Check(PresentationGeometry.Fit(600,1000) == new PixelViewport(0,275,600,450), "Portrait display letterboxes without changing game coordinates");
        Checks.Check(PresentationGeometry.Fit(0,0) == default, "Minimized framebuffer has no render area");
        var fractional = PresentationGeometry.Fit(1001,750);
        Checks.Check(fractional == new PixelViewport(0,0,1000,750), "Fractional DPI rounding does not stretch content");
        Checks.Check(PresentationGeometry.Fit(1600,1200).Width == 2 * PresentationGeometry.Fit(800,600).Width,
            "Same logical client at 200% DPI uses double physical render dimensions");
        var device=(SlimDX.Direct3D9.Device)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(SlimDX.Direct3D9.Device));
        device.GameWidth=640;device.GameHeight=480;
        var backend=Fake.Create<IRenderBackend>();
        typeof(SlimDX.Direct3D9.Device).GetField("_renderBackend",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)!.SetValue(device,backend);
        device.ResizeFramebuffer(800,600);
        device.ResizeFramebuffer(1600,1200); // Same client coordinates, new monitor pixel density.
        device.ResizeFramebuffer(1600,1200);
        Checks.Check(Fake.Of(backend).Calls.Count(c=>c.Name=="Resize")==2,"Framebuffer-only DPI changes reach backend exactly once per changed size");
        Checks.Check(device.FramebufferWidth==1600&&device.GameWidth==640,"DPI change never changes logical game width");
        Checks.Check(!device.ResizeFramebuffer(0,0)&&device.FramebufferWidth==1600,"Minimize skips rendering without overwriting valid framebuffer dimensions");
        Console.WriteLine("PASS: 4:3 presentation geometry, fullscreen, zero size and DPI rounding");
    }
}
