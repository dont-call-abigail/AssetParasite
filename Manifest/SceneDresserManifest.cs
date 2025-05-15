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
        public string ScriptGuid;
        public string ComponentType;
        public string MemberName;
        public int CollectionIndex;
    }

    public Dictionary<string, List<ManifestAssetPath>> Assets = new();
}