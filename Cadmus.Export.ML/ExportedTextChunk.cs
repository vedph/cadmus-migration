using Fusi.Tools.Text;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cadmus.Export.ML
{
    public class ExportedTextChunk
    {
        private List<MergedRange>? _ranges;

        public ExportedTextChunkType Type { get; set; }

        public string? Text { get; set; }

        public bool HasRanges => _ranges?.Count > 0;

        public IList<MergedRange> Ranges
        {
            get
            {
                return _ranges ??= new List<MergedRange>();
            }
        }

        public override string ToString()
        {
            return (Text ?? Enum.GetName(Type)) +
                (HasRanges? string.Join(", ", Ranges.Select(r => r.Id)) : "");
        }
    }

    public enum ExportedTextChunkType
    {
        Text = 0,
        Open,
        Close
    }
}
