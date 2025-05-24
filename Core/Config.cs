namespace Core;

public static class Config
{
    public static HashSet<string> ExcludedAssets =
    [
        "0000000000000000f000000000000000",
        "0000000000000000e000000000000000"
        // ^^^ refs to builtin Unity assets
    ];
    public static HashSet<string> WhitelistedFileExtensions = [
      ".yaml", ".unity", ".asset", ".prefab"  
    ];
    public static string SearchPattern = "*";
    public static string ModGuid = "basegame";
    public static Dictionary<string, string> Guid2ScriptName = [];
    public static string DatabasePath = "database.db";
    public static bool IncludeAllRefs = false;
    public static bool VerboseLogging = false;
    
    public const int AnchorByteBufferSize = 13;     // 13 bytes is the length of a long int in UTF-8
    public const string HelpMessage = """
                                      AssetParasite builds a manifest of referenced assets.
                                      Usage: AssetParasite (asset folder) [options]
                                      Options:
                                      -o, --output-file         Path to database file for catalog. Defaults to "database.db".
                                      -f, --fresh               Clears the entire database before running. Database path should be provided before this arg.
                                      -m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
                                      -a, --find-all            Finds all occurances of an asset, instead of the first.
                                      -p, --search-pattern      Used to determine which asset filetypes are included. The default is *. The * and ? characters are supported. Regular expressions are not supported.
                                      -e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
                                      -n, --script-names        Path to a file which includes a line by line, comma seperated, mapping of asset GUIDs to 
                                                                    MonoBehaviour type names. Example line: fe2f6beebe0c93d45b047e3a99c9e426,PlayerCamera
                                      -v, --version             Print program version and exit.
                                      """;
}