using VYaml.Annotations;
using VYaml.Parser;
using VYaml.Serialization;

namespace AssetParasite;

public class SceneParser
{
    private string scenePath;
    private SceneAssetReferenceMap assetMap;

    public SceneParser(string path)
    {
        scenePath = path;
        assetMap = new SceneAssetReferenceMap(Path.GetFileName(scenePath));
    }

    public SceneAssetReferenceMap BuildAssetReferenceMap()
    {
        var sceneBytes = File.ReadAllBytes(scenePath);
        var parser = YamlParser.FromBytes(sceneBytes);
        
        while (parser.Read())
        {
            if (parser.CurrentEventType == ParseEventType.DocumentStart)
            {
                if (!parser.TryGetCurrentAnchor(out var currentDocAnchor)) continue;
                
                parser.SkipAfter(ParseEventType.DocumentStart);

                var objectType = parser.ReadScalarAsString();

                switch (objectType)
                {
                    case "GameObject":
                        var go = GameObjectNode.ParseSelf(parser);
                        assetMap.goID2GameObject.Add(currentDocAnchor.Id, go);
                        break;
                    case "MonoBehaviour":
                        var mb = MonoBehaviourNode.ParseSelf(parser);
                        RegisterComponentAssets(mb);
                        break;
                    case "Transform":
                        var tsfm = TransformNode.ParseSelf(parser, currentDocAnchor.Id);
                        assetMap.tsfmID2Transform.Add(tsfm.FileID, tsfm);
                        assetMap.goID2Transform.Add(tsfm.GameObjectID, tsfm);
                        break;
                    default:
                        var cn = ComponentNode.ParseSelf(parser, objectType);
                        RegisterComponentAssets(cn);
                        break;
                }
            }
        }

        return assetMap;
    }

    private void RegisterComponentAssets(ComponentNode cn)
    {
        if (cn.Assets.Count > 0)
        {
            foreach (var assetRef in cn.Assets)
            {
                if (assetMap.assetGuid2Component.TryGetValue(assetRef.guid, out var value))
                {
                    value.Add(cn);
                }
                else
                {
                    assetMap.assetGuid2Component.Add(assetRef.guid, [cn]);
                }
            }
        } 
    } 
}