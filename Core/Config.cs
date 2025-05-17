namespace Core;

public static class Config
{
    // refs to builtin Unity assets
    public static HashSet<string> ExcludedAssets =
    [
        "0000000000000000f000000000000000",
        "0000000000000000e000000000000000"
    ];

    // 13 bytes is the length of a long int in UTF-8
    public const int AnchorByteBufferSize = 13; 
    
    // these options exist purely to keep filesize down -- tbd if we even need >1
    public const int MaxAssetPathsIncludedPerScene = 2;
    public const int MaxPathsPerAsset = 3;
}