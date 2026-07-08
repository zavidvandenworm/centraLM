namespace Application.Utilities;

public static class PathUtilities
{
    private const string PathSeparator = "/";

    public static string Combine(params string[] paths)
    {
        return string.Join(PathSeparator, paths);
    }

    public static string AddIdToPath(string path, string id)
    {
        return path + PathSeparator + id;
    }
}