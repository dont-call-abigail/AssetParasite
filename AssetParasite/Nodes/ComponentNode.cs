using VYaml.Annotations;
using VYaml.Parser;

namespace AssetParasite;

[YamlObject]
public class ComponentNode
{
    public record AssetReference(string guid, string memberName);
    public List<AssetReference> Assets;
    public string ComponentType;
    public long GameObjectFileID;

    public override bool Equals(object? obj)
    {
        return obj is ComponentNode other && other.Assets == Assets;
    }

    public static ComponentNode ParseSelf(YamlParser parser, string componentType)
    {
        var newNode = new ComponentNode();
        newNode.ComponentType = componentType;
        
        while (parser.CurrentEventType != ParseEventType.DocumentEnd)
        {
            var key = parser.ReadScalarAsString();
            var scalar = parser.ReadScalarAsString();
            
            if (key == "m_GameObject")
            {
                newNode.GameObjectFileID = RegexUtil.ParseFileId(scalar!);
            }

            var potentialGuid = RegexUtil.ParseGuid(scalar);
            if (!string.IsNullOrEmpty(potentialGuid))
            {
                newNode.Assets ??= [];
                newNode.Assets.Add(new AssetReference(potentialGuid, key));
            }
        }

        return newNode;
    }
}