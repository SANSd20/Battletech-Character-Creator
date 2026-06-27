using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using BattletechCharacterCreator.Core.LifePath;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;

namespace BattletechCharacterCreator.App;

public partial class App : Application
{
    public App()
    {
        DispatcherUnhandledException += (_, e) =>
        {
            var reportPath = AppErrorReporter.WriteReport(
                e.Exception,
                "Unhandled UI exception");
            MessageBox.Show(
                $"Something went wrong, but a report was saved here:\n\n{reportPath}",
                "A Time of War application error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            e.Handled = true;
        };
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception exception)
            {
                AppErrorReporter.WriteReport(
                    exception,
                    "Unhandled application exception");
            }
        };
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            AppErrorReporter.WriteReport(
                e.Exception,
                "Unobserved background task exception");
            e.SetObserved();
        };
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var errorReportArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--smoke-error-report=",
                StringComparison.Ordinal));
        if (errorReportArgument is not null)
        {
            var outputPath = Path.GetFullPath(
                errorReportArgument["--smoke-error-report=".Length..]);
            AppErrorReporter.WriteReport(
                new InvalidOperationException("Smoke error report"),
                "Smoke test",
                outputPath);
            ShutdownSmoke(0);
            return;
        }

        var operationErrorArgument = e.Args.FirstOrDefault(
            argument => argument.StartsWith("--smoke-operation-error-report=",
                StringComparison.Ordinal));
        if (operationErrorArgument is not null)
        {
            var outputPath = Path.GetFullPath(
                operationErrorArgument[
                    "--smoke-operation-error-report=".Length..]);
            BattletechCharacterCreator.App.MainWindow
                .SmokeOperationErrorReport(outputPath);
            ShutdownSmoke(0);
            return;
        }

        if (e.Args.Contains("--smoke-wizard", StringComparer.Ordinal))
        {
            try
            {
                SmokeWizardHeadless();
                ShutdownSmoke(0);
            }
            catch (Exception exception)
            {
                AppErrorReporter.WriteReport(exception, "Wizard smoke");
                ShutdownSmoke(1);
            }
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
                    ShutdownSmoke(0);
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
                    ShutdownSmoke(0);
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
                        ShutdownSmoke(0);
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
                        ShutdownSmoke(0);
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(errorPath, ex.ToString());
                        wizard.Close();
                        ShutdownSmoke(1);
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
                    MainWindow? editor = null;
                    try
                    {
                        wizard.SmokeHomeworldClanCharacter();
                        editor = new MainWindow(wizard.CreatedCharacter!);
                        editor.SmokeCampaignYearEraSelection();
                        editor.SmokeXpAllocation();
                        editor.Close();
                        wizard.Close();
                        ShutdownSmoke(0);
                    }
                    catch (Exception exception)
                    {
                        AppErrorReporter.WriteReport(
                            exception,
                            "Editor allocation smoke");
                        editor?.Close();
                        wizard.Close();
                        ShutdownSmoke(1);
                    }
                });
            wizard.Show();
            return;
        }

        if (e.Args.Contains("--smoke-inventory", StringComparer.Ordinal))
        {
            try
            {
                SmokeInventoryCatalogHeadless();
                ShutdownSmoke(0);
            }
            catch (Exception exception)
            {
                var reportPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "inventory-smoke-error-report.txt");
                AppErrorReporter.WriteReport(
                    exception,
                    "Inventory smoke",
                    reportPath);
                ShutdownSmoke(1);
            }
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
                        ShutdownSmoke(0);
                    }
                    catch (Exception ex)
                    {
                        File.WriteAllText(outputPath + ".error.txt", ex.ToString());
                        wizard.Close();
                        ShutdownSmoke(1);
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
                                        ShutdownSmoke(0);
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
                    ShutdownSmoke(0);
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
                    ShutdownSmoke(0);
                });
            wizard.Show();
            return;
        }

        new StartWindow().Show();
    }

    private void ShutdownSmoke(int exitCode)
    {
        Environment.ExitCode = exitCode;
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000).ConfigureAwait(false);
            Environment.Exit(exitCode);
        });

        foreach (var window in Windows.Cast<Window>().ToArray())
        {
            window.Close();
        }
        Current.Shutdown(exitCode);
    }

    private static void SmokeInventoryCatalogHeadless()
    {
        var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        var catalog = ResourceCatalog.Load(resourcePath);
        if (catalog.Options.IncludeCompanion)
        {
            throw new InvalidOperationException(
                "Companion catalog content must be disabled by default.");
        }

        var flak = catalog.Equipment.Single(item => item.Name == "Flak/Jacket");
        var katana = catalog.Weapons.Single(item => item.Name == "Katana");
        var character = new Character();
        character.Equipment.Add(new EquipmentItem
        {
            Name = flak.Name,
            Cost = flak.Cost,
            Mass = flak.Mass,
            Locations = flak.Locations,
            Armor = flak.Armor,
            Notes = flak.Notes,
            Count = "2"
        });
        character.Weapons.Add(new WeaponItem
        {
            Skill = katana.Skill,
            Name = katana.Name,
            Damage = katana.Damage,
            Range = katana.Range,
            Cost = katana.Cost,
            Mass = katana.Mass,
            Shots = katana.Shots,
            AmmoCost = katana.AmmoCost,
            AmmoMass = katana.AmmoMass,
            Notes = katana.Notes,
            Count = "3"
        });
        var summary = CharacterRules.Calculate(character);
        if (character.Equipment.Single().Armor != "1/5/1/3" ||
            character.Weapons.Single().Skill != "Melee Weapons" ||
            summary.InventoryMass <= 0)
        {
            throw new InvalidOperationException(
                "Catalog equipment and weapon fields were not copied correctly.");
        }

        character.Equipment.Clear();
        character.Weapons.Clear();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Heavy smoke item",
            Cost = "0",
            Mass = "999",
            Count = "1"
        });
        if (CharacterRules.Calculate(character).RemainingCapacity >= 0)
        {
            throw new InvalidOperationException(
                "Inventory carrying capacity warning was not detectable.");
        }

        var companionCatalog = ResourceCatalog.Load(
            resourcePath,
            new ResourceCatalogOptions(IncludeCompanion: true));
        if (!companionCatalog.Options.IncludeCompanion ||
            companionCatalog.Equipment.All(item =>
                item.Name != "Vintage Bulletproof Vest") ||
            companionCatalog.Weapons.All(item => item.Name != "Shock Staff") ||
            catalog.Equipment.Any(item => item.Source == RulebookSource.Companion) ||
            catalog.Weapons.Any(item => item.Source == RulebookSource.Companion))
        {
            throw new InvalidOperationException(
                "Companion catalog opt-in filtering did not work.");
        }

        character = new Character();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Wildcard smoke item",
            Cost = "*",
            Mass = "0",
            Count = "2"
        });
        if (CharacterRules.Calculate(character).UnresolvedInventoryPrices != 2)
        {
            throw new InvalidOperationException(
                "Wildcard inventory pricing was not counted.");
        }

        character.Equipment.Clear();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Patch warning armor",
            Cost = "500",
            Mass = "2",
            PatchCount = "2"
        });
        if (CharacterRules.PatchPurchasesNeedingPrice(character) != 2)
        {
            throw new InvalidOperationException(
                "Patch pricing warnings were not detected.");
        }
        character.Equipment.Clear();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Priced patch armor",
            Cost = "500/100",
            Mass = "2",
            PatchCount = "2"
        });
        if (CharacterRules.PatchPurchasesNeedingPrice(character) != 0)
        {
            throw new InvalidOperationException(
                "Patch pricing warnings did not clear with a patch price.");
        }

        character.Weapons.Add(new WeaponItem
        {
            Name = "Ammo detail warning weapon",
            Cost = "100",
            Mass = "1",
            AmmoCost = "",
            AmmoMass = "0.1",
            AmmoCount = "2"
        });
        if (CharacterRules.AmmoPurchasesNeedingDetails(character) != 2)
        {
            throw new InvalidOperationException(
                "Ammo detail warnings were not detected.");
        }
        character.Weapons.Clear();
        character.Weapons.Add(new WeaponItem
        {
            Name = "Power-pack weapon",
            Cost = "100",
            Mass = "1",
            Shots = "5 PPS",
            AmmoCost = "5",
            AmmoMass = "0.1",
            AmmoCount = "2"
        });
        if (CharacterRules.AmmoPurchasesNeedingReloadReview(character) != 2)
        {
            throw new InvalidOperationException(
                "Ammo reload review warnings were not detected.");
        }
        character.Weapons.Clear();
        character.Weapons.Add(new WeaponItem
        {
            Name = "Numeric ammo weapon",
            Cost = "100",
            Mass = "1",
            Shots = "10",
            AmmoCost = "5",
            AmmoMass = "0.1",
            AmmoCount = "2"
        });
        if (CharacterRules.AmmoPurchasesNeedingDetails(character) != 0 ||
            CharacterRules.AmmoPurchasesNeedingReloadReview(character) != 0)
        {
            throw new InvalidOperationException(
                "Ammo warnings did not clear with complete ammo details.");
        }

        character.Equipment.Clear();
        character.Weapons.Clear();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Prosthetic Enhancement - Vibroblade",
            Cost = "1000",
            Mass = "0",
            Locations = "Prosthetic"
        });
        if (CharacterRules.UnmountedProstheticEnhancements(character) != 1)
        {
            throw new InvalidOperationException(
                "Unmounted prosthetic enhancement warnings were not detected.");
        }
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Gill Implant",
            Cost = "8000",
            Mass = "0",
            Locations = "Implant"
        });
        if (CharacterRules.UnmountedProstheticEnhancements(character) != 0)
        {
            throw new InvalidOperationException(
                "Prosthetic enhancement warnings did not clear with an implant host.");
        }

        character.Equipment.Clear();
        character.Equipment.Add(new EquipmentItem
        {
            Name = "Hoodling Sensor HoverJeep",
            Cost = "92000",
            Mass = "0",
            Locations = "Vehicle"
        });
        if (CharacterRules.UnbackedVehiclePurchases(character) != 1)
        {
            throw new InvalidOperationException(
                "Vehicle trait support warnings were not detected.");
        }
        character.Traits.Add(new NamedValue("Vehicle", 100));
        if (CharacterRules.UnbackedVehiclePurchases(character) != 0)
        {
            throw new InvalidOperationException(
                "Vehicle trait support warnings did not clear with the Vehicle trait.");
        }
    }

    private static void SmokeWizardHeadless()
    {
        if (EraAvailabilityCatalog.EarliestAffiliationYear("invading-clan") is not { } clanYear ||
            clanYear > 3052)
        {
            throw new InvalidOperationException(
                "Wizard smoke could not verify Clan Invasion affiliation availability.");
        }
        var rasalhague = LifePathCatalog.Affiliations
            .First(module => module.Id == "rasalhague");
        var rasalhagueIn3052 = EraAvailabilityCatalog.FilterSubAffiliations(
            rasalhague.Id, rasalhague.SubAffiliations ?? [], 3052);
        var rasalhagueIn3062 = EraAvailabilityCatalog.FilterSubAffiliations(
            rasalhague.Id, rasalhague.SubAffiliations ?? [], 3062);
        if (rasalhagueIn3052.Any(module =>
                module.Name == "Ghost Bear Dominion") ||
            !rasalhagueIn3062.Any(module =>
                module.Name == "Ghost Bear Dominion"))
        {
            throw new InvalidOperationException(
                "Wizard smoke could not verify era-aware Rasalhague sub-affiliation availability.");
        }

        const string language = "Language/English";
        var affiliation = LifePathCatalog.Affiliations
            .First(module => module.Id == "lyran");
        var childhood = LifePathCatalog.Childhoods
            .First(module => module.Id == "street");
        var lateChildhood = LifePathCatalog.LateChildhoods
            .First(module => module.Id == "late-street");
        var career = LifePathCatalog.RealLifeModules
            .First(module => module.Id == "real-agitator");

        var character = LifePathEngine.CreateBase("Wizard Smoke", language);
        character.Affiliation = affiliation.Name;
        character.EarlyChildhood = childhood.Name;
        character.LateChildhood = lateChildhood.Name;
        character.RealLife = career.Name;
        character.BirthYear = 3026;
        character.GameYear = 3052;

        var modules = new[] { affiliation, childhood, lateChildhood, career };
        LifePathEngine.Apply(character, CreateDefaultSelection(affiliation));
        LifePathEngine.Apply(character, CreateDefaultSelection(childhood));
        LifePathEngine.Apply(character, CreateDefaultSelection(lateChildhood));
        LifePathEngine.ApplyStage4(character, CreateDefaultSelection(career));
        LifePathEngine.ApplyAffiliationContext(
            character, affiliation, childhood, language);
        LifePathEngine.ApplyAffiliationContext(
            character, affiliation, lateChildhood, language);
        LifePathEngine.ApplyAffiliationContext(
            character, affiliation, career, language);
        LifePathEngine.ApplyModuleAccounting(character, modules);

        if (character.Age != 26 ||
            character.Affiliation != "Lyran Alliance" ||
            character.RealLifeHistory.LastOrDefault() != "Agitator")
        {
            throw new InvalidOperationException(
                "Wizard smoke representative character did not preserve its basic life path.");
        }
        if (!character.Skills.Any(item =>
                item.Name == "Protocol/Lyran" && item.Value != 0) ||
            !character.Skills.Any(item =>
                item.Name == "Leadership" && item.Value != 0))
        {
            throw new InvalidOperationException(
                "Wizard smoke representative character did not apply expected effects.");
        }
        if (PrerequisiteRules.Evaluate(character)
            .Any(issue => issue.Category == "Affiliation"))
        {
            throw new InvalidOperationException(
                "Wizard smoke representative character has an affiliation conflict.");
        }
    }

    private static ModuleSelection CreateDefaultSelection(LifePathModule module)
    {
        var choices = module.Choices.ToDictionary(
            choice => choice.Id,
            choice =>
                (IReadOnlyList<string>)choice.Options
                    .DefaultIfEmpty("Perception")
                    .Take(choice.Count)
                    .ToArray());
        return new ModuleSelection(module, choices);
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
