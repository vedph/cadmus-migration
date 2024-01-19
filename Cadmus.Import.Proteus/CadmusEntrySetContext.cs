using Cadmus.Core;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using System.Collections.Generic;

namespace Cadmus.Import.Proteus;

/// <summary>
/// Cadmus entry set context. This adds a list of imported items to the
/// base <see cref="EntrySetContext"/>, so that importers can collect items
/// and their parts.
/// <para>Tag: <c>entry-set-context.cadmus</c>.</para>
/// </summary>
[Tag("entry-set-context.cadmus")]
public class CadmusEntrySetContext : EntrySetContext
{
    /// <summary>
    /// Gets the items.
    /// </summary>
    public List<IItem> Items { get; } = [];

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Cloned context.</returns>
    public override IEntrySetContext Clone()
    {
        CadmusEntrySetContext context = (CadmusEntrySetContext)base.Clone();

        // copy items into a cloned list
        context.Items.AddRange(Items);
        return context;
    }

    /// <summary>
    /// Converts to string.
    /// </summary>
    /// <returns>String.</returns>
    public override string ToString()
    {
        return $"{base.ToString()} I={Items.Count}";
    }
}
