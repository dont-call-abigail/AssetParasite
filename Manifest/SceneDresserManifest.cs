using System.Text.Json.Serialization;

namespace Manifest;

public sealed class SceneDresserManifest
{
    public struct ManifestAssetPath
    {
        public string SceneName;
        public string BaseGameObject;
        public string TransformPath;
        public ManifestComponentData Component;
    }

    public struct ManifestComponentData
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ScriptGuid;
        public string ComponentType;
        public string MemberName;
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CollectionIndex;
    }

    public Dictionary<string, List<ManifestAssetPath>> Assets = new();
}