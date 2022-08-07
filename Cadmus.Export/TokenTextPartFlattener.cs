using Cadmus.Core;
using Cadmus.Core.Layers;
using Cadmus.General.Parts;
using Fusi.Tools.Config;
using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cadmus.Export
{
    /// <summary>
    /// Token-based text exporter. This takes a <see cref="TokenTextPart"/>
    /// with any number of layer parts linked to it, and produces a string
    /// representing this text with a list of spans corresponding to each
    /// fragment in each of the received layers.
    /// <para>Tag: <c>it.vedph.text-flattener.token</c>.</para>
    /// </summary>
    [Tag("it.vedph.text-flattener.token")]
    public sealed class TokenTextPartFlattener : ITextPartFlattener,
        IConfigurable<TokenTextPartFlattenerOptions>
    {
        private string _lineSeparator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TokenTextPartFlattener"/>
        /// class.
        /// </summary>
        public TokenTextPartFlattener()
        {
            _lineSeparator = "\n";
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(TokenTextPartFlattenerOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            _lineSeparator = options.LineSeparator;
        }

        private static int LocateTokenEnd(string text, int index)
        {
            int i = text.IndexOf(' ', index);
            return i == -1 ? text.Length : i;
        }

        private int GetIndexFromPoint(TokenTextPoint p, TokenTextPart part,
            bool end = false)
        {
            // base index for Y
            int index = ((p.Y - 1) * _lineSeparator.Length) +
                part.Lines.Select(l => l.Text.Length).Take(p.Y - 1).Sum();

            // locate X
            if (p.X > 1 || end)
            {
                string line = part.Lines[p.Y - 1].Text;
                int x = p.X - 1, i = 0;
                while (x > 0)
                {
                    i = LocateTokenEnd(line, i);
                    if (i < line.Length) i++;
                    x--;
                }
                if (end && p.At == 0)
                {
                    i = LocateTokenEnd(line, i) - 1;
                }
                index += i;
            }

            // locate A
            if (p.At > 0)
            {
                index += p.At - 1;
                if (end) index += p.Run - 1;
            }

            return index;
        }

        private MergedRange GetRangeFromLoc(string loc, string text,
            TokenTextPart part)
        {
            TokenTextLocation l = TokenTextLocation.Parse(loc);
            MergedRange range = new()
            {
                Start = GetIndexFromPoint(l.A, part)
            };

            if (l.IsRange)
            {
                // range
                range.End = GetIndexFromPoint(l.B, part, true);
            }
            else
            {
                // single point (partial/whole token)
                range.End = l.A.Run > 0
                    ? range.Start + l.A.Run - 1
                    : LocateTokenEnd(text, range.Start) - 1;
            }

            return range;
        }

        private static IList<string> GetFragmentLocations(IPart part)
        {
            PropertyInfo? pi = part.GetType().GetProperty("Fragments");
            if (pi == null) return Array.Empty<string>();

            System.Collections.IEnumerable? frags = pi.GetValue(part)!
                as System.Collections.IEnumerable;
            if (frags == null) return Array.Empty<string>();

            List<string> locs = new();
            foreach (object fr in frags)
            {
                locs.Add((fr as ITextLayerFragment)!.Location);
            }
            return locs;
        }

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
        /// <exception cref="ArgumentNullException">textPart or layerParts
        /// </exception>
        public Tuple<string, MergedRangeSet> GetTextRanges(IPart textPart,
            IList<IPart> layerParts)
        {
            TokenTextPart? txt = textPart as TokenTextPart;
            if (txt == null) throw new ArgumentNullException(nameof(textPart));
            if (layerParts is null)
                throw new ArgumentNullException(nameof(layerParts));

            string text = string.Join(_lineSeparator,
                txt.Lines.Select(l => l.Text));

            // convert all the fragment locations into ranges
            IList<MergedRange> ranges = new List<MergedRange>();
            int layerIndex = 0;
            foreach (IPart part in layerParts)
            {
                int fi = 0;
                foreach (string loc in GetFragmentLocations(part))
                {
                    MergedRange r = GetRangeFromLoc(loc, text, txt);
                    r.Id = $"L{layerIndex}F{fi++}";
                    r.GroupId = $"L{layerIndex}";
                    ranges.Add(r);
                }
                layerIndex++;
            }

            return Tuple.Create(text, new MergedRangeSet(ranges));
        }
    }

    /// <summary>
    /// Options for <see cref="TokenTextPartFlattener"/>.
    /// </summary>
    public class TokenTextPartFlattenerOptions
    {
        /// <summary>
        /// Gets or sets the line separator to be used in the string built
        /// from the text being exported. The default value is LF.
        /// </summary>
        public string LineSeparator { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="TokenTextPartFlattenerOptions"/> class.
        /// </summary>
        public TokenTextPartFlattenerOptions()
        {
            LineSeparator = "\n";
        }
    }
}