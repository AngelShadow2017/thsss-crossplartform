internal sealed class ModelWorkspace : IDisposable
{
    private readonly string _previous=Directory.GetCurrentDirectory();
    private readonly string _directory=Path.Combine(Path.GetTempPath(),"thsss-mesh-workspace-"+Guid.NewGuid().ToString("N"));
    internal ModelWorkspace(string gameRoot)
    {
        Directory.CreateDirectory(_directory);
        Directory.CreateSymbolicLink(Path.Combine(_directory,"Model"),Path.Combine(gameRoot,"Model"));
        Directory.SetCurrentDirectory(_directory);
    }
    public void Dispose()
    {
        Directory.SetCurrentDirectory(_previous);
        Directory.Delete(_directory,true);
    }
}
