﻿using Cadmus.Core;
using Fusi.Tools.Text;
using MongoDB.Driver.Linq;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Cadmus.Export
{
    /// <summary>
    /// Base class for <see cref="IItemComposer"/> implementors.
    /// </summary>
    public abstract class ItemComposer
    {
        /// <summary>The item ID metadata key (<c>item-id</c>).</summary>
        public const string M_ITEM_ID = "item-id";
        /// <summary>The item title metadata key (<c>item-title</c>).</summary>
        public const string M_ITEM_TITLE = "item-title";
        /// <summary>The item facet metadata key (<c>item-facet</c>).</summary>
        public const string M_ITEM_FACET = "item-facet";
        /// <summary>The item group metadata key (<c>item-group</c>).</summary>
        public const string M_ITEM_GROUP = "item-group";
        /// <summary>The item flags metadata key (<c>item-flags</c>).</summary>
        public const string M_ITEM_FLAGS = "item-flags";
        /// <summary>The item number metadata key (<c>item-nr</c>).</summary>
        public const string M_ITEM_NR = "item-nr";

        private bool _externalOutput;
        private string? _lastGroupId;

        /// <summary>
        /// Gets or sets the optional text part flattener.
        /// </summary>
        public ITextPartFlattener? TextPartFlattener { get; set; }

        /// <summary>
        /// Gets or sets the optional text block renderer.
        /// </summary>
        public ITextBlockRenderer? TextBlockRenderer { get; set; }

        /// <summary>
        /// Gets the JSON renderers.
        /// </summary>
        public IDictionary<string, IJsonRenderer> JsonRenderers { get; init; }

        /// <summary>
        /// Gets the ordinal item number. This is set to 0 when opening the
        /// composer, and increased whenever a new item is processed.
        /// </summary>
        public int ItemNumber { get; private set; }

        /// <summary>
        /// Gets the context using during processing.
        /// </summary>
        public RendererContext Context { get; }

        /// <summary>
        /// Gets the output handled by this composer, or null if not opened.
        /// </summary>
        public ItemComposition? Output { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemComposer"/>
        /// class.
        /// </summary>
        protected ItemComposer()
        {
            JsonRenderers = new Dictionary<string, IJsonRenderer>();
            Context = new RendererContext();
        }

        /// <summary>
        /// Ensures the writer with the specified key exists in <see cref="Output"/>,
        /// creating it if required.
        /// </summary>
        /// <param name="key">The writer's key.</param>
        protected abstract void EnsureWriter(string key);

        /// <summary>
        /// Writes <paramref name="content"/> to the output writer with the
        /// specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The writer's key. If the writer does not exist,
        /// it will be created (via <see cref="EnsureWriter(string)"/>).</param>
        /// <param name="content">The content.</param>
        protected void WriteOutput(string key, string content)
        {
            if (string.IsNullOrEmpty(content)) return;

            EnsureWriter(key);
            Output?.Writers[key].Write(content);
        }

        /// <summary>
        /// Open the composer. This implementation just sets <see cref="Output"/>
        /// from <paramref name="output"/>, or by creating a new instance if
        /// <paramref name="output"/> is null. In the latter case, this output
        /// will be disposed when calling <see cref="Close"/>. Otherwise, the
        /// lifetime handling of the output is entrusted to client code.
        /// </summary>
        /// <param name="output">The output object to use, or null to create
        /// a new one.</param>
        /// <remarks>This base implementation resets the internal state and
        /// <see cref="Context"/>, and sets <see cref="Output"/>.</remarks>
        public virtual void Open(ItemComposition? output = null)
        {
            // reset state
            _externalOutput = output != null;
            _lastGroupId = null;
            ItemNumber = 0;

            // reset context
            Context.Data.Clear();
            Context.TargetIds.Clear();

            // set output
            Output = output ?? new ItemComposition();
        }

        /// <summary>
        /// Fills the specified template using <see cref="Context"/>.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <returns>The filled template.</returns>
        protected string FillTemplate(string? template)
            => template != null
                ? TextTemplate.FillTemplate(template, Context.Data)
                : "";

        /// <summary>
        /// Invoked when the item's group changed since the last call to
        /// <see cref="Compose"/>. This can be used when processing grouped
        /// items in order.
        /// </summary>
        /// <param name="item">The new item.</param>
        /// <param name="prevGroupId">The previous group identifier.</param>
        protected virtual void OnGroupChanged(IItem item, string? prevGroupId)
        {
        }

        /// <summary>
        /// Does the composition for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        protected abstract void DoCompose(IItem item);

        /// <summary>
        /// Clears the context data.
        /// </summary>
        /// <param name="excludedPrefix">The excluded prefix. When set, all
        /// the context data whose key starts with this prefix are not removed.
        /// </param>
        protected void ClearContextData(string? excludedPrefix = null)
        {
            if (string.IsNullOrEmpty(excludedPrefix)) Context.Data.Clear();
            else
            {
                foreach (string key in Context.Data.Keys
                    .Where(k => !k.StartsWith(excludedPrefix,
                                   StringComparison.Ordinal))
                    .ToList())
                {
                    Context.Data.Remove(key);
                }
            }
        }

        /// <summary>
        /// Composes the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <exception cref="ArgumentNullException">item</exception>
        public void Compose(IItem item)
        {
            if (item is null) throw new ArgumentNullException(nameof(item));

            Context.Data[M_ITEM_NR] = ++ItemNumber;
            Context.Data[M_ITEM_ID] = item.Id;
            Context.Data[M_ITEM_TITLE] = item.Title;
            Context.Data[M_ITEM_FACET] = item.FacetId;
            Context.Data[M_ITEM_GROUP] = item.GroupId;
            Context.Data[M_ITEM_FLAGS] = item.Flags;

            if (item.GroupId != _lastGroupId)
                OnGroupChanged(item, _lastGroupId);

            DoCompose(item);

            ClearContextData();
        }

        /// <summary>
        /// Close the composer.
        /// </summary>
        /// <remarks>This base implementation disposes <see cref="Output"/>
        /// when it was not got from an external source in <see cref="Open"/>.
        /// </remarks>
        public virtual void Close()
        {
            if (!_externalOutput && Output != null)
            {
                Output.Dispose();
                Output = null;
            }
        }
    }
}
