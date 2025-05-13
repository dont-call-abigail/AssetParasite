using VYaml.Parser;

namespace AssetParasite;

// For our purposes, Transforms are not considered Components. Their chief usage is traversal in scene hierarchies.
public class TransformNode
{
    public long FileID;
    public long GameObjectID;
    public int RootOrder;
    public long Parent;
    public long[] Children;

    public static TransformNode ParseSelf(YamlParser parser, long fileID)
    {
        var newNode = new TransformNode();
        newNode.FileID = fileID;
        
        while (parser.CurrentEventType != ParseEventType.DocumentEnd)
        {
            var key = parser.ReadScalarAsString();
            var scalar = parser.ReadScalarAsString();
            
            switch (key)
            {
                case "m_GameObject":
                    newNode.GameObjectID = RegexUtil.ParseFileId(scalar);
                    break;
                case "m_RootOrder":
                    newNode.RootOrder = int.Parse(scalar);
                    break;
                case "m_Father":
                    newNode.Parent = RegexUtil.ParseFileId(scalar);
                    break;
                case "m_Children":
                    newNode.Children = scalar.Split("-").Select(RegexUtil.ParseFileId).ToArray();
                    break;
            }
        }

        return newNode;
    }
}