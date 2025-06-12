using System.Collections.Generic;

namespace AssetCatalogue.Parser
{
    public class ParserConfig
    {
        public HashSet<string> ExcludedAssets =
            new HashSet<string>
            {
                "0000000000000000f000000000000000",
                "0000000000000000e000000000000000"
                // ^^^ refs to builtin Unity assets
            };
        public HashSet<string> WhitelistedFileExtensions =
            new HashSet<string> { ".yaml", ".unity", ".asset", ".prefab" };
        public string SearchPattern = "*";
        public string ModGuid = "basegame";
        public string DatabasePath = "database.db";
        public bool IncludeAllRefs = false;
        public bool VerboseLogging = false;
        public bool FlushModAssets = false;
        public bool OnlyRegisterBasegameMatches = false;
        public string MonoScriptsFolder = null;
    
        public const int AnchorByteBufferSize = 20;  // a.k.a max character length of a long int
        public const string HelpMessage = @"AssetParasite builds a SQLite database of assets and their dependencies.
Usage: AssetParasite (asset folder|asset path list file) [options]
Options:
-o, --output-file         Path to database file for catalogue database. Defaults to ""database.db"".
-m, --mod-guid            Guid to identify this mod. If not provided, assets are presumed to belong to the base game.
-f, --fresh               Clears the database entries for a given mod. Database path should be provided before this arg.
                              If a mod guid is not provided, all assets will be cleared.
-a, --find-all            Find all occurances of an asset, instead of the first.
-o, --only-matches        Only add asset references where a match exists in the ""basegame"" collection.
-p, --search-pattern      Used to determine which asset filetypes are included. The default is *. 
                              The * and ? characters are supported. Regular expressions are not supported.
-e, --exclude-assets      Comma-seperated asset GUIDs to ignore.
-s, --script-path         Path to a folder containing MonoScripts and associated .meta files.
-u, --scenes              Only process scene files. Intended for ingesting references from base game.
-v, --verbose             Extra logging information for debugging.";
    }
}