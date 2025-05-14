using System.Diagnostics;
using VYaml.Parser;

namespace AssetParasite;

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

    public static ComponentNode ParseSelf(ref YamlParser parser, string componentType)
    {
        var newNode = new ComponentNode();
        newNode.ComponentType = componentType;
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
                }

                parser.Read();
                switch (parser.CurrentEventType)
                {
                    case ParseEventType.SequenceStart:
                        while (!parser.End && !parser.IsAt(ParseEventType.SequenceEnd))
                        {
                            parser.Read();
                            var potentialGuid = parser.ReadAssetGUID();
                            if (!string.IsNullOrEmpty(potentialGuid))
                            {
                                newNode.Assets.Add(new AssetReference(potentialGuid, memberName));
                            }
                        }
                        break;
                    case ParseEventType.MappingStart:
                        var potentialGuid1 = parser.ReadAssetGUID();
                        if (!string.IsNullOrEmpty(potentialGuid1))
                        {
                            newNode.Assets.Add(new AssetReference(potentialGuid1, memberName));
                        }
                        break;
                }
            }

            parser.Read();
        }

        return newNode;
    }
}