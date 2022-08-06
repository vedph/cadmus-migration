using Cadmus.Core;
using Cadmus.General.Parts;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cadmus.Export.ML.Test
{
    public sealed class TextBlockBuilderTest
    {
        [Fact]
        public void Build_Ok()
        {
            // 0123456789-1234567
            // que vixit|annos XX
            // ..A...............
            // ..BBB.............
            // ....CCCCCCCCCCC...
            // ................DD
            TokenTextExporter exporter = new();
            TokenTextPart textPart = TokenTextExporterTest.GetSampleTextPart();
            IList<IPart> layerParts = TokenTextExporterTest.GetSampleLayerParts();
            var tr = exporter.GetTextRanges(textPart, layerParts);
            TextBlockBuilder builder = new();

            List<IList<TextBlock>> rows = builder.Build(tr.Item1, tr.Item2)
                .ToList();

            Assert.Equal(2, rows.Count);

            // row 0
            IList<TextBlock> row = rows[0];
            Assert.Equal(5, row.Count);
            // qu: -
            Assert.Equal("qu", row[0].Text);
            Assert.Equal(0, row[0].LayerIds.Count);
            // e: AB
            Assert.Equal("e", row[1].Text);
            Assert.Equal(2, row[1].LayerIds.Count);
            // _: B
            Assert.Equal(" ", row[2].Text);
            Assert.Equal(1, row[2].LayerIds.Count);
            // v: BC
            Assert.Equal("v", row[3].Text);
            Assert.Equal(2, row[3].LayerIds.Count);
            // ixit: C
            Assert.Equal("ixit", row[4].Text);
            Assert.Equal(1, row[4].LayerIds.Count);

            // row 1
            row = rows[1];
            Assert.Equal(3, row.Count);
            // annos: C
            Assert.Equal("annos", row[0].Text);
            Assert.Equal(1, row[0].LayerIds.Count);
            // _: -
            Assert.Equal(" ", row[1].Text);
            Assert.Equal(0, row[1].LayerIds.Count);
            // XX: D
            Assert.Equal("XX", row[2].Text);
            Assert.Equal(1, row[2].LayerIds.Count);
        }
    }
}
