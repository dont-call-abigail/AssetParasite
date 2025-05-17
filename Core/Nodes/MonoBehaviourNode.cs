using VYaml.Parser;

namespace Core;

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
                var memberName = parser.ReadScalarAsString();
                                
                if (memberName == "m_GameObject")
                {
                    newNode.GameObjectFileID = parser.ReadFileID();
                } else if (memberName == "m_Script")
                {
                    newNode.ScriptGUID = parser.ReadAssetGUID();
                }

                switch (parser.CurrentEventType)
                {
                    case ParseEventType.SequenceStart:
                        int index = 0;
                        while (!parser.End && !parser.IsAt(ParseEventType.SequenceEnd))
                        {
                            parser.Read();
                            var potentialGuid = parser.ReadAssetGUID();
                            if (!string.IsNullOrEmpty(potentialGuid))
                            {
                                newNode.Assets.Add(new AssetReference(potentialGuid, memberName!, index));
                            }
                            index++;
                        }
                        break;
                    case ParseEventType.MappingStart:
                        var potentialGuid1 = parser.ReadAssetGUID();
                        if (!string.IsNullOrEmpty(potentialGuid1))
                        {
                            newNode.Assets.Add(new AssetReference(potentialGuid1, memberName!, 0));
                        }
                        break;
                }
            }

            parser.Read();
        }
        
        return newNode;
    }
}