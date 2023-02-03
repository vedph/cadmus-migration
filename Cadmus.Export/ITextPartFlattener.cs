using Cadmus.Core;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;

namespace Cadmus.Export;

/// <summary>
/// Flattener for parts containing a text representing the base text of
/// a set of layer parts.
/// </summary>
public interface ITextPartFlattener
{
    /// <summary>
    /// Starting from a text part and a list of layer parts, gets a string
    /// representing the text with a list of layer ranges representing
    /// the extent of each layer's fragment on it.
    /// </summary>
    /// <param name="textPart">The text part used as the base text. This is
    /// the part identified by role ID <see cref="PartBase.BASE_TEXT_ROLE_ID"/>
    /// in an item.</param>
    /// <param name="layerParts">The layer parts you want to export.</param>
    /// <param name="layerIds">The optional IDs to assign to each layer
    /// part's range. When specified, it must have the same size of
    /// <paramref name="layerParts"/> so that the first entry in it
    /// corresponds to the first entry in layer IDs, the second to the second,
    /// and so forth.</param>
    /// <returns>Tuple with 1=text and 2=ranges.</returns>
    Tuple<string, MergedRangeSet> GetTextRanges(IPart textPart,
        IList<IPart> layerParts, IList<string?>? layerIds = null);
}
