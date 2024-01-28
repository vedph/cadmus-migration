using Cadmus.Core;
using Cadmus.General.Parts;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Cadmus.Export.Test;

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
        TokenTextPartFlattener exporter = new();
        TokenTextPart textPart = TokenTextPartFlattenerTest.GetSampleTextPart();
        IList<IPart> layerParts = TokenTextPartFlattenerTest.GetSampleLayerParts();
        var tr = exporter.GetTextRanges(textPart, layerParts);
        TextBlockBuilder builder = new();

        List<TextBlockRow> rows = builder.Build(tr.Item1, tr.Item2)
            .ToList();

        Assert.Equal(2, rows.Count);

        // row 0
        TextBlockRow row = rows[0];
        Assert.Equal(5, row.Blocks.Count);
        // qu: -
        Assert.Equal("qu", row.Blocks[0].Text);
        Assert.Empty(row.Blocks[0].LayerIds);
        // e: AB
        Assert.Equal("e", row.Blocks[1].Text);
        Assert.Equal(2, row.Blocks[1].LayerIds.Count);
        // _: B
        Assert.Equal(" ", row.Blocks[2].Text);
        Assert.Single(row.Blocks[2].LayerIds);
        // v: BC
        Assert.Equal("v", row.Blocks[3].Text);
        Assert.Equal(2, row.Blocks[3].LayerIds.Count);
        // ixit: C
        Assert.Equal("ixit", row.Blocks[4].Text);
        Assert.Single(row.Blocks[4].LayerIds);

        // row 1
        row = rows[1];
        Assert.Equal(3, row.Blocks.Count);
        // annos: C
        Assert.Equal("annos", row.Blocks[0].Text);
        Assert.Single(row.Blocks[0].LayerIds);
        // _: -
        Assert.Equal(" ", row.Blocks[1].Text);
        Assert.Empty(row.Blocks[1].LayerIds);
        // XX: D
        Assert.Equal("XX", row.Blocks[2].Text);
        Assert.Single(row.Blocks[2].LayerIds);
    }
}
