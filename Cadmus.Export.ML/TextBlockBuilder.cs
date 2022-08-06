using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadmus.Export.ML
{
    /// <summary>
    /// Builder of <see cref="TextBlock"/>'s. This gets a text with its merged
    /// ranges set, and builds a set of lists of text blocks, one for each
    /// original line in the text. Every block encodes the span of text linked
    /// to the same subset of layer IDs. Further processing can then add
    /// decoration and tip to blocks.
    /// </summary>
    public class TextBlockBuilder
    {
        /// <summary>
        /// Gets or sets the delimiter used to separate rows in the source text.
        /// The default value is LF.
        /// </summary>
        public string Delimiter { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextBlockBuilder"/> class.
        /// </summary>
        public TextBlockBuilder()
        {
            Delimiter = "\n";
        }

        private bool HasDelimiterAt(string text, int index)
        {
            if (Delimiter.Length + index > text.Length) return false;
            for (int i = 0; i < Delimiter.Length; i++)
            {
                if (text[i + index] != Delimiter[i]) return false;
            }
            return true;
        }

        /// <summary>
        /// Builds rows of text blocks from the specified text and ranges set.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="set">The ranges set.</param>
        /// <returns>Enumerable of list of text blocks, each representing a row
        /// (line) in the original text.</returns>
        /// <exception cref="ArgumentNullException">text or set</exception>
        public IEnumerable<IList<TextBlock>> Build(string text,
            MergedRangeSet set)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (set is null) throw new ArgumentNullException(nameof(set));

            int i = 1, n = 0, start = 0;
            List<TextBlock> row = new();

            while (i < text.Length)
            {
                // whenever we find a line delimiter, end the row of blocks
                // and emit it, then continue with a new row
                if (HasDelimiterAt(text, i))
                {
                    if (i > start)
                    {
                        IList<MergedRange> ranges = set.GetRangesAt(start);
                        row.Add(new TextBlock(
                            $"{++n}", text[start..i], ranges.Select(r => r.Id!)));
                    }
                    if (row.Count > 0) yield return row;

                    row = new List<TextBlock>();
                    i += Delimiter.Length;
                    start = i;
                    continue;
                }

                if (!set.AreEqualAt(i, i - 1))
                {
                    if (i > start)
                    {
                        IList<MergedRange> ranges = set.GetRangesAt(start);
                        row.Add(new TextBlock(
                            $"{++n}", text[start..i], ranges.Select(r => r.Id!)));
                    }
                    start = i++;
                }
                else i++;
            }

            if (i > start)
            {
                IList<MergedRange> ranges = set.GetRangesAt(start);
                row.Add(new TextBlock(
                    $"{++n}", text[start..i], ranges.Select(r => r.Id!)));
            }
            if (row.Count > 0) yield return row;
        }
    }
}
