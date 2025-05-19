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

            string sceneFolderPath = args[0];
            string[]? assetGuids = null;
            string? outputPath = null;
            
            int i = 1;
            for (; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg is "--help" or "-h" || args.Length < 2)
                {
                    Console.WriteLine(Config.HelpMessage);
                    return;
                } else if (arg is "--version" or "-v")
                {
                    Console.WriteLine(Assembly.GetEntryAssembly().GetName().Version);
                    return;
                } else if (arg is "--exclude-assets" or "-a")
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
                } else if (arg is "-p")
                {
                    Config.PrettyPrint = true;
                }
            }
            
            List<SceneAssetReferenceMap> sceneAssets = new();

            // totally paralellizable, if anyone feels so inclined
            foreach (var scenePath in Directory.GetFiles(sceneFolderPath, "*.unity"))
            {
                if (Config.ExcludedScenes.Contains(Path.GetFileName(scenePath))) continue;
                var parser = new SceneParser(scenePath);
                sceneAssets.Add(parser.BuildAssetReferenceMap());
            }

            var manifest = assetGuids != null ? 
                ManifestGenerator.GenerateManifest(assetGuids, sceneAssets) 
                : ManifestGenerator.GenerateAllAssetManifest(sceneAssets);

#pragma warning disable CA1869
            string serializedManifest = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = Config.PrettyPrint });
#pragma warning restore CA1869
            File.WriteAllText(!string.IsNullOrEmpty(outputPath) ? outputPath : "manifest.json", serializedManifest);
        }
    }
}
