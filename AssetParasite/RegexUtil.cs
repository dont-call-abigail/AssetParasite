using System.Text.RegularExpressions;

namespace AssetParasite;

public static partial class RegexUtil
{
    [GeneratedRegex(@"\{fileID: [0-9]+")]
    private static partial Regex FileIdRegex();
    [GeneratedRegex( @"guid: [A-Za-z0-9]+,")]
    private static partial Regex GuidRegex();
    
    public static long ParseFileId(string scalar)
    {
        var match = FileIdRegex().Match(scalar);
        return match.Success && long.TryParse(match.Value, out long res) ? res : long.MaxValue;
    }
    
    public static string ParseGuid(string scalar)
    {
        var match = GuidRegex().Match(scalar);
        return match.Success ? match.Value : "";
    }
}