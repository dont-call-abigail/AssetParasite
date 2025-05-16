using System.Text;
using VYaml.Parser;

namespace Core;

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
        var cursor = YamlParser.FromBytes(sceneBytes);
        
        Console.WriteLine($"Parsing scene {Path.GetFileName(scenePath)}");
        
        while (cursor.Read())
        {
            if (cursor.IsAt(ParseEventType.DocumentStart))
            {
                ParserExtensions.PrintBlock(sceneBytes,cursor.CurrentMark.Position + 1, 4);
                long currentAnchor = DecipherAnchor(sceneBytes, cursor.CurrentMark.Position + 2);
                cursor.SkipAfter(ParseEventType.DocumentStart);
                if (cursor.IsAt(ParseEventType.MappingStart))
                {
                    cursor.Read();
                    
                    var objectType = cursor.ReadScalarAsString();
                    Console.WriteLine($"     Processing object {currentAnchor} @ {cursor.CurrentMark} (Type {objectType})");
                    
                    switch (objectType)
                    {
                        case "GameObject":
                            var go = GameObjectNode.ParseSelf(ref cursor);
                            assetMap.goID2GameObject.Add(currentAnchor, go);
                            break;
                        case "MonoBehaviour":
                            var mb = MonoBehaviourNode.ParseSelf(ref cursor);
                            RegisterComponentAssets(mb);
                            break;
                        case "Transform":
                            var tsfm = TransformNode.ParseSelf(ref cursor, currentAnchor);
                            assetMap.tsfmID2Transform.Add(tsfm.FileID, tsfm);
                            assetMap.goID2Transform.Add(tsfm.GameObjectID, tsfm);
                            break;
                        default:
                            var cn = ComponentNode.ParseSelf(ref cursor, objectType);
                            RegisterComponentAssets(cn);
                            // components without assets won't be ref'd after this point and will be GC'd
                            break;
                    }
                }
            }
        }

        return assetMap;
    }

    private long DecipherAnchor(byte[] source, int sourceIdx)
    {
        byte[] caughtBytes = new byte[10];
        Array.Copy(source, sourceIdx, caughtBytes, 0, 10);
        string intStr = System.Text.Encoding.UTF8.GetString(caughtBytes);
        var anchor = int.Parse(intStr.Split('\n')[0].Split(' ')[0]);

        return anchor;
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