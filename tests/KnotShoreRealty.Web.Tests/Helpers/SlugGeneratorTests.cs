using FluentAssertions;
using KnotShoreRealty.Core.Helpers;

namespace KnotShoreRealty.Web.Tests.Helpers;

public class SlugGeneratorTests
{
    [Fact]
    public void Generate_SimpleInput_ReturnsLowercaseHyphenated()
    {
        SlugGenerator.Generate("Clayton Gardens").Should().Be("clayton-gardens");
    }

    [Fact]
    public void Generate_MixedCase_LowercasesAll()
    {
        SlugGenerator.Generate("CENTRAL WEST END").Should().Be("central-west-end");
    }

    [Fact]
    public void Generate_WithApostrophe_RemovesApostrophe()
    {
        SlugGenerator.Generate("O'Fallon").Should().Be("ofallon");
    }

    [Fact]
    public void Generate_MultipleConsecutiveNonAlphanumeric_CollapsesToSingleHyphen()
    {
        SlugGenerator.Generate("22 Log  Cabin  Ln").Should().Be("22-log-cabin-ln");
    }

    [Fact]
    public void Generate_LeadingAndTrailingWhitespace_Trimmed()
    {
        SlugGenerator.Generate("  Soulard  ").Should().Be("soulard");
    }

    [Fact]
    public void Generate_AlreadySlugFormatted_IsIdempotent()
    {
        SlugGenerator.Generate("soulard").Should().Be("soulard");
    }

    [Fact]
    public void Generate_AddressWithHash_ProducesCleanSlug()
    {
        SlugGenerator.Generate("101 S Hanley Rd #402").Should().Be("101-s-hanley-rd-402");
    }
}
