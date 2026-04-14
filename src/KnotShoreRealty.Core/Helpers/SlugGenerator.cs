using System.Text.RegularExpressions;

namespace KnotShoreRealty.Core.Helpers;

public static class SlugGenerator
{
    private static readonly Regex NonAlphanumeric = new(@"[^a-z0-9]+", RegexOptions.Compiled);

    public static string Generate(string input)
    {
        // Remove apostrophes before the general replacement so "O'Fallon" → "ofallon" not "o-fallon"
        var withoutApostrophes = input.Replace("'", string.Empty);

        var lowered = withoutApostrophes.ToLowerInvariant();

        // Replace any run of non-alphanumeric characters with a single hyphen
        var hyphenated = NonAlphanumeric.Replace(lowered, "-");

        return hyphenated.Trim('-');
    }
}
