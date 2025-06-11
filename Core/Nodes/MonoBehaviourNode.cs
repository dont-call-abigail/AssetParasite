using System.Collections.Generic;
using AssetManifest.Parser;
using VYaml.Parser;

namespace AssetManifest.Nodes
{
    public class MonoBehaviourNode : ComponentNode
    {
        // Technically we could figure out the script name/type from the Guid.
        // To avoid recreating the wheel we'll leave that step to Unity's AssetDatabase.
        public string ScriptGUID;

        public static MonoBehaviourNode ParseSelf(ref YamlParser parser)
        {
            var newNode = new MonoBehaviourNode();
            newNode.ComponentType = "MonoBehaviour";
            newNode.Assets = new List<AssetReference>();
        
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
                        newNode.ComponentType = ManifestGenerator.Reader.GetComponentType(newNode.ScriptGUID);
                        if (newNode.ComponentType == "MonoBehaviour" && AssetParasite.Config.VerboseLogging)
                        {
                            AssetParasite.Logger.WriteLine($"WARNING: Could not find derived type for MonoBehaviour with GUID {newNode.ScriptGUID}");
                        }
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