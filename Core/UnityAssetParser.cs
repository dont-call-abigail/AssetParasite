using System.Text;
using VYaml.Parser;

namespace Core;

public class UnityAssetParser
{
    private string assetPath;
    private AssetReferenceMap assetMap;

    public UnityAssetParser(string path)
    {
        assetPath = path;
        assetMap = new AssetReferenceMap(Path.GetFileName(assetPath));
    }

    public AssetReferenceMap BuildAssetReferenceMap()
    {
        var sceneBytes = File.ReadAllBytes(assetPath);
        var cursor = YamlParser.FromBytes(sceneBytes);
        
        Console.WriteLine($"Parsing asset {Path.GetFileName(assetPath)}");
        
        while (cursor.Read())
        {
            if (cursor.IsAt(ParseEventType.DocumentStart))
            {
                //ParserExtensions.PrintBlock(sceneBytes,cursor.CurrentMark.Position + 1, 4);
                long currentAnchor = DecipherAnchor(sceneBytes, cursor.CurrentMark.Position + 2);
                cursor.SkipAfter(ParseEventType.DocumentStart);
                cursor.TryGetCurrentTag(out Tag t);
                Console.WriteLine("Tag : " + t.ToString());
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
                        case "Transform" or "RectTransform":
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
        byte[] caughtBytes = new byte[Config.AnchorByteBufferSize];
        Array.Copy(source, sourceIdx, caughtBytes, 0, Config.AnchorByteBufferSize);
        Span<char> intStr = Encoding.UTF8.GetChars(caughtBytes).AsSpan();
        int numberEndIdx = 0;
        for (int i = 0; i < intStr.Length; i++)
        {
            if (!char.IsNumber(intStr[i]))
            {
                numberEndIdx = i;
                break;
            }
        }

        return int.Parse(intStr[..numberEndIdx]);
    }

    private void RegisterComponentAssets(ComponentNode cn)
    {
        if (cn.Assets.Count > 0)
        {
            foreach (var assetRef in cn.Assets)
            {
                if (assetMap.assetGuid2Component.TryGetValue(assetRef.Guid, out var value))
                {
                    value.Add(cn);
                }
                else
                {
                    assetMap.assetGuid2Component.Add(assetRef.Guid, [cn]);
                }
                Console.WriteLine($"     FOUND: {assetRef.MemberName} @ {assetRef.Guid}");
            }
        } 
    }
}