using SharpEngine.Core.Manager;

namespace SharpEngine.SQLite;

/// <summary>
/// Static class with extensions and add version functions
/// </summary>
public static class SESQLite
{
    /// <summary>
    /// Add versions to DebugManager
    /// </summary>
    public static void AddVersions()
    {
        DebugManager.Versions.Add("System.Data.SQLite", "2.0.3");
        DebugManager.Versions.Add("SharpEngine.SQLite", "2.1.0 ");
    }
}
