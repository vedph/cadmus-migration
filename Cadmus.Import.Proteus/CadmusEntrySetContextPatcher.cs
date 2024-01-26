﻿using Cadmus.Core;
using Fusi.Tools.Configuration;
using Proteus.Core.Regions;
using Proteus.Entries.Export;
using System;
using System.Threading.Tasks;

namespace Cadmus.Import.Proteus;

/// <summary>
/// Cadmus entry set context patcher. This patcher is used to patch the items
/// in the context to ensure that they have facet ID, title, description,
/// sort key, creator ID, and user ID.
/// <para>Tag: <c>entry-set-context-patcher.cadmus</c>.</para>
/// </summary>
[Tag("entry-set-context-patcher.cadmus")]
public class CadmusEntrySetContextPatcher : IEntrySetContextPatcher
{
    private readonly StandardItemSortKeyBuilder _sortKeyBuilder;

    /// <summary>
    /// Initializes a new instance of the <see cref="CadmusEntrySetContextPatcher"/>
    /// class.
    /// </summary>
    public CadmusEntrySetContextPatcher()
    {
        _sortKeyBuilder = new StandardItemSortKeyBuilder();
    }

    /// <summary>
    /// Patches the specified context.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public Task PatchAsync(IEntrySetContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        CadmusEntrySetContext ctx = (CadmusEntrySetContext)context;
        foreach (IItem item in ctx.Items)
        {
            item.FacetId ??= "default";
            item.Title ??= item.Id;
            item.Description ??= "";
            item.SortKey ??= _sortKeyBuilder.BuildKey(item, null);
            item.CreatorId ??= "zeus";
            item.UserId ??= "zeus";
        }
        return Task.CompletedTask;
    }
}
