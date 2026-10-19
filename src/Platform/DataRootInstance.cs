using System;
using System.IO;

namespace Shooting;

public sealed class DataRootInstance : IDisposable
{
    private readonly FileStream _lock;
    public DataRootInstance(string dataRoot)
    {
        string path = Path.GetFullPath(dataRoot);
        if (!Directory.Exists(path)) throw new DirectoryNotFoundException($"THSSS data root does not exist: {path}");
        try { _lock = new FileStream(Path.Combine(path, ".thsss.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None); }
        catch (IOException ex) { throw new IOException($"Cannot acquire THSSS data-root lock for {path}; another instance may be running.", ex); }
    }
    public void Dispose() => _lock.Dispose();
}
