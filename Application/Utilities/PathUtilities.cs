namespace Application.Utilities;

public static class PathUtilities
{
    public const string Separator = "/";

    public static string Combine(params string[] paths)
    {
        return string.Join(Separator, paths);
    }

    public static string AddIdToPath(string path, string id)
    {
        return path + Separator + id;
    }
}