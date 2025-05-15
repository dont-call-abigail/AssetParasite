using Manifest;

namespace Core;

public class AssetLocationManifest
{
    public static SceneDresserManifest GenerateManifest(string[] assetGuids, SceneAssetReferenceMap[] scenes)
    {
        var manifest = new SceneDresserManifest();
        foreach (var scene in scenes)
        {
            foreach (var guid in assetGuids)
            {
                var foundAssets = scene.CreateManifestEntry(guid);
                if (foundAssets != null && !manifest.Assets.TryAdd(guid, foundAssets))
                {
                    manifest.Assets[guid].AddRange(foundAssets);
                }
            }
        }
        
        return manifest;
    }

    public static SceneDresserManifest GenerateAllAssetManifest(SceneAssetReferenceMap[] scenes)
    {
        return GenerateManifest(scenes.SelectMany(s => s.assetGuid2Component.Keys.ToArray()).ToArray(), scenes);
    }
}