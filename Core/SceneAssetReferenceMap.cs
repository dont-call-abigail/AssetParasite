using System.Text;
using Manifest;

namespace Core;

public class SceneAssetReferenceMap
{
    public string sceneName;
    public Dictionary<long, TransformNode> goID2Transform;
    public Dictionary<long, GameObjectNode> goID2GameObject;
    public Dictionary<string, List<ComponentNode>> assetGuid2Component;
    public Dictionary<long, TransformNode> tsfmID2Transform;

    public SceneAssetReferenceMap(string sceneName)
    {
        this.sceneName = sceneName;
        goID2Transform = [];
        goID2GameObject = [];
        assetGuid2Component = [];
        tsfmID2Transform = [];
    }
    
    public List<SceneDresserManifest.ManifestAssetPath>? CreateManifestEntry(string guid)
    {
        if (!ContainsAsset(guid))
        {
            return null;
        }

        var manifestPaths = new List<SceneDresserManifest.ManifestAssetPath>();
        var foundAssets = assetGuid2Component[guid];
        
        foreach (var component in foundAssets.Take(Config.MaxAssetPathsIncludedPerScene))
        {
            var componentData = ResolveComponentData(guid, component);
            var hierarchyPath = ResolveHierarchyPath(component.GameObjectFileID);

            manifestPaths.Add(new SceneDresserManifest.ManifestAssetPath
            {
                SceneName = sceneName,
                Component = componentData,
                TransformPath = hierarchyPath.tsfmPath,
                BaseGameObject = hierarchyPath.goName
            });
        }
        
        return manifestPaths;
    }

    public bool ContainsAsset(string assetGuid)
    {
        return assetGuid2Component.ContainsKey(assetGuid);
    }

    private (string goName, string tsfmPath) ResolveHierarchyPath(long goID)
    {
        if (goID == 0) return ("", "");
        StringBuilder sb = new StringBuilder();
        TransformNode currNode = goID2Transform[goID];

        while (currNode.Parent != 0)
        {
            if (sb.Length >= 1) sb.Insert(0, ';');
            sb.Insert(0, $"{currNode.RootOrder}");
            currNode = tsfmID2Transform[currNode.Parent];
        }

        var rootGO = goID2GameObject[currNode.GameObjectID];

        return (rootGO.Name, sb.ToString());
    }

    private SceneDresserManifest.ManifestComponentData ResolveComponentData(string assetGuid, ComponentNode component)
    {
        var foundRecord = component.Assets.FindAll(refr => refr.Guid == assetGuid);
        if (foundRecord.Count > 1)
        {
            Console.WriteLine($"WARNING: Found multiple refs for asset {assetGuid} on {component} (of {goID2GameObject[component.GameObjectFileID].Name})");
        }
        if (foundRecord.Count == 0)
        {
            throw new Exception($"There are no assets for GUID {assetGuid} in scene {sceneName}");
        }

        var componentData = new SceneDresserManifest.ManifestComponentData
        {
            ComponentType = component.ComponentType,
            MemberName = foundRecord[0].MemberName,
            CollectionIndex = foundRecord[0].CollectionIndex
        };
        
        if (component is MonoBehaviourNode scriptNode)
        {
            componentData.ScriptGuid = scriptNode.ScriptGUID;
        }

        return componentData;
    }

}