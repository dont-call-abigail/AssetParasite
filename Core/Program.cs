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

            string sceneFolderPath = args[0];
            string[]? assetGuids = null;
            string? outputPath = null;
            
            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg is "--exclude-assets" or "-a")
                {
                    var excludeAssets = args[++i].Split(',');
                    foreach (var assetId in excludeAssets)
                    {
                        Config.ExcludedAssets.Add(assetId);
                    }
                } else if (arg is "--exclude-scenes" or "-s")
                {
                    var excludeScenes = args[++i].Split(',');
                    foreach (var sceneName in excludeScenes)
                    {
                        Config.ExcludedScenes.Add(sceneName.EndsWith(".unity") ? sceneName : sceneName + ".unity");
                    }
                } else if (arg is "--assets" or "-a")
                {
                    assetGuids = args[++i].Split();
                } else if (arg is "--script-names" or "-n")
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
                }
            }
            
            List<AssetReferenceMap> sceneAssets = new();

            // totally paralellizable, if anyone feels so inclined
            foreach (var scenePath in Directory.GetFiles(sceneFolderPath, "*.unity"))
            {
                if (Config.ExcludedScenes.Contains(Path.GetFileName(scenePath))) continue;
                var parser = new UnityAssetParser(scenePath);
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
