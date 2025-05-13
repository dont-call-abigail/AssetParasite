using System.Text;

namespace AssetParasite;

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

    public bool SceneContainsAsset(string assetGuid)
    {
        return assetGuid2Component.ContainsKey(assetGuid);
    }

    public string ResolveHierarchyPath(long goID)
    {
        StringBuilder sb = new StringBuilder();
        TransformNode currNode = goID2Transform[goID];
        var parent = tsfmID2Transform[currNode.Parent];

        while (parent.FileID != 0)
        {
            sb.Insert(0, $"{parent.RootOrder};");
            currNode = tsfmID2Transform[currNode.Parent];
            parent = tsfmID2Transform[currNode.Parent];
        }

        var rootGO = goID2GameObject[currNode.GameObjectID];
        sb.Insert(0, $"{rootGO.Name};");

        return sb.ToString();
    }

    public string ResolveComponentAssetPath(string assetGuid, ComponentNode component)
    {
        var foundRecord = component.Assets.FindAll(refr => refr.guid == assetGuid);
        if (foundRecord.Count > 1)
        {
            Console.WriteLine($"WARNING: Found multiple refs for asset {assetGuid} on {component} (of {goID2GameObject[component.GameObjectFileID].Name})");
        }
        if (foundRecord.Count == 0)
        {
            return "";
        }
        
        if (component is MonoBehaviourNode scriptNode)
        {
            return $"{scriptNode.ComponentType};{foundRecord[0].memberName};{scriptNode.ScriptGUID}";
        }
        else
        {
            return $"{component.ComponentType};{foundRecord[0].memberName}";
        }
    }

}