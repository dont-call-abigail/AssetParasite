using VYaml.Parser;

namespace AssetParasite;

// For our purposes Transforms are not considered Components. Their chief usage is traversal in scene hierarchies.
public class TransformNode
{
    public long FileID;
    public long GameObjectID;
    public int RootOrder;
    public long Parent;
    public long[] Children;

    public static TransformNode ParseSelf(ref YamlParser parser, long fileID)
    {
        var newNode = new TransformNode();
        newNode.FileID = fileID;
        
        while (!parser.IsAt(ParseEventType.DocumentEnd))
        {
            if (parser.IsAt(ParseEventType.Scalar))
            {
                var key = parser.ReadScalarAsString();
                switch (key)
                {
                    case "m_GameObject":
                        newNode.GameObjectID = parser.ReadFileID();
                        break;
                    case "m_RootOrder":
                        newNode.RootOrder = parser.ReadScalarAsInt32();
                        break;
                    case "m_Father":
                        newNode.Parent = parser.ReadFileID();
                        break;
                    case "m_Children":
                    {
                        List<long> childIDs = [];
                        while (!parser.IsAt(ParseEventType.SequenceEnd))
                        {
                            parser.Read();
                            var childID = parser.ReadFileID();
                            if (childID != -1) childIDs.Add(childID);
                        }
                        newNode.Children = childIDs.ToArray();
                        break;
                    }
                }
            }

            parser.Read();
        }

        return newNode;
    }
}