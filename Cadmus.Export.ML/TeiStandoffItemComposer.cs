using Cadmus.Core;
using Fusi.Tools.Config;
using System;

namespace Cadmus.Export.ML
{
    [Tag("it.vedph.item-composer.tei-standoff")]
    public sealed class TeiStandoffItemComposer : ItemComposer, IItemComposer
    {
        public object? Compose(IItem item, object? context = null)
        {
            throw new NotImplementedException();
        }
    }
}
