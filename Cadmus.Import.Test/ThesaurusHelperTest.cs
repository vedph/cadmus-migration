using Cadmus.Core.Config;
using Xunit;

namespace Cadmus.Import.Test;

public class ThesaurusHelperTest
{
    private static Thesaurus GetRgbThesaurus()
    {
        Thesaurus t = new();
        t.AddEntry(new ThesaurusEntry { Id = "r", Value = "red" });
        t.AddEntry(new ThesaurusEntry { Id = "g", Value = "green" });
        t.AddEntry(new ThesaurusEntry { Id = "b", Value = "blue" });
        return t;
    }

    [Fact]
    public void CopyThesaurus_Replace_NoAliasVsNoAlias_Replaced()
    {
        Thesaurus source = new();
        source.AddEntry(new ThesaurusEntry { Id = "r", Value = "rosso" });
        source.AddEntry(new ThesaurusEntry { Id = "g", Value = "verde" });

        Thesaurus target = GetRgbThesaurus();

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Replace);

        Assert.NotNull(result);
        Assert.Equal(2, result.Entries.Count);
        Assert.Equal("r", result.Entries[0].Id);
        Assert.Equal("rosso", result.Entries[0].Value);

        Assert.Equal("g", result.Entries[1].Id);
        Assert.Equal("verde", result.Entries[1].Value);
    }

    [Fact]
    public void CopyThesaurus_Replace_AliasVsNoAlias_Alias()
    {
        Thesaurus source = new()
        {
            TargetId = "target@en"
        };

        Thesaurus target = GetRgbThesaurus();

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Replace);

        Assert.NotNull(result);
        Assert.Equal(0, result.Entries.Count);
        Assert.Equal("target@en", result.TargetId);
    }

    [Fact]
    public void CopyThesaurus_Replace_NoAliasVsAlias_NoAlias()
    {
        Thesaurus source = GetRgbThesaurus();

        Thesaurus target = new()
        {
            TargetId = "target@en"
        };

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Replace);

        Assert.NotNull(result);
        Assert.Equal(3, result.Entries.Count);
        Assert.Equal("r", result.Entries[0].Id);
        Assert.Equal("red", result.Entries[0].Value);
        Assert.Equal("g", result.Entries[1].Id);
        Assert.Equal("green", result.Entries[1].Value);
        Assert.Equal("b", result.Entries[2].Id);
        Assert.Equal("blue", result.Entries[2].Value);
    }

}