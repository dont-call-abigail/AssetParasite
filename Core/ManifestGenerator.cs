using Manifest;

namespace Core;

public class ManifestGenerator
{
    public static SceneDresserManifest GenerateManifest(string[] assetGuids, List<SceneAssetReferenceMap> scenes)
    {
        var manifest = new SceneDresserManifest();
         foreach (var guid in assetGuids)
         {
             foreach (var scene in scenes)
             { 
                 var foundAssets = scene.CreateManifestEntry(guid); 
                 if (foundAssets != null && !manifest.Assets.TryAdd(guid, foundAssets))
                 {
                    manifest.Assets[guid].AddRange(foundAssets);
                 }
                
                 if (manifest.Assets.TryGetValue(guid, out var items) && items.Count >= Config.MaxPathsPerAsset) break;
            }
         }
        
        return manifest;
    }

    public static SceneDresserManifest GenerateAllAssetManifest(List<SceneAssetReferenceMap> scenes)
    {
        // this is functional programming (in two ways)
        return GenerateManifest(scenes.Aggregate(Enumerable.Empty<string>(), 
            (current, scene) => current.Union(scene.assetGuid2Component.Keys)).ToArray(), scenes);
    }
}