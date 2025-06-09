using AssetManifest.Parser;
using VYaml.Parser;

namespace AssetManifest.Nodes
{
    public class GameObjectNode
    {
        public string Name;

        public static GameObjectNode ParseSelf(ref YamlParser parser)
        {
            var newNode = new GameObjectNode();
            while (!parser.IsAt(ParseEventType.DocumentEnd))
            {
                if (parser.IsAt(ParseEventType.Scalar))
                {
                    var nameFound = parser.TryGetSpecificScalar("m_Name", out string potentialName);
                    if (nameFound)
                    {
                        newNode.Name = potentialName;
                    }
                }

                parser.Read();
            }

            return newNode;
        }
    }
}