namespace AssetParasite;

public class ComponentNode
{
    public long AssetGuid;
    public string MemberName;
    public string ComponentType;

    public override bool Equals(object? obj)
    {
        return obj is ComponentNode other && other.AssetGuid == AssetGuid;
    }
}