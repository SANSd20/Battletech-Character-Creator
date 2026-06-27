using System.IO;
using System.Windows;

namespace BattletechCharacterCreator.App;

public static class Program
{
    [STAThread]
    public static int Main(string[] args)
    {
        if (args.Contains("--smoke-start", StringComparer.Ordinal))
        {
            try
            {
                SmokeStartWindowHeadless();
                return 0;
            }
            catch (Exception exception)
            {
                var reportPath = Path.Combine(
                    Environment.GetEnvironmentVariable("APPDATA") ??
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "start-smoke-error-report.txt");
                AppErrorReporter.WriteReport(
                    exception,
                    "Start window smoke",
                    reportPath);
                return 1;
            }
        }

        var app = new App();
        app.InitializeComponent();
        app.Run();
        return Environment.ExitCode;
    }

    private static void SmokeStartWindowHeadless()
    {
        var xamlPath = Path.Combine(AppContext.BaseDirectory, "StartWindow.xaml");
        if (!File.Exists(xamlPath))
        {
            xamlPath = Path.GetFullPath(
                Path.Combine(AppContext.BaseDirectory,
                    "..", "..", "..", "StartWindow.xaml"));
        }
        if (!File.Exists(xamlPath))
        {
            throw new FileNotFoundException(
                "Start window XAML could not be found.",
                xamlPath);
        }

        var xaml = File.ReadAllText(xamlPath);
        if (!xaml.Contains("A TIME OF WAR", StringComparison.Ordinal) ||
            !xaml.Contains("Character Wizard", StringComparison.Ordinal) ||
            !xaml.Contains("Character Editor", StringComparison.Ordinal) ||
            !xaml.Contains("Wizard_Click", StringComparison.Ordinal) ||
            !xaml.Contains("Editor_Click", StringComparison.Ordinal) ||
            !xaml.Contains("Assets/Images/pilot6.jpg", StringComparison.Ordinal) ||
            !xaml.Contains("Assets/Fonts/#Montserrat", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Start window XAML is missing expected title, choices, handlers, or assets.");
        }

        var assetRoot = AppContext.BaseDirectory;
        var pilotPath = Path.Combine(assetRoot, "Assets", "Images", "pilot6.jpg");
        var fontPath = Path.Combine(assetRoot, "Assets", "Fonts", "Montserrat-Black.ttf");
        var projectRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
        if (!File.Exists(pilotPath))
        {
            pilotPath = Path.Combine(projectRoot, "Assets", "Images", "pilot6.jpg");
        }
        if (!File.Exists(fontPath))
        {
            fontPath = Path.Combine(projectRoot, "Assets", "Fonts", "Montserrat-Black.ttf");
        }
        if (!File.Exists(pilotPath))
        {
            throw new FileNotFoundException(
                "Start window pilot image asset could not be found.",
                pilotPath);
        }
        if (!File.Exists(fontPath))
        {
            throw new FileNotFoundException(
                "Start window Montserrat font asset could not be found.",
                fontPath);
        }
    }
}
