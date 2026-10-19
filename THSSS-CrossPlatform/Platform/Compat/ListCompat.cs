// Compatibility shims for behaviours that .NET (Core) changed relative to .NET 2.0.
using System;
using System.Collections.Generic;

namespace SlimDX.Compat;

public static class ListCompat
{
    /// <summary>
    /// .NET 2.0's List&lt;T&gt;.ForEach walked by index and did not verify a version
    /// counter, so a callback could add or remove entries and the loop simply kept
    /// going. .NET modern uses an enumerator and throws InvalidOperationException.
    ///
    /// The game (its event/bullet pipelines in particular) relies on the old
    /// behaviour, so this walks a snapshot: entries appended during the walk run on
    /// the next pass, entries removed during the walk are skipped. That is exactly
    /// what the original index-based loop did for the add/remove patterns in play.
    ///
    /// The generic receivers live in the game's own generic containers, so this is
    /// called explicitly (<c>ListCompat.ForEach(list, a =&gt; ...)</c>) rather than
    /// relying on extension-method lookup, which the instance method would win.
    /// </summary>
    /// <summary>Copy used by rewritten call sites that mutate while iterating.</summary>
    public static T[] Snapshot<T>(List<T> list) => list.ToArray();

    /// <summary>Copy used by rewritten call sites that mutate while iterating.</summary>
    public static T[] Snapshot<T>(IList<T> list) => System.Linq.Enumerable.ToArray(list);

    public static void ForEach<T>(List<T> list, Action<T> action)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(action);
        foreach (var item in list.ToArray()) action(item);
    }
}
