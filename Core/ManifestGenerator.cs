using DatabaseOps;

namespace Core;

public static class ManifestGenerator
{
    private static DatabaseWriter? _writer;
    private static DatabaseReader? _reader;

    public static DatabaseWriter Writer
    {
        get
        {
            _writer ??= new DatabaseWriter(Config.DatabasePath);
            return _writer;
        }
    }

    public static DatabaseReader Reader
    {
        get
        {
            _reader ??= new DatabaseReader();
            return _reader;
        }
    }
    
    public static void GenerateManifest(string[] assetGuids, List<AssetReferenceMap> assets)
    {
        foreach (var guid in assetGuids)
        {
            bool includedOnceFlag = false;
            foreach (var asset in assets)
            { 
                if (!Config.IncludeAllRefs && includedOnceFlag) break;
                includedOnceFlag = asset.TryCreateManifestEntry(guid); 
            }
        }
        
    }

    public static void GenerateAllAssetManifest(List<AssetReferenceMap> scenes)
    {
        GenerateManifest(scenes.Aggregate(Enumerable.Empty<string>(), 
            (current, scene) => current.Union(scene.assetGuid2Component.Keys)).ToArray(), scenes);
    }
}