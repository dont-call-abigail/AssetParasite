using System.Reflection;
using System.Text.Json;

namespace Core
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine(Config.HelpMessage);
                return;
            } 
            
            if (args.Length == 1)
            {
                string lonelyArg = args[0];
                if (lonelyArg is "--help" or "-h")
                {
                    Console.WriteLine(Config.HelpMessage);
                    return;
                }
                if (lonelyArg is "--version")
                {
                    Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version);
                    return;
                }
            }

            string assetFolderOrRegistryFile = args[0];
            
            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg is "--output-file" or "-o")
                {
                    Config.DatabasePath = args[++i];
                } else if (arg is "--search-pattern" or "-p")
                {
                    Config.SearchPattern = args[++i];
                } else if (arg is "--exclude-assets" or "-e")
                {
                    var excludeAssets = args[++i].Split(',');
                    foreach (var asset in excludeAssets)
                    {
                        Config.ExcludedAssets.Add(asset);
                    }
                } else if (arg is "--script-path" or "-s")
                {
                    var maybePath = args[++i];
                    if (Directory.Exists(maybePath))
                    {
                        Config.MonoScriptsFolder = maybePath;
                    }
                    else
                    {
                        Console.Write("ERROR! Provided script folder cannot be found. Given: " + maybePath);
                    }

                } else if (arg is "-a" or "--find-all")
                {
                    Config.IncludeAllRefs = true;
                } else if (arg is "--fresh" or "-f")
                {
                    Config.FlushModAssets = true;
                } else if (arg is "--mod-guid" or "-m")
                {
                    Config.ModGuid = args[++i];
                } else if (arg is "--verbose" or "-v")
                {
                    Config.VerboseLogging = true;
                } else if (arg is "-scenes" or "-u")
                {
                    Config.SearchPattern = "*.unity";
                }
            }
            
            if (!string.IsNullOrEmpty(Config.MonoScriptsFolder))
            {
                Console.WriteLine($"MonoScripts folder : {Config.MonoScriptsFolder}");
                ManifestGenerator.PopulateScriptIDs(Config.MonoScriptsFolder);
            }
            
            List<AssetReferenceMap> sceneAssets = new();

            // totally paralellizable, if anyone feels so inclined
            foreach (var assetPath in FetchAssetsFromSource(assetFolderOrRegistryFile))
            {
                if (!Config.WhitelistedFileExtensions.Contains(Path.GetExtension(assetPath))) continue;
                var parser = new UnityAssetParser(assetPath);
                sceneAssets.Add(parser.BuildAssetReferenceMap());
            }
            
            ManifestGenerator.GenerateAllAssetManifest(sceneAssets);
        }

        static string[] FetchAssetsFromSource(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            } else if (Directory.Exists(path))
            {
                HashSet<string> assets = [];
                foreach (var assetPath in Directory.GetFiles(path, Config.SearchPattern, SearchOption.AllDirectories))
                {
                    assets.Add(assetPath);
                }

                return assets.ToArray();
            }
            else
            {
                throw new ArgumentException("ERROR! Provided path does not match a folder or a file containing a list of asset paths. Given: " + path);
            }
        }
    }
}
