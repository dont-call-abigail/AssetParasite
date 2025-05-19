using System.Text.Json.Serialization;

namespace Schema;

[Serializable]
public sealed class SceneDresserManifest
{
    [Serializable]
    public sealed class ManifestAssetPath
    {
        public string SceneName { get; set; }
        public string BaseGameObject { get; set; }
        public string TransformPath { get; set; }
        public ManifestComponentData Component { get; set; }
    }

    [Serializable]
    public sealed class ManifestComponentData
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string ScriptGuid { get; set; }
        public string ComponentType { get; set; }
        public string MemberName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public int CollectionIndex { get; set; }
    }

    public Dictionary<string, List<ManifestAssetPath>> Assets { get; set; } = new();
}