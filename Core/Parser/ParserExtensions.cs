using System;
using System.Text;
using VYaml.Parser;

namespace AssetManifest.Parser
{
    public static class ParserExtensions
    {
        /// <summary>
        /// Expects a mapping. Will consume the full mapping to find a FileID.
        /// </summary>
        public static long ReadFileID(this ref YamlParser parser)
        {
            if (parser.IsAt(ParseEventType.MappingStart))
            {
                parser.Read();

                while (!parser.IsAt(ParseEventType.MappingEnd))
                {
                    var key = parser.ReadScalarAsString();
                    var value = parser.ReadScalarAsString();

                    if (key == "fileID")
                    {
                        return long.TryParse(value, out long res) ? res : -1;
                    }
                }

                parser.Read();
            }

            return -1;
        }

        /// <summary>
        /// Expects a mapping. Will consume the full mapping to find a GUID.
        /// </summary>
        public static string ReadAssetGUID(this ref YamlParser parser)
        {
            if (parser.IsAt(ParseEventType.MappingStart))
            {
                parser.Read();
            
                while (!parser.IsAt(ParseEventType.MappingEnd))
                {
                    var key = parser.ReadScalarAsString();
                    if (parser.IsAt(ParseEventType.MappingStart))
                    {
                        return parser.ReadAssetGUID();
                    }
                    else if (parser.IsAt(ParseEventType.Scalar))
                    {
                        var value = parser.ReadScalarAsString();
                        if (key == "guid" && !AssetParasite.Config.ExcludedAssets.Contains(value))
                        {
                            return value;
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the next scalar to determine if it is of interest. Only reads scalar + value if there is a match.
        /// </summary>
        public static bool TryGetSpecificScalar(this ref YamlParser parser, string desiredKey, out string value)
        {
            var scalarKey = parser.GetScalarAsString();
            if (scalarKey == desiredKey)
            {
                parser.SkipCurrentNode();
                if (parser.IsAt(ParseEventType.Scalar))
                {
                    var scalarValue = parser.ReadScalarAsString();
                    value = scalarValue;
                    return true;
                }
            }

            value = null;
            return false;
        }

        /// <summary>
        /// Where's my cursor at?
        /// </summary>
        public static bool IsAt(this ref YamlParser parser, ParseEventType type)
        {
            return parser.CurrentEventType == type;
        }
    
        public static void PrintBlock(byte[] bytes, int idx, int offset)
        {
            var caughtBytes = new byte[Math.Abs(offset) + 1];
            Buffer.BlockCopy(bytes, offset > 0 ? idx : idx - offset, caughtBytes, 0, Math.Abs(offset));
            Console.WriteLine($"[{Encoding.UTF8.GetString(caughtBytes)}]");
        }
    }
}