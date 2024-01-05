using Cadmus.Core;
using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace Cadmus.Import.Proteus;

/// <summary>
/// An imported item used in <see cref="CadmusEntrySetContext"/>. This has the
/// same metadata as a Cadmus <see cref="Item"/>, plus a list of parts as
/// JSON nodes.
/// </summary>
/// <seealso cref="IHasVersion" />
/// <seealso cref="IHasFlags" />
public class ImportedItem : IHasVersion, IHasFlags
{
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Item title.
    /// </summary>
    public string Title { get; set; } = "";

    /// <summary>
    /// Item short description.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Item's facet.
    /// </summary>
    /// <value>The facet defines which parts can be stored in the item,
    /// and their order and other presentational attributes. It is a unique
    /// string defined in the corpus configuration.</value>
    public string FacetId { get; set; } = "default";

    /// <summary>
    /// Gets or sets the group identifier. This is an arbitrary string
    /// which can be used to group items into a set. For instance, you
    /// might have a set of items belonging to the same literary work,
    /// a set of lemmata belonging to the same dictionary letter, etc.
    /// </summary>
    public string? GroupId { get; set; }

    /// <summary>
    /// The sort key for the item. This is a value used to sort items in
    /// a list.
    /// </summary>
    public string SortKey { get; set; } = "";

    /// <summary>
    /// Creation date and time (UTC).
    /// </summary>
    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who created the resource.
    /// </summary>
    public string CreatorId { get; set; } = "zeus";

    /// <summary>
    /// Last saved date and time (UTC).
    /// </summary>
    public DateTime TimeModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID of the user who last saved the resource.
    /// </summary>
    public string UserId { get; set; } = "zeus";

    /// <summary>
    /// Gets or sets generic flags for the item.
    /// </summary>
    public int Flags { get; set; }

    /// <summary>
    /// Gets the parts modeled as editable JSON nodes. Each part has id, itemId,
    /// typeId, roleId, thesaurusScope, timeCreated, creatorId, timeModified,
    /// userId, and any other properties.
    /// </summary>
    public List<JsonNode> Parts { get; } = [];

    /// <summary>
    /// Converts this object into a Cadmus <see cref="Item"/> without parts.
    /// </summary>
    /// <returns>Item.</returns>
    public Item ToItem()
    {
        return new Item
        {
            Id = Id,
            Title = Title,
            Description = Description,
            FacetId = FacetId,
            GroupId = GroupId,
            SortKey = SortKey,
            TimeCreated = TimeCreated,
            CreatorId = CreatorId,
            TimeModified = TimeModified,
            UserId = UserId,
            Flags = Flags
        };
    }

    /// <summary>
    /// Returns a <see cref="string" /> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="string" /> that represents this instance.
    /// </returns>
    public override string ToString()
    {
        return $"{Id}: {Title}" + (FacetId != null ? $" [{FacetId}]" : "");
    }
}
