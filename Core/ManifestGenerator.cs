using System.Collections.Generic;
using System.IO;
using System.Linq;
using DatabaseOps;

namespace AssetManifest
{
    public static class ManifestGenerator
    {
        private static DatabaseWriter _writer;
        private static DatabaseReader _reader;

        public static DatabaseWriter Writer
        {
            get
            {
                _writer ??= new DatabaseWriter(AssetParasite.Config.DatabasePath);
                return _writer;
            }
        }

        public static DatabaseReader Reader
        {
            get
            {
                _reader ??= new DatabaseReader(AssetParasite.Config.DatabasePath);
                return _reader;
            }
        }
    
        public static void GenerateManifest(string[] assetGuids, List<AssetReferenceMap> assets)
        {
            if (AssetParasite.Config.FlushModAssets)
            {
                Writer.RemoveModAssets(AssetParasite.Config.ModGuid);
            }
        
            foreach (var guid in assetGuids)
            {
                bool includedOnceFlag = false;
                foreach (var asset in assets)
                { 
                    if (!AssetParasite.Config.IncludeAllRefs && includedOnceFlag) break;
                    includedOnceFlag = asset.TryCreateManifestEntry(guid); 
                }
            }
        
        }

        public static void GenerateAllAssetManifest(List<AssetReferenceMap> scenes)
        {
            GenerateManifest(scenes.Aggregate(Enumerable.Empty<string>(), 
                (current, scene) => current.Union(scene.assetGuid2Component.Keys)).ToArray(), scenes);
        }

        public static void PopulateScriptIDs(string monoScriptsFolder)
        {
            // NB: this system assumes that filename matches typename. also, unity only recognizes one
            // MonoBehaviour/ScriptableObject declaration per file. so .meta files should have parity w/ Unity types

            string[] metaFiles = Directory.GetFiles(monoScriptsFolder, "*.cs.meta", SearchOption.AllDirectories);
            for (int i = 0; i < metaFiles.Length; i++)
            {
                string metaPath = metaFiles[i];
                string typeName = Path.GetFileNameWithoutExtension(metaPath);
                typeName = typeName.Substring(0, typeName.Length - 3);
                string guid = File.ReadAllLines(metaPath)[1].Split(' ')[1];
                Writer.InsertScriptTypeId(typeName, guid);
            }
        }
    }
}