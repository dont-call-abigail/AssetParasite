using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using AssetCatalogue.Parser;

namespace AssetCatalogue
{
    public class AssetParasite
    {
        public static ParserConfig Config;
        public static Logger Logger = new Logger();
        
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                AssetParasite.Logger.WriteLine(ParserConfig.HelpMessage);
                return;
            } 
            
            if (args.Length == 1)
            {
                string lonelyArg = args[0];
                if (lonelyArg == "--help" || lonelyArg == "-h")
                {
                    Logger.WriteLine(ParserConfig.HelpMessage);
                    return;
                }
                if (lonelyArg == "--version" || lonelyArg == "-v")
                {
                    Logger.WriteLine(Assembly.GetEntryAssembly().GetName().Version);
                    return;
                }
            }

            Config = new ParserConfig();

            string assetFolderOrRegistryFile = args[0];
            
            for (int i = 1; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "--output-file" || arg == "-o")
                {
                    Config.DatabasePath = args[++i];
                } else if (arg == "--search-pattern" || arg == "-p")
                {
                    Config.SearchPattern = args[++i];
                } else if (arg == "--exclude-assets" || arg == "-e")
                {
                    var excludeAssets = args[++i].Split(',');
                    foreach (var asset in excludeAssets)
                    {
                        Config.ExcludedAssets.Add(asset);
                    }
                } else if (arg == "--script-path" || arg == "-s")
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

                } else if (arg == "-a" || arg == "--find-all")
                {
                    Config.IncludeAllRefs = true;
                } else if (arg == "--fresh" || arg == "-f")
                {
                    Config.FlushModAssets = true;
                } else if (arg == "--mod-guid" || arg == "-m")
                {
                    Config.ModGuid = args[++i];
                } else if (arg == "--verbose" || arg == "-v")
                {
                    Config.VerboseLogging = true;
                } else if (arg == "--scenes" || arg == "-u")
                {
                    Config.SearchPattern = "*.unity";
                } else if (arg == "--only-matches" || arg == "-o")
                {
                    Config.OnlyRegisterBasegameMatches = true;
                }
            }
            
            RunCatalogueStandalone(assetFolderOrRegistryFile);
        }

        private static void RunCatalogueStandalone(string assetFolderOrRegistryFile)
        {
            RegisterMonoScripts();
            List<AssetReferenceMap> sceneAssets = new List<AssetReferenceMap>();

            // totally paralellizable, if anyone feels so inclined
            foreach (var assetPath in FetchAssetsFromSource(assetFolderOrRegistryFile))
            {
                if (!Config.WhitelistedFileExtensions.Contains(Path.GetExtension(assetPath))) continue;
                var parser = new UnityAssetParser(assetPath);
                sceneAssets.Add(parser.BuildAssetReferenceMap());
            }
            
            CatalogueGenerator.GenerateAllAssetCatalogue(sceneAssets);
        }

        public static void CatalogueAssets(IEnumerable<string> assetPaths, ParserConfig config)
        {
            Config = config;
            
            RegisterMonoScripts();
            List<AssetReferenceMap> sceneAssets = new List<AssetReferenceMap>();

            // totally paralellizable, if anyone feels so inclined
            foreach (var assetPath in assetPaths)
            {
                if (!Config.WhitelistedFileExtensions.Contains(Path.GetExtension(assetPath))) continue;
                var parser = new UnityAssetParser(assetPath);
                sceneAssets.Add(parser.BuildAssetReferenceMap());
            }
            
            CatalogueGenerator.GenerateAllAssetCatalogue(sceneAssets);
        }

        public static void CatalogueAssets(string assetFolder, ParserConfig config)
        {
            Config = config;
            CatalogueAssets(FetchAssetsFromSource(assetFolder), config);
        }

        static void RegisterMonoScripts()
        {
            if (!string.IsNullOrEmpty(Config.MonoScriptsFolder))
            {
                AssetParasite.Logger.WriteLine($"MonoScripts folder : {Config.MonoScriptsFolder}");
                CatalogueGenerator.PopulateScriptIDs(Config.MonoScriptsFolder);
            }
        }

        static string[] FetchAssetsFromSource(string path)
        {
            if (File.Exists(path))
            {
                return File.ReadAllLines(path);
            } else if (Directory.Exists(path))
            {
                HashSet<string> assets = new HashSet<string> { };
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
