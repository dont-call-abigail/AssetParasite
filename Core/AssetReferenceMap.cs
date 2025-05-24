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
        foreach (var component in foundAssets)
        {
            var componentIdx = ResolveComponentData(guid, component);
            var hierarchyPath = ResolveHierarchyPath(component.GameObjectFileID);
            ManifestGenerator.Writer.InsertGameAsset(assetName, guid, hierarchyPath.goName, hierarchyPath.tsfmPath, componentIdx);
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

    private long ResolveComponentData(string assetGuid, ComponentNode component)
    {
        var foundRecord = component.Assets.FindAll(refr => refr.Guid == assetGuid);
        if (foundRecord.Count > 1)
        {
            Console.WriteLine($"WARNING: Found multiple refs for asset {assetGuid} on {component} (of {goID2GameObject[component.GameObjectFileID].Name})");
        }
        if (foundRecord.Count == 0)
        {
            throw new Exception($"There are no assets for GUID {assetGuid} in scene {assetName}");
        }
        
        var recordData = foundRecord[0];
        long propId;
        if (component is MonoBehaviourNode scriptNode)
        {
            propId = ManifestGenerator.Writer.InsertPropertyData(component.ComponentType, recordData.MemberName, recordData.IsCollection, recordData.CollectionIndex, scriptNode.ScriptGUID);
        }
        else
        {
            propId = ManifestGenerator.Writer.InsertPropertyData(component.ComponentType, recordData.MemberName, recordData.IsCollection, recordData.CollectionIndex);
        }
        
        return propId;
    }

}