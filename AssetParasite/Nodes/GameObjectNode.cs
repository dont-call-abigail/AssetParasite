
using VYaml.Parser;

namespace AssetParasite;

public class GameObjectNode
{
    public string Name;

    public static GameObjectNode ParseSelf(YamlParser parser)
    {
        var newNode = new GameObjectNode();
        while (parser.CurrentEventType != ParseEventType.DocumentEnd)
        {
            var key = parser.ReadScalarAsString();
            var scalar = parser.ReadScalarAsString();

            if (key == "m_Name")
            {
                newNode.Name = scalar;
            }
        }

        return newNode;
    }
}