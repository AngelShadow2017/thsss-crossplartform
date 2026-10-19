using System;
using System.Collections.Generic;

namespace SlimDX.Compat;

/// <summary>
/// Reproduces pre-.NET-4.5 List.ForEach's live index walk, not snapshot semantics.
/// Removing the current item advances past the item shifted into its place;
/// appends are visited during the same walk. These are intentional legacy effects.
/// The modern List enumerator cannot be used because it rejects mutation.
/// </summary>
public static class ListCompat
{
    public static IEnumerable<T> LegacyItems<T>(IList<T> list)
    {
        ArgumentNullException.ThrowIfNull(list);
        return Walk(list);
    }

    private static IEnumerable<T> Walk<T>(IList<T> list)
    {
        for (int index=0; index<list.Count; index++) yield return list[index];
    }

    /// <summary>Explicit snapshot for callers that really require frozen membership.</summary>
    public static T[] Snapshot<T>(List<T> list) => list.ToArray();
    public static T[] Snapshot<T>(IList<T> list) => System.Linq.Enumerable.ToArray(list);

    public static void ForEach<T>(List<T> list, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(action);
        for (int index=0; index<list.Count; index++) action(list[index]);
    }
}
