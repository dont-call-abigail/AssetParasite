namespace AssetParasite;

public class AssetLocationManifest
{
    struct ManifestEntry
    {
        public string SceneName;
        public List<ManifestAssetPath> AssetLocations;
    }

    struct ManifestAssetPath
    {
        public string ScenePath;
        public string ComponentData;
    }

    private Dictionary<string, ManifestEntry> Assets;

    public static void BuildManifest(SceneAssetReferenceMap[] scenes, string[]? requestedGuids = null)
    {
        
    }
}