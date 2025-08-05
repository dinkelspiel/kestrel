namespace Kestrel.Framework.Utils;

public static class Paths
{
    public static string InAssets(string relativePath) =>
        Path.Combine(AppContext.BaseDirectory, "../../../../assets", relativePath);
}