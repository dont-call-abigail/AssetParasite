using System.Collections.Generic;
using AssetCatalogue.Parser;
using VYaml.Parser;

namespace AssetCatalogue.Nodes
{
    public class ComponentNode
    {
        public class AssetReference
        {
            public string Guid;
            public string FieldName;
            public bool IsCollection;
            public int CollectionIndex;

            public AssetReference(string guid, string fieldName, bool isCollection, int collectionIndex)
            {
                Guid = guid;
                FieldName = fieldName;
                IsCollection = isCollection;
                CollectionIndex = collectionIndex;
            }
        }

        public List<AssetReference> Assets;
        public string ComponentType;
        public long GameObjectFileID;

        public static ComponentNode ParseSelf(ref YamlParser parser, string componentType)
        {
            var newNode = new ComponentNode();
            newNode.ComponentType = componentType;
            newNode.Assets = new List<AssetReference>();

            while (!parser.IsAt(ParseEventType.DocumentEnd))
            {
                if (parser.IsAt(ParseEventType.Scalar))
                {
                    var memberName = parser.ReadScalarAsString();

                    if (memberName == "m_GameObject")
                    {
                        newNode.GameObjectFileID = parser.ReadFileID();
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
                                    newNode.Assets.Add(new AssetReference(potentialGuid, memberName, true, index));
                                }
                                index++;
                            }
                            break;
                        case ParseEventType.MappingStart:
                            var potentialGuid1 = parser.ReadAssetGUID();
                            if (!string.IsNullOrEmpty(potentialGuid1))
                            {
                                newNode.Assets.Add(new AssetReference(potentialGuid1, memberName, false, -1));
                            }
                            break;
                    }
                }

                parser.Read();
            }

            return newNode;
        }
    }
}