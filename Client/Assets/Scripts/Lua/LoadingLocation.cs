public enum ELoadingLocation
{
    Package,
    Cache,
}

public struct LoadingLocation
{

    public ELoadingLocation location;
    public string path;

}
