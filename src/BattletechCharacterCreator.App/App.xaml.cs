using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace BattletechCharacterCreator.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Contains("--smoke-wizard", StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    wizard.SmokeAllSelections();
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-start", StringComparer.Ordinal))
        {
            var start = new StartWindow();
            start.Loaded += (_, _) => start.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    start.Close();
                    Shutdown(0);
                });
            start.Show();
            return;
        }

        var captureArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--capture-start=", StringComparison.Ordinal));
        if (captureArgument is not null)
        {
            var outputPath = Path.GetFullPath(captureArgument["--capture-start=".Length..]);
            var start = new StartWindow();
            start.Loaded += (_, _) => start.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    CaptureWindow(start, outputPath);
                    start.Close();
                    Shutdown(0);
                });
            start.Show();
            return;
        }

        var wizardCaptureArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--capture-wizard=", StringComparison.Ordinal));
        if (wizardCaptureArgument is not null)
        {
            var outputPath = Path.GetFullPath(
                wizardCaptureArgument["--capture-wizard=".Length..]);
            var wizardStepArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-wizard-step=",
                    StringComparison.Ordinal));
            var wizardStep = wizardStepArgument is not null &&
                int.TryParse(wizardStepArgument["--capture-wizard-step=".Length..],
                    out var requestedStep)
                    ? requestedStep
                    : 0;
            var wizardAffiliationArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-affiliation=",
                    StringComparison.Ordinal));
            var wizardAffiliation = wizardAffiliationArgument?[
                "--capture-affiliation=".Length..];
            var lateChildhoodArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-late-childhood=",
                    StringComparison.Ordinal));
            var lateChildhood = lateChildhoodArgument?[
                "--capture-late-childhood=".Length..];
            var educationArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-education=",
                    StringComparison.Ordinal));
            var education = educationArgument?["--capture-education=".Length..];
            var advancedFieldArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-advanced-field=",
                    StringComparison.Ordinal));
            var advancedField = advancedFieldArgument?[
                "--capture-advanced-field=".Length..];
            var thirdFieldArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-third-field=",
                    StringComparison.Ordinal));
            var thirdField = thirdFieldArgument?[
                "--capture-third-field=".Length..];
            var firstCareerArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-first-career=",
                    StringComparison.Ordinal));
            var firstCareer = firstCareerArgument?[
                "--capture-first-career=".Length..];
            var secondCareerArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-second-career=",
                    StringComparison.Ordinal));
            var secondCareer = secondCareerArgument?[
                "--capture-second-career=".Length..];
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    if (!string.IsNullOrWhiteSpace(wizardAffiliation))
                    {
                        wizard.SelectAffiliationForCapture(wizardAffiliation);
                    }
                    if (!string.IsNullOrWhiteSpace(lateChildhood))
                    {
                        wizard.SelectLateChildhoodForCapture(lateChildhood);
                    }
                    if (!string.IsNullOrWhiteSpace(education))
                    {
                        wizard.SelectEducationForCapture(
                            education, advancedField, thirdField);
                    }
                    if (!string.IsNullOrWhiteSpace(firstCareer))
                    {
                        wizard.SelectCareersForCapture(firstCareer, secondCareer);
                    }
                    wizard.ShowStepForCapture(wizardStep);
                    CaptureWindow(wizard, outputPath);
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        new StartWindow().Show();
    }

    private static void CaptureWindow(Window window, string outputPath)
    {
        window.UpdateLayout();
        var dpi = VisualTreeHelper.GetDpi(window);
        var bitmap = new RenderTargetBitmap(
            (int)Math.Ceiling(window.ActualWidth * dpi.DpiScaleX),
            (int)Math.Ceiling(window.ActualHeight * dpi.DpiScaleY),
            dpi.PixelsPerInchX,
            dpi.PixelsPerInchY,
            PixelFormats.Pbgra32);
        bitmap.Render(window);

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var output = File.Create(outputPath);
        encoder.Save(output);
    }
}
