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
                if (lonelyArg is "--version" or "-v")
                {
                    Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version);
                    return;
                }
            }

            string assetFolderPath = args[0];
            string[]? assetGuids = null;
            string? outputPath = null;
            
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
                } else if (arg is "--script-names" or "-s")
                {
                    var lines = File.ReadAllLines(args[++i]);
                    foreach (var line in lines)
                    {
                        var parts = line.Split(',');
                        if (parts.Length != 2)
                        {
                            throw new ArgumentException(
                                $"Reached malformed line in provided GUID -> MonoBehaviour mapping: {line}\nRun AssetParasite --help for usage instructions.");
                        }
                        Config.Guid2ScriptName.TryAdd(parts[0], parts[1]);
                    }
                } else if (arg is "-a" or "--find-all")
                {
                    Config.IncludeAllRefs = true;
                } else if (arg is "--fresh" or "-f")
                {
                    if (File.Exists(Config.DatabasePath)) File.Delete(Config.DatabasePath);
                    Console.WriteLine("Removing database file...");
                } else if (arg is "--mod-guid" or "-m")
                {
                    Config.ModGuid = args[++i];
                } else if (arg is "--verbose" or "-v")
                {
                    Config.VerboseLogging = true;
                }
            }
            
            List<AssetReferenceMap> sceneAssets = new();

            // totally paralellizable, if anyone feels so inclined
            foreach (var assetPath in Directory.GetFiles(assetFolderPath, Config.SearchPattern))
            {
                if (!Config.WhitelistedFileExtensions.Contains(Path.GetExtension(assetPath))) continue;
                var parser = new UnityAssetParser(assetPath);
                sceneAssets.Add(parser.BuildAssetReferenceMap());
            }

            if (assetGuids != null)
            {
                ManifestGenerator.GenerateManifest(assetGuids, sceneAssets);
            }
            else
            {
                ManifestGenerator.GenerateAllAssetManifest(sceneAssets);
            }

        }
    }
}
