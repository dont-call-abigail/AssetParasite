namespace Core;

public static class Config
{
    public static readonly HashSet<string> ExcludedAssets =
    [
        "0000000000000000f000000000000000",
        "0000000000000000e000000000000000"
        // ^^^ refs to builtin Unity assets
    ];
    public static readonly HashSet<string> WhitelistedFileExtensions = [
      ".yaml", ".unity", ".asset", ".prefab"  
    ];
    public static string SearchPattern = "*";
    public static string ModGuid = "basegame";
    public static string DatabasePath = "database.db";
    public static bool IncludeAllRefs = false;
    public static bool VerboseLogging = false;
    public static bool FlushModAssets = false;
    public static string? MonoScriptsFolder = null;
    
    public const int AnchorByteBufferSize = 13;     // 13 bytes is the length of a long int in UTF-8
    public const string HelpMessage = """
                                      AssetParasite builds a manifest of referenced assets.
                                      Usage: AssetParasite (asset folder|asset path list file) [options]
                                      Options:
                                      -o, --output-file         Path to database file for catalog. Defaults to "database.db".
                                      -m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
                                      -f, --fresh               Clears the database entries for a given mod. Database path should be provided before this arg.
                                                                    If a mod guid is not provided, all assets will be cleared.
                                      -a, --find-all            Find all occurances of an asset, instead of the first.
                                      -p, --search-pattern      Used to determine which asset filetypes are included. The default is *. The * and ? characters are supported. Regular expressions are not supported.
                                      -e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
                                      -s, --script-path         Path to a folder containing MonoScripts and associated .meta files.
                                      -u, --scenes              Only process scene files. Intended for ingesting references from base game.
                                      -v, --version             Print program version and exit.
                                      """;
}