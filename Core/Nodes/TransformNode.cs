using VYaml.Parser;

namespace Core;

// For our purposes Transforms are not considered Components. Their chief usage is traversal in scene hierarchies.
public class TransformNode
{
    public long FileID;
    public long GameObjectID;
    public int RootOrder;
    public long Parent;
    public long[] Children;

    public static TransformNode ParseSelf(ref YamlParser cursor, long fileID)
    {
        var newNode = new TransformNode();
        newNode.FileID = fileID;
        
        while (!cursor.IsAt(ParseEventType.DocumentEnd))
        {
            if (cursor.IsAt(ParseEventType.Scalar))
            {
                var key = cursor.ReadScalarAsString();
                switch (key)
                {
                    case "m_GameObject":
                        newNode.GameObjectID = cursor.ReadFileID();
                        break;
                    case "m_RootOrder":
                        newNode.RootOrder = cursor.ReadScalarAsInt32();
                        break;
                    case "m_Father":
                        newNode.Parent = cursor.ReadFileID();
                        break;
                    case "m_Children":
                    {
                        List<long> childIDs = [];
                        while (!cursor.IsAt(ParseEventType.SequenceEnd))
                        {
                            cursor.Read();
                            var childID = cursor.ReadFileID();
                            if (childID != -1) childIDs.Add(childID);
                        }
                        newNode.Children = childIDs.ToArray();
                        break;
                    }
                }
            }

            cursor.Read();
        }

        return newNode;
    }
}