using Cadmus.Core;
using Fusi.Tools.Config;

namespace Cadmus.Export.ML.Test
{
    internal sealed class RamTeiStandoffItemComposer : TeiStandoffItemComposer,
        IItemComposer, IConfigurable<TeiStandoffItemComposerOptions>
    {
        private TeiStandoffItemComposerOptions? _options;

        public void Configure(TeiStandoffItemComposerOptions options)
        {
            _options = options
                ?? throw new System.ArgumentNullException(nameof(options));
        }

        protected override object? DoCompose(IItem item, object? context = null)
        {
            return RenderFlows(item);
        }
    }
}
