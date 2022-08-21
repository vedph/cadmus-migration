using Fusi.Tools.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cadmus.Export.Filters
{
    /// <summary>
    /// Trivial sentence split renderer filter. This is used to split a text
    /// into sentences, so that each line corresponds to a single sentence.
    /// Sentence splitting is performed on the basis of a list of end-of-sentence
    /// markers.
    /// <para>Tag: <c>it.vedph.renderer-filter.sentence-split</c>.</para>
    /// </summary>
    [Tag("it.vedph.renderer-filter.sentence-split")]
    public sealed class SentenceSplitRendererFilter : IRendererFilter,
        IConfigurable<SentenceSplitRendererFilterOptions>
    {
        private readonly HashSet<char> _markers;
        private string _newLine;
        private bool _trimming;

        /// <summary>
        /// Initializes a new instance of the <see cref="SentenceSplitRendererFilter"/>
        /// class.
        /// </summary>
        public SentenceSplitRendererFilter()
        {
            _markers = new()
            {
                '.', '?', '!',
                '\u037e',   // Greek ';'
                '\u2026'    // ellipsis
            };
            _newLine = Environment.NewLine;
        }

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">options</exception>
        public void Configure(SentenceSplitRendererFilterOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (!string.IsNullOrEmpty(options.EndMarkers))
            {
                _markers.Clear();
                foreach (char c in options.EndMarkers) _markers.Add(c);
            }
            _newLine = options.NewLine;
            _trimming = options.Trimming;
        }

        private void TrimAroundNewLine(StringBuilder text, int index)
        {
            int a, b;

            // right
            a = b = index + _newLine.Length;
            while (b < text.Length && (text[b] == ' ' || text[b] == '\t')) b++;
            if (b > a) text.Remove(a, b - a);

            // left
            a = b = index;
            while (a > 0 && (text[a - 1] == ' ' || text[a - 1] == '\t')) a++;
            if (a < b) text.Remove(a, b - a);
        }

        /// <summary>
        /// Applies this filter to the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="context">The optional rendering context.</param>
        /// <returns>The filtered text.</returns>
        public string Apply(string text, IRendererContext? context = null)
        {
            if (string.IsNullOrEmpty(text)) return text;

            StringBuilder sb = new();
            int i = 0;
            List<int>? nlIndexes = _trimming? new() : null;

            while (i < text.Length)
            {
                // replace existing CR/LF with space
                if (text[i] == '\r' || text[i] == '\n')
                {
                    sb.Append(' ');
                    // skip LF after CR
                    if (text[i] == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    i++;
                    continue;
                }

                // if it's a marker, go past it (and its siblings) and append
                // a newline
                if (_markers.Contains(text[i]))
                {
                    while (i < text.Length && _markers.Contains(text[i]))
                    {
                        if (text[i] != '\r' && text[i] != '\n')
                            sb.Append(text[i]);
                        i++;
                    }
                    nlIndexes?.Add(sb.Length);
                    sb.Append(_newLine);
                }
                else sb.Append(text[i++]);
            }
            if (sb.Length == 0) return "";

            // trim if required
            if (_trimming)
            {
                for (int j = nlIndexes!.Count - 1; j > -1; j--)
                    TrimAroundNewLine(sb, nlIndexes[j]);
            }

            // ensure that the text ends with a newline unless empty
            string result = sb.ToString();
            if (!result.EndsWith(_newLine)) result += _newLine;

            if (_trimming)
            {
                // trim end
                i = result.Length;
                while (i > 0 && (result[i - 1] == ' ' || result[i - 1] == '\t')) i--;
                if (i < result.Length) result = result[..i];
            }

            return result;
        }
    }

    /// <summary>
    /// Options for <see cref="SentenceSplitRendererFilter"/>.
    /// </summary>
    public class SentenceSplitRendererFilterOptions
    {
        /// <summary>
        /// Gets or sets the end-of-sentence marker characters. Each character
        /// in this string is treated as a sentence end marker. Any sequence
        /// of such end marker characters is treated as a single end.
        /// Default characters are <c>.</c>, <c>?</c>, <c>!</c>, Greek question
        /// mark (U+037E), and ellipsis (U+2026).
        /// </summary>
        public string EndMarkers { get; set; }

        /// <summary>
        /// Gets or sets the newline marker to use. The default value is the
        /// newline sequence of the host OS.
        /// </summary>
        public string NewLine { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether trimming spaces/tabs at
        /// both sides of any inserted newline is enabled.
        /// </summary>
        public bool Trimming { get; set; }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="SentenceSplitRendererFilterOptions"/> class.
        /// </summary>
        public SentenceSplitRendererFilterOptions()
        {
            // U+037E = Greek question mark
            // U+2026 = ellipsis
            EndMarkers = ".?!\u037e\u2026";
            NewLine = Environment.NewLine;
        }
    }
}
