using VYaml.Parser;

namespace AssetParasite;

public class MonoBehaviourNode : ComponentNode
{
    public string ScriptGUID;

    public static MonoBehaviourNode ParseSelf(YamlParser parser)
    {
        var newNode = new MonoBehaviourNode();
        newNode.ComponentType = "MonoBehaviour";
        
        // duplicate code because reading from the parser moves the cursor
        while (parser.CurrentEventType != ParseEventType.DocumentEnd)
        {
            var key = parser.ReadScalarAsString();
            var scalar = parser.ReadScalarAsString();
            
            if (key == "m_GameObject")
            {
                newNode.GameObjectFileID = RegexUtil.ParseFileId(scalar!);
            } else if (key == "m_Script")
            {
                newNode.ScriptGUID = RegexUtil.ParseGuid(scalar!);
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