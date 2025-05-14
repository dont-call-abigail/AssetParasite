using VYaml.Parser;

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
        
        Console.WriteLine($"Parsing scene {Path.GetFileName(scenePath)}");
        
        while (parser.Read())
        {
            if (parser.IsAt(ParseEventType.DocumentStart))
            {
                parser.SkipAfter(ParseEventType.DocumentStart);
                parser.TryGetCurrentAnchor(out var currentDocAnchor);
                if (parser.IsAt(ParseEventType.MappingStart))
                {
                    parser.Read();
                    
                    var objectType = parser.ReadScalarAsString();
                    Console.WriteLine($"     Processing object {currentDocAnchor.Id} @ {parser.CurrentMark} (Type {objectType})");
                    
                    switch (objectType)
                    {
                        case "GameObject":
                            var go = GameObjectNode.ParseSelf(ref parser);
                            assetMap.goID2GameObject.Add(currentDocAnchor.Id, go);
                            break;
                        case "MonoBehaviour":
                            var mb = MonoBehaviourNode.ParseSelf(ref parser);
                            RegisterComponentAssets(mb);
                            break;
                        case "Transform":
                            var tsfm = TransformNode.ParseSelf(ref parser, currentDocAnchor.Id);
                            assetMap.tsfmID2Transform.Add(tsfm.FileID, tsfm);
                            assetMap.goID2Transform.Add(tsfm.GameObjectID, tsfm);
                            break;
                        default:
                            var cn = ComponentNode.ParseSelf(ref parser, objectType);
                            RegisterComponentAssets(cn);
                            // components without assets won't be ref'd after this point and will be GC'd
                            break;
                    }
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
                Console.WriteLine($"     FOUND: {assetRef.memberName} @ {assetRef.guid}");
            }
        } 
    } 
}