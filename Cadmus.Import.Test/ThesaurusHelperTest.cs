using Cadmus.Core.Config;
using Xunit;

namespace Cadmus.Import.Test;

public sealed class ThesaurusHelperTest
{
    private static Thesaurus GetRgbThesaurus()
    {
        Thesaurus t = new();
        t.AddEntry(new ThesaurusEntry { Id = "r", Value = "red" });
        t.AddEntry(new ThesaurusEntry { Id = "g", Value = "green" });
        t.AddEntry(new ThesaurusEntry { Id = "b", Value = "blue" });
        return t;
    }

    #region Replace
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
            TargetId = "x@en"
        };

        Thesaurus target = GetRgbThesaurus();

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Replace);

        Assert.NotNull(result);
        Assert.Equal(0, result.Entries.Count);
        Assert.Equal("x@en", result.TargetId);
    }

    [Fact]
    public void CopyThesaurus_Replace_NoAliasVsAlias_NoAlias()
    {
        Thesaurus source = GetRgbThesaurus();

        Thesaurus target = new()
        {
            TargetId = "x@en"
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

    [Fact]
    public void CopyThesaurus_Replace_AliasVsAlias_Replaced()
    {
        Thesaurus source = new()
        {
            TargetId = "source@en"
        };
        Thesaurus target = new()
        {
            TargetId = "target@en"
        };

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Replace);

        Assert.NotNull(result);
        Assert.Empty(result.Entries);
        Assert.Equal("source@en", result.TargetId);
    }
    #endregion

    #region Patch
    // all tests as above for replace, but using patch mode
    [Fact]
    public void CopyThesaurus_Patch_NoAliasVsNoAlias_Patched()
    {
        Thesaurus source = new();
        source.AddEntry(new ThesaurusEntry { Id = "r", Value = "rosso" });
        source.AddEntry(new ThesaurusEntry { Id = "g", Value = "verde" });

        Thesaurus target = GetRgbThesaurus();

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
                       ImportUpdateMode.Patch);

        Assert.NotNull(result);
        Assert.Equal(3, result.Entries.Count);
        Assert.Equal("r", result.Entries[0].Id);
        Assert.Equal("rosso", result.Entries[0].Value);

        Assert.Equal("g", result.Entries[1].Id);
        Assert.Equal("verde", result.Entries[1].Value);

        Assert.Equal("b", result.Entries[2].Id);
        Assert.Equal("blue", result.Entries[2].Value);
    }

    [Fact]
    public void CopyThesaurus_Patch_AliasVsNoAlias_Alias()
    {
        Thesaurus source = new()
        {
            TargetId = "x@en"
        };

        Thesaurus target = GetRgbThesaurus();

        Thesaurus result = ThesaurusHelper.CopyThesaurus(source, target,
            ImportUpdateMode.Patch);

        Assert.NotNull(result);
        Assert.Equal(0, result.Entries.Count);
        Assert.Equal("x@en", result.TargetId);
    }
    #endregion
}