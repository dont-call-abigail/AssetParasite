using System.Text;

namespace Core;

public class AssetReferenceMap
{
    public readonly string assetName;
    public Dictionary<long, TransformNode> goID2Transform;
    public Dictionary<long, GameObjectNode> goID2GameObject;
    public Dictionary<string, List<ComponentNode>> assetGuid2Component;
    public Dictionary<long, TransformNode> tsfmID2Transform;

    public AssetReferenceMap(string assetName)
    {
        this.assetName = assetName;
        goID2Transform = [];
        goID2GameObject = [];
        assetGuid2Component = [];
        tsfmID2Transform = [];
    }
    
    public bool TryCreateManifestEntry(string guid)
    {
        if (!ContainsAsset(guid))
        {
            return false;
        }
        
        var foundAssets = assetGuid2Component[guid];
        for (var i = 0; i < foundAssets.Count; i++)
        {
            var component = foundAssets[i];
            var foundRecord = component.Assets.FindAll(refr => refr.Guid == guid);
            if (foundRecord.Count > 1)
            {
                Console.WriteLine(
                    $"WARNING: Found {foundRecord.Count} refs for asset {guid} on {component.ComponentType} (of {goID2GameObject[component.GameObjectFileID].Name})");
                i += foundRecord.Count - 1;
            }

            if (foundRecord.Count == 0)
            {
                throw new Exception($"There are no assets for GUID {guid} in scene {assetName}");
            }

            var componentIdx = ResolveComponentData(foundRecord[0], component);
            var hierarchyPath = ResolveHierarchyPath(component.GameObjectFileID);


            ManifestGenerator.Writer.InsertAsset(Config.ModGuid, assetName, guid, hierarchyPath.goName, hierarchyPath.tsfmPath,
                    componentIdx);

        }

        return true;
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

    private long ResolveComponentData(ComponentNode.AssetReference @ref, ComponentNode component)
    {
        long propId;
        if (component is MonoBehaviourNode scriptNode)
        {
            propId = ManifestGenerator.Writer.InsertPropertyData(component.ComponentType, @ref.MemberName, @ref.IsCollection, @ref.CollectionIndex, 
                Config.Guid2ScriptName.TryGetValue(scriptNode.ScriptGUID, out var type) ? type : scriptNode.ScriptGUID);
        }
        else
        {
            propId = ManifestGenerator.Writer.InsertPropertyData(component.ComponentType, @ref.MemberName, @ref.IsCollection, @ref.CollectionIndex);
        }
        
        return propId;
    }

}