namespace Core;

public static class Config
{
    public static HashSet<string> ExcludedAssets =
    [
        "0000000000000000f000000000000000",
        "0000000000000000e000000000000000"
        // ^^^ refs to builtin Unity assets
    ];
    public static HashSet<string> ExcludedScenes = [];
    public static Dictionary<string, string> Guid2ScriptName = [];
    public static bool PrettyPrint = false;
    // 13 bytes is the length of a long int in UTF-8
    public const int AnchorByteBufferSize = 13; 
    // these options exist purely to keep manifest size down -- tbd if we even need >1
    public const int MaxAssetPathsIncludedPerScene = 2;
    public const int MaxPathsPerAsset = 3;
    public const string HelpMessage = """
                                      AssetParasite builds a manifest of assets included in your scene files for easy locating during runtime.
                                      Usage: AssetParasite (scene folder) [options]
                                      Options:
                                      -o, --output          Path to save generated manifest.
                                      -a, --assets          Comma-seperated asset GUIDs to catalog. If not provided, the tool will catalog all assets.
                                      -n, --script-names    Path to a file which includes a line by line, comma seperated, mapping of asset GUIDs to 
                                                                MonoBehaviour type names. Example line: fe2f6beebe0c93d45b047e3a99c9e426,PlayerCamera
                                      -s, --exclude-scenes  Comma-seperated scene filenames to ignore.
                                      -e, --exclude-assets  Comma-seperated asset GUIDs to ignore.
                                      -v, --version         Print program version and exit.
                                      -p                    Use PrettyPrint for manifest json.
                                      """;
}