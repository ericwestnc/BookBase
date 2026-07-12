namespace Microsoft.Maui.Controls
{
    public class Shell
    {
        public static Shell Current { get; set; } = new();

        public Task GoToAsync(string route) => Task.CompletedTask;
    }
}

namespace Microsoft.Maui.Storage
{
    public static class FileSystem
    {
        public static string AppDataDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "BookBase.Tests");
    }
}
