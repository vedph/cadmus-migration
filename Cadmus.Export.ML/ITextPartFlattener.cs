using Cadmus.Core;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cadmus.Export.ML
{
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
        /// <returns>Tuple with 1=text and 2=ranges.</returns>
        Tuple<string, MergedRangeSet> GetTextRanges(IPart textPart,
            IList<IPart> layerParts);
    }
}
