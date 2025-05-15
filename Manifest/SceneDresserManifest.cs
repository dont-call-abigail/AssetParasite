namespace Manifest;

public sealed class SceneDresserManifest
{
    public struct ManifestEntry
    {
        public string SceneName;
        public ManifestAssetPath[] Assets = [];

        public ManifestEntry()
        {
        }
    }

    public struct ManifestAssetPath
    {
        public string BaseGameObject;
        public string TransformPath;
        public ManifestComponentData Component;
    }

    public struct ManifestComponentData
    {
        public string ScriptGuid;
        public string MemberName;
        public int CollectionIndex;
    }

    public Dictionary<string, ManifestEntry> Assets = new();
}