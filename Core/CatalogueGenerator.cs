using System.Collections.Generic;
using System.IO;
using System.Linq;
using AssetCatalogue.Database;

namespace AssetCatalogue
{
    public static class CatalogueGenerator
    {
        private static CatalogueWriter _writer;
        private static CatalogueReader _reader;

        public static CatalogueWriter Writer
        {
            get
            {
                _writer ??= new CatalogueWriter(AssetParasite.Config.DatabasePath);
                return _writer;
            }
        }

        public static CatalogueReader Reader
        {
            get
            {
                _reader ??= new CatalogueReader(AssetParasite.Config.DatabasePath);
                return _reader;
            }
        }
    
        public static void GenerateCatalogue(string[] assetGuids, List<AssetReferenceMap> assetRefMaps)
        {
            if (AssetParasite.Config.FlushModAssets)
            {
                Writer.RemoveModAssets(AssetParasite.Config.ModGuid);
            }
        
            foreach (var guid in assetGuids)
            {
                bool includedOnceFlag = false;
                if (AssetParasite.Config.OnlyRegisterBasegameMatches && !Reader.DependantExistsForAsset(guid, "basegame"))
                {
                    if (AssetParasite.Config.VerboseLogging)
                    {
                        AssetParasite.Logger.WriteLine($"Skipping registration of asset {guid} due to lack of basegame match!");
                    }
                    break;
                }
                
                foreach (var referenceMap in assetRefMaps)
                { 
                    if (!AssetParasite.Config.IncludeAllRefs && includedOnceFlag) break;
                    includedOnceFlag = referenceMap.TryCreateCatalogueEntry(guid); 
                }
            }
            
            AssetParasite.Logger.WriteLine($"Success! Asset reference database is built.");
            DestroyInterfaces();
        }

        public static void GenerateAllAssetCatalogue(List<AssetReferenceMap> scenes)
        {
            GenerateCatalogue(scenes.Aggregate(Enumerable.Empty<string>(), 
                (current, scene) => current.Union(scene.assetGuid2Component.Keys)).ToArray(), scenes);
        }

        public static void PopulateScriptIDs(string monoScriptsFolder)
        {
            // NB: this system assumes that filename matches typename. since unity only recognizes one
            // UnityEngine.Object declaration per file, .meta files *should* have parity w/ Unity types

            string[] metaFiles = Directory.GetFiles(monoScriptsFolder, "*.cs.meta", SearchOption.AllDirectories);
            for (int i = 0; i < metaFiles.Length; i++)
            {
                string metaPath = metaFiles[i];
                string typeName = Path.GetFileNameWithoutExtension(metaPath);
                typeName = typeName.Substring(0, typeName.Length - 3);
                string guid = File.ReadAllLines(metaPath)[1].Split(' ')[1];
                Writer.InsertComponentType(typeName, guid);
            }
        }

        private static void DestroyInterfaces()
        {
            _writer.Dispose();
            _reader.Dispose();
            _writer = null;
            _reader = null;
        }
    }
}