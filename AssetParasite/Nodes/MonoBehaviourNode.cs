using VYaml.Parser;

namespace AssetParasite;

public class MonoBehaviourNode : ComponentNode
{
    public string ScriptGUID;

    public static MonoBehaviourNode ParseSelf(ref YamlParser parser)
    {
        var newNode = new MonoBehaviourNode();
        newNode.ComponentType = "MonoBehaviour";
        newNode.Assets = [];
        
        while (!parser.IsAt(ParseEventType.DocumentEnd))
        {
            if (parser.IsAt(ParseEventType.Scalar))
            {
                var memberName = parser.GetScalarAsString();
                
                if (memberName == "m_GameObject")
                {
                    parser.Read();
                    newNode.GameObjectFileID = parser.ReadFileID();
                } else if (memberName == "m_Script")
                {
                    parser.Read();
                    newNode.ScriptGUID = parser.ReadAssetGUID();
                }

                parser.Read(); // Reading manually; for some reason ReadScalar* is not moving the cursor like I thought
                switch (parser.CurrentEventType)
                {
                    case ParseEventType.SequenceStart:
                        while (!parser.End && !parser.IsAt(ParseEventType.SequenceEnd))
                        {
                            parser.Read();
                            var potentialGuid = parser.ReadAssetGUID();
                            if (!string.IsNullOrEmpty(potentialGuid))
                            {
                                newNode.Assets.Add(new AssetReference(potentialGuid, memberName!));
                            }
                        }
                        break;
                    case ParseEventType.MappingStart:
                        var potentialGuid1 = parser.ReadAssetGUID();
                        if (!string.IsNullOrEmpty(potentialGuid1))
                        {
                            newNode.Assets.Add(new AssetReference(potentialGuid1, memberName!));
                        }
                        break;
                }
            }

            parser.Read();
        }
        
        return newNode;
    }
}