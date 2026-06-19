using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace BattletechCharacterCreator.App;

public static class AppErrorReporter
{
    public static string WriteReport(
        Exception exception,
        string context,
        string? path = null)
    {
        var reportPath = path ?? CreateDefaultReportPath();
        try
        {
            return WriteReportFile(reportPath, exception, context);
        }
        catch when (path is null)
        {
            var fallbackPath = Path.Combine(
                Path.GetTempPath(),
                "A-Time-of-War-Character-Creator",
                "Error Reports",
                Path.GetFileName(reportPath));
            return WriteReportFile(fallbackPath, exception, context);
        }
    }

    private static string WriteReportFile(
        string path,
        Exception exception,
        string context)
    {
        Directory.CreateDirectory(
            Path.GetDirectoryName(Path.GetFullPath(path))!);
        File.WriteAllText(path, BuildReport(exception, context), Encoding.UTF8);
        return path;
    }

    private static string CreateDefaultReportPath()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "A Time of War Character Creator",
            "Error Reports");
        var fileName = $"error-{DateTimeOffset.Now:yyyyMMdd-HHmmss}.txt";
        return Path.Combine(directory, fileName);
    }

    private static string BuildReport(Exception exception, string context)
    {
        var builder = new StringBuilder();
        builder.AppendLine("A Time of War Character Creator Error Report");
        builder.AppendLine($"Created: {DateTimeOffset.Now:O}");
        builder.AppendLine($"Context: {context}");
        builder.AppendLine($"Version: {ApplicationVersion()}");
        builder.AppendLine($"Executable: {Environment.ProcessPath ?? "Unknown"}");
        builder.AppendLine($"Working directory: {Environment.CurrentDirectory}");
        builder.AppendLine($"Command line: {Environment.CommandLine}");
        builder.AppendLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        builder.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        builder.AppendLine($".NET: {RuntimeInformation.FrameworkDescription}");
        builder.AppendLine();
        builder.AppendLine(exception.ToString());
        return builder.ToString();
    }

    private static string ApplicationVersion()
    {
        var assembly = Assembly.GetEntryAssembly() ?? typeof(AppErrorReporter).Assembly;
        return assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion ??
            assembly.GetName().Version?.ToString() ??
            "Unknown";
    }
}
