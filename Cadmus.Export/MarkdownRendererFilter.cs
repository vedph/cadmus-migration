﻿using Fusi.Tools.Config;
using Markdig;
using System;

namespace Cadmus.Export
{
    /// <summary>
    /// Markdown renderer filter. This renders any Markdown region or the whole
    /// text, as specified in its configuration.
    /// <para>Tag: <c>it.vedph.renderer-filter.markdown</c>.</para>
    /// </summary>
    [Tag("it.vedph.renderer-filter.markdown")]
    public sealed class MarkdownRendererFilter : IRendererFilter,
        IConfigurable<MarkdownRendererFilterOptions>
    {
        private MarkdownRendererFilterOptions? _options;

        /// <summary>
        /// Configures the object with the specified options.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <exception cref="ArgumentNullException">null options</exception>
        public void Configure(MarkdownRendererFilterOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Applies this filter to the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="context">The optional renderer context.</param>
        /// <returns>Filtered text.</returns>
        public string Apply(string text, IRendererContext? context = null)
        {
            if (string.IsNullOrEmpty(text) || _options == null) return text;

            switch (_options.Format?.ToLowerInvariant())
            {
                case "txt":
                    if (_options.MarkdownOpen == null ||
                        _options.MarkdownClose == null)
                    {
                        return Markdown.ToPlainText(text);
                    }
                    else
                    {
                        return MarkdownHelper.ConvertRegions(text,
                            _options.MarkdownOpen,
                            _options.MarkdownClose,
                            true);
                    }
                case "html":
                    if (_options.MarkdownOpen == null ||
                        _options.MarkdownClose == null)
                    {
                        return Markdown.ToHtml(text);
                    }
                    else
                    {
                        return MarkdownHelper.ConvertRegions(text,
                            _options.MarkdownOpen,
                            _options.MarkdownClose,
                            false);
                    }
                default:
                    return text;
            }
        }
    }

    /// <summary>
    /// Options for <see cref="MarkdownRendererFilter"/>.
    /// </summary>
    public class MarkdownRendererFilterOptions
    {
        /// <summary>
        /// Gets or sets the markdown region opening tag. When not set, it is
        /// assumed that the whole text is Markdown.
        /// </summary>
        public string? MarkdownOpen { get; set; }

        /// <summary>
        /// Gets or sets the markdown region closing tag. When not set, it is
        /// assumed that the whole text is Markdown.
        /// </summary>
        public string? MarkdownClose { get; set; }

        /// <summary>
        /// Gets or sets the Markdown regions target format: if not specified,
        /// nothing is done; if <c>txt</c>, any Markdown region is converted
        /// into plain text; if <c>html</c>, any Markdown region is converted
        /// into HTML.
        /// </summary>
        public string? Format { get; set; }
    }
}
