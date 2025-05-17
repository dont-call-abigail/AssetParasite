namespace Core;

public static class Config
{
    public static HashSet<string> ExcludedAssets =
    [
        "0000000000000000f000000000000000",
        "0000000000000000e000000000000000"
    ];

    public const int AnchorByteBufferSize = 13; // 13 bytes is the length of long in UTF-8
    
    // these options exist purely to keep filesize down -- tbd if we even need >1
    public const int MaxAssetPathsIncludedPerScene = 2;
    public const int MaxPathsPerAsset = 3;
}