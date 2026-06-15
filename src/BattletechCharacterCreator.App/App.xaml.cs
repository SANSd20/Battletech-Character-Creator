using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BattletechCharacterCreator.Core.Rules;

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
                    wizard.SmokeCreateCharacter();
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-clan", StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    wizard.SmokeInvadingClanCharacter();
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-homeworld-clan", StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    wizard.SmokeHomeworldClanCharacter();
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-clan-roundtrip", StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    var path = Path.Combine(
                        Path.GetTempPath(), $"atow-clan-{Guid.NewGuid():N}.btcc");
                    try
                    {
                        wizard.SmokeHomeworldClanCharacter();
                        var editor = new MainWindow(wizard.CreatedCharacter!);
                        editor.SmokeSaveAndReload(path);
                        editor.Close();
                        wizard.Close();
                        Shutdown(0);
                    }
                    finally
                    {
                        File.Delete(path);
                    }
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-complete-life-paths",
                StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    var paths = new List<string>();
                    var errorPath = Path.Combine(
                        Path.GetTempPath(),
                        "atow-complete-life-paths.error.txt");
                    try
                    {
                        File.Delete(errorPath);
                        foreach (var character in
                                 wizard.SmokeRepresentativeLifePaths())
                        {
                            var path = Path.Combine(
                                Path.GetTempPath(),
                                $"atow-life-path-{Guid.NewGuid():N}.btcc");
                            paths.Add(path);
                            BattletechCharacterCreator.Core.Persistence
                                .LegacyCharacterSerializer.Save(character, path);
                            var loaded = BattletechCharacterCreator.Core.Persistence
                                .LegacyCharacterSerializer.Load(path);
                            if (loaded.Affiliation != character.Affiliation ||
                                !loaded.RealLifeHistory.SequenceEqual(
                                    character.RealLifeHistory) ||
                                CharacterRules.Calculate(loaded).FreeXp !=
                                CharacterRules.Calculate(character).FreeXp)
                            {
                                throw new InvalidOperationException(
                                    $"{character.Name} did not round-trip correctly.");
                            }
                        }
                        wizard.Close();
                        Shutdown(0);
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(errorPath, ex.ToString());
                        wizard.Close();
                        Shutdown(1);
                    }
                    finally
                    {
                        foreach (var path in paths)
                        {
                            File.Delete(path);
                        }
                    }
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-editor-allocation", StringComparer.Ordinal))
        {
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    wizard.SmokeHomeworldClanCharacter();
                    var editor = new MainWindow(wizard.CreatedCharacter!);
                    editor.SmokeXpAllocation();
                    editor.Close();
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-inventory", StringComparer.Ordinal))
        {
            var editor = new MainWindow(
                new BattletechCharacterCreator.Core.Models.Character());
            editor.Loaded += (_, _) => editor.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    try
                    {
                        editor.SmokeInventoryCatalog();
                        editor.Close();
                        Shutdown(0);
                    }
                    catch
                    {
                        editor.Close();
                        Shutdown(1);
                    }
                });
            editor.Show();
            return;
        }

        var sheetExportArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--smoke-sheet-export=",
                StringComparison.Ordinal));
        if (sheetExportArgument is not null)
        {
            var outputPath = Path.GetFullPath(
                sheetExportArgument["--smoke-sheet-export=".Length..]);
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    try
                    {
                        wizard.SmokeHomeworldClanCharacter();
                        var editor = new MainWindow(wizard.CreatedCharacter!);
                        editor.SmokeExportCharacterSheet(outputPath);
                        editor.Close();
                        wizard.Close();
                        Shutdown(0);
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(outputPath + ".error.txt", ex.ToString());
                        wizard.Close();
                        Shutdown(1);
                    }
                });
            wizard.Show();
            return;
        }

        var editorCaptureArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--capture-clan-editor=",
                StringComparison.Ordinal));
        if (editorCaptureArgument is not null)
        {
            var outputPath = Path.GetFullPath(
                editorCaptureArgument["--capture-clan-editor=".Length..]);
            var editorTabArgument = e.Args.FirstOrDefault(
                argument => argument.StartsWith("--capture-editor-tab=",
                    StringComparison.Ordinal));
            var editorTab = editorTabArgument?[
                "--capture-editor-tab=".Length..];
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    var characterPath = Path.Combine(
                        Path.GetTempPath(), $"atow-clan-{Guid.NewGuid():N}.btcc");
                    try
                    {
                        wizard.SmokeHomeworldClanCharacter();
                        var editor = new MainWindow(wizard.CreatedCharacter!);
                        editor.Loaded += (_, _) => editor.Dispatcher.BeginInvoke(
                            DispatcherPriority.ApplicationIdle,
                            () =>
                            {
                                editor.SmokeSaveAndReload(characterPath);
                                if (!string.IsNullOrWhiteSpace(editorTab))
                                {
                                    editor.SelectTabForCapture(editorTab);
                                }
                                editor.Dispatcher.BeginInvoke(
                                    DispatcherPriority.Render,
                                    () =>
                                    {
                                        editor.UpdateLayout();
                                        CaptureWindow(editor, outputPath);
                                        editor.Close();
                                        wizard.Close();
                                        File.Delete(characterPath);
                                        Shutdown(0);
                                    });
                            });
                        editor.Show();
                        wizard.Hide();
                    }
                    catch
                    {
                        File.Delete(characterPath);
                        throw;
                    }
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
            var clanTest = e.Args.Contains(
                "--capture-clan-test", StringComparer.Ordinal);
            var homeworldClanTest = e.Args.Contains(
                "--capture-homeworld-clan-test", StringComparer.Ordinal);
            var wizard = new CharacterWizardWindow();
            wizard.Loaded += (_, _) => wizard.Dispatcher.BeginInvoke(
                DispatcherPriority.ApplicationIdle,
                () =>
                {
                    if (clanTest)
                    {
                        wizard.SelectInvadingClanTestPath();
                    }
                    if (homeworldClanTest)
                    {
                        wizard.SelectHomeworldClanTestPath();
                    }
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
