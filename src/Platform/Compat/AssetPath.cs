using System;
using System.IO;
using System.Linq;

namespace SlimDX.Direct3D9;

internal static class AssetPath
{
    internal static string Resolve(string path)
    {
        path = path.Replace('\\', Path.DirectorySeparatorChar);
        if (File.Exists(path)) return path;
        string full = Path.GetFullPath(path);
        string current = Path.GetPathRoot(full);
        foreach (string component in full[current.Length..].Split(Path.DirectorySeparatorChar,StringSplitOptions.RemoveEmptyEntries))
        {
            string exact = Path.Combine(current,component);
            if (File.Exists(exact) || Directory.Exists(exact)) { current = exact; continue; }
            if (!Directory.Exists(current)) throw new FileNotFoundException("Asset not found.",path);
            var matches = Directory.EnumerateFileSystemEntries(current)
                .Where(p => string.Equals(Path.GetFileName(p),component,StringComparison.OrdinalIgnoreCase)).ToArray();
            if (matches.Length != 1) throw new FileNotFoundException("Asset name is missing or ambiguous.",path);
            current = matches[0];
        }
        if (!File.Exists(current)) throw new FileNotFoundException("Asset not found.",path);
        return current;
    }
}
