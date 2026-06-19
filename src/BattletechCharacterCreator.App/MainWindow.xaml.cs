using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Persistence;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;
using Microsoft.Win32;

namespace BattletechCharacterCreator.App;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private Character character = new();
    private string? currentPath;
    private CharacterSummary summary;
    private IReadOnlyList<PrerequisiteIssue> prerequisiteIssues = [];
    private string ruleStatus = "";
    private Brush ruleStatusBrush = Brushes.DarkGreen;
    private string skillFilter = "";
    private string traitFilter = "";
    private string equipmentCatalogFilter = "";
    private string weaponCatalogFilter = "";
    private string? selectedSkillName;
    private string? selectedTraitName;
    private readonly string resourcePath;
    private bool includeCompanionContent;
    private EquipmentCatalogItem? selectedEquipmentCatalogItem;
    private WeaponCatalogItem? selectedWeaponCatalogItem;

    public ObservableCollection<XpEditorRow> AttributeRows { get; } = [];
    public ObservableCollection<XpEditorRow> SkillRows { get; } = [];
    public ObservableCollection<XpEditorRow> TraitRows { get; } = [];
    public ICollectionView SkillRowsView { get; }
    public ICollectionView TraitRowsView { get; }
    public ICollectionView EquipmentCatalogView { get; private set; } =
        CollectionViewSource.GetDefaultView(Array.Empty<EquipmentCatalogItem>());
    public ICollectionView WeaponCatalogView { get; private set; } =
        CollectionViewSource.GetDefaultView(Array.Empty<WeaponCatalogItem>());

    public MainWindow() : this(new Character())
    {
    }

    public MainWindow(Character initialCharacter)
    {
        character = initialCharacter;
        resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        Catalog = ResourceCatalog.Load(resourcePath);
        RebuildCatalogViews();
        summary = CharacterRules.Calculate(character);
        SkillRowsView = CollectionViewSource.GetDefaultView(SkillRows);
        SkillRowsView.Filter = MatchesSkillFilter;
        TraitRowsView = CollectionViewSource.GetDefaultView(TraitRows);
        TraitRowsView.Filter = MatchesTraitFilter;
        DataContext = this;
        InitializeComponent();
        UpdateFileStatus();
        Recalculate();
    }

    public Character Character
    {
        get => character;
        private set
        {
            character = value;
            OnPropertyChanged();
            OnPropertyChanged(string.Empty);
        }
    }

    public ResourceCatalog Catalog { get; private set; }
    public string[] SexOptions { get; } = ["Male", "Female"];
    public CharacterSummary Summary
    {
        get => summary;
        private set
        {
            summary = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(InventoryStatus));
            OnPropertyChanged(nameof(InventoryStatusBrush));
        }
    }
    public IReadOnlyList<PrerequisiteIssue> PrerequisiteIssues
    {
        get => prerequisiteIssues;
        private set
        {
            prerequisiteIssues = value;
            OnPropertyChanged();
        }
    }
    public string RuleStatus
    {
        get => ruleStatus;
        private set
        {
            ruleStatus = value;
            OnPropertyChanged();
        }
    }
    public Brush RuleStatusBrush
    {
        get => ruleStatusBrush;
        private set
        {
            ruleStatusBrush = value;
            OnPropertyChanged();
        }
    }

    public string CharacterName
    {
        get => Character.Name;
        set
        {
            Character.Name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Character));
        }
    }
    public string SubAffiliation
    {
        get => Character.SubAffiliation;
        set
        {
            Character.SubAffiliation = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Character));
        }
    }
    public string Sex { get => Character.Sex; set => Character.Sex = value; }
    public int BirthYear { get => Character.BirthYear; set => Character.BirthYear = value; }
    public int GameYear { get => Character.GameYear; set => Character.GameYear = value; }
    public int CharacterHeight { get => Character.Height; set => Character.Height = value; }
    public int Weight { get => Character.Weight; set => Character.Weight = value; }
    public string Notes { get => Character.Notes; set => Character.Notes = value; }
    public object Equipment => Character.Equipment;
    public object Weapons => Character.Weapons;
    public string InventoryStatus =>
        Summary.UnresolvedInventoryPrices > 0
            ? $"{Summary.UnresolvedInventoryPrices} inventory price(s) need manual pricing."
            : Summary.RemainingCBills < 0
                ? $"Inventory is over budget by {-Summary.RemainingCBills} C-Bills."
                : Summary.RemainingCapacity < 0
                    ? $"Inventory is over carrying capacity by {-Summary.RemainingCapacity:0.##} kg."
                    : $"Inventory has {Summary.RemainingCBills} C-Bills and {Summary.RemainingCapacity:0.##} kg capacity remaining.";
    public Brush InventoryStatusBrush =>
        Summary.UnresolvedInventoryPrices > 0
            ? Brushes.DarkGoldenrod
            : Summary.RemainingCBills < 0 || Summary.RemainingCapacity < 0
                ? Brushes.Firebrick
                : Brushes.DarkGreen;
    public bool IncludeCompanionContent
    {
        get => includeCompanionContent;
        set
        {
            if (includeCompanionContent == value) return;
            includeCompanionContent = value;
            ReloadCatalog();
            OnPropertyChanged();
        }
    }
    public EquipmentCatalogItem? SelectedEquipmentCatalogItem
    {
        get => selectedEquipmentCatalogItem;
        set
        {
            selectedEquipmentCatalogItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedEquipmentSourceLabel));
            OnPropertyChanged(nameof(SelectedEquipmentSummary));
            OnPropertyChanged(nameof(SelectedEquipmentNotes));
        }
    }
    public string SelectedEquipmentSourceLabel =>
        SelectedEquipmentCatalogItem?.SourceLabel ?? "";
    public string SelectedEquipmentSummary =>
        SelectedEquipmentCatalogItem is { } item
            ? $"{item.Category} | Cost {item.Cost} | Mass {item.Mass} | Armor/Rating {item.Armor}"
            : "";
    public string SelectedEquipmentNotes =>
        PlainCatalogText(SelectedEquipmentCatalogItem?.Notes ?? "");
    public WeaponCatalogItem? SelectedWeaponCatalogItem
    {
        get => selectedWeaponCatalogItem;
        set
        {
            selectedWeaponCatalogItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedWeaponSourceLabel));
            OnPropertyChanged(nameof(SelectedWeaponSummary));
            OnPropertyChanged(nameof(SelectedWeaponNotes));
        }
    }
    public string SelectedWeaponSourceLabel =>
        SelectedWeaponCatalogItem?.SourceLabel ?? "";
    public string SelectedWeaponSummary =>
        SelectedWeaponCatalogItem is { } item
            ? $"{item.Category} | {item.Skill} | Damage {item.Damage} | Range {item.Range} | Cost {item.Cost} | Mass {item.Mass}"
            : "";
    public string SelectedWeaponNotes =>
        SelectedWeaponCatalogItem is { } item
            ? PlainCatalogText(
                $"Shots {item.Shots}; Ammo {item.AmmoCost} C-Bills / {item.AmmoMass} kg. {item.Notes}")
            : "";
    public string? SelectedSkillName
    {
        get => selectedSkillName;
        set
        {
            selectedSkillName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedSkillSourceLabel));
            OnPropertyChanged(nameof(SelectedSkillReference));
            OnPropertyChanged(nameof(SelectedSkillDescription));
        }
    }
    public string SelectedSkillSourceLabel =>
        SelectedSkillCatalogItem?.SourceLabel ?? "";
    public string SelectedSkillReference =>
        SelectedSkillCatalogItem?.Rules ?? "";
    public string SelectedSkillDescription =>
        PlainCatalogText(SelectedSkillCatalogItem?.Description ?? "");
    public string? SelectedTraitName
    {
        get => selectedTraitName;
        set
        {
            selectedTraitName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedTraitSourceLabel));
            OnPropertyChanged(nameof(SelectedTraitReference));
            OnPropertyChanged(nameof(SelectedTraitDescription));
        }
    }
    public string SelectedTraitSourceLabel =>
        SelectedTraitCatalogItem?.SourceLabel ?? "";
    public string SelectedTraitReference =>
        SelectedTraitCatalogItem?.Reference ?? "";
    public string SelectedTraitDescription =>
        PlainCatalogText(SelectedTraitCatalogItem?.Description ?? "");
    public string SkillFilter
    {
        get => skillFilter;
        set
        {
            skillFilter = value ?? "";
            OnPropertyChanged();
            SkillRowsView.Refresh();
        }
    }
    public string TraitFilter
    {
        get => traitFilter;
        set
        {
            traitFilter = value ?? "";
            OnPropertyChanged();
            TraitRowsView.Refresh();
        }
    }
    public string EquipmentCatalogFilter
    {
        get => equipmentCatalogFilter;
        set
        {
            equipmentCatalogFilter = value ?? "";
            OnPropertyChanged();
            EquipmentCatalogView.Refresh();
        }
    }
    public string WeaponCatalogFilter
    {
        get => weaponCatalogFilter;
        set
        {
            weaponCatalogFilter = value ?? "";
            OnPropertyChanged();
            WeaponCatalogView.Refresh();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private SkillCatalogItem? SelectedSkillCatalogItem =>
        SelectedSkillName is { Length: > 0 } name
            ? Catalog.Skills.FirstOrDefault(item => item.Name == name)
            : null;

    private TraitCatalogItem? SelectedTraitCatalogItem =>
        SelectedTraitName is { Length: > 0 } name
            ? Catalog.Traits.FirstOrDefault(item => item.Name == name)
            : null;

    public void SmokeSaveAndReload(string path)
    {
        var original = Character;
        LegacyCharacterSerializer.Save(original, path);
        var loaded = LegacyCharacterSerializer.Load(path);
        VerifyRoundTrip(original, loaded);
        Character = loaded;
        currentPath = path;
        UpdateFileStatus();
        Recalculate();
    }

    public void SmokeXpAllocation()
    {
        var originalFreeXp = Summary.FreeXp;
        var bod = AttributeRows.Single(item => item.Name == "BOD");
        var required = 300 - bod.Xp;
        if (!AdjustXp(bod, required, false))
        {
            throw new InvalidOperationException(
                "Editor XP allocation rejected an affordable Attribute increase.");
        }
        if (Summary.FreeXp != originalFreeXp - 75 ||
            PrerequisiteIssues.Any(issue =>
                issue.Category == "Attribute" && issue.Name == "BOD"))
        {
            throw new InvalidOperationException(
                "Editor XP allocation did not update totals and prerequisites.");
        }

        var beforeRejectedSpend = bod.Xp;
        if (AdjustXp(bod, Summary.FreeXp + 5, false) ||
            bod.Xp != beforeRejectedSpend)
        {
            throw new InvalidOperationException(
                "Editor XP allocation allowed spending beyond Free XP.");
        }

        SkillFilter = "Gunnery";
        if (SkillRowsView.Cast<XpEditorRow>()
            .Any(item => !item.Name.Contains("Gunnery",
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                "Editor Skill filtering returned an unrelated row.");
        }
        SkillFilter = "";
    }

    public void SmokeInventoryCatalog()
    {
        Character = new Character();
        if (IncludeCompanionContent)
        {
            throw new InvalidOperationException(
                "Companion catalog content must be disabled by default.");
        }
        SelectedEquipmentCatalogItem = Catalog.Equipment.Single(item =>
            item.Name == "Flak/Jacket");
        AddEquipment_Click(this, new RoutedEventArgs());
        SelectedWeaponCatalogItem = Catalog.Weapons.Single(item =>
            item.Name == "Katana");
        AddWeapon_Click(this, new RoutedEventArgs());

        var equipment = Character.Equipment.Single();
        var weapon = Character.Weapons.Single();
        equipment.Count = "2";
        weapon.Count = "3";
        Recalculate();

        if (equipment.Armor != "1/5/1/3" ||
            weapon.Skill != "Melee Weapons" ||
            Summary.InventoryMass <= 0)
        {
            throw new InvalidOperationException(
                "Catalog equipment and weapon fields were not copied correctly.");
        }

        EquipmentGrid.SelectedItem = equipment;
        RemoveEquipment_Click(this, new RoutedEventArgs());
        WeaponsGrid.SelectedItem = weapon;
        RemoveWeapon_Click(this, new RoutedEventArgs());
        if (Character.Equipment.Count != 0 || Character.Weapons.Count != 0)
        {
            throw new InvalidOperationException(
                "Inventory remove actions did not remove the selected rows.");
        }

        Character.Equipment.Add(new EquipmentItem
        {
            Name = "Heavy smoke item",
            Cost = "0",
            Mass = "999",
            Count = "1"
        });
        Recalculate();
        if (Summary.RemainingCapacity >= 0 ||
            !InventoryStatus.Contains("over carrying capacity",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Inventory carrying capacity warning was not shown in the editor.");
        }
        Character.Equipment.Clear();
        Recalculate();

        EquipmentCatalogFilter = "Flak";
        if (EquipmentCatalogView.Cast<EquipmentCatalogItem>()
            .Any(item => !MatchesCatalogText(
                EquipmentCatalogFilter,
                item.Name,
                item.Category,
                item.Notes,
                item.SourceLabel)))
        {
            throw new InvalidOperationException(
                "Editor equipment catalog filtering returned an unrelated row.");
        }
        EquipmentCatalogFilter = "";

        IncludeCompanionContent = true;
        if (!Catalog.Options.IncludeCompanion)
        {
            throw new InvalidOperationException(
                "The Companion catalog toggle did not reload catalog options.");
        }
        EquipmentCatalogFilter = "vintage";
        WeaponCatalogFilter = "shock";
        if (!EquipmentCatalogView.Cast<EquipmentCatalogItem>()
                .Any(item => item.Name == "Vintage Bulletproof Vest") ||
            EquipmentCatalogView.Cast<EquipmentCatalogItem>()
                .Any(item => item.Name == "Flak/Jacket") ||
            !WeaponCatalogView.Cast<WeaponCatalogItem>()
                .Any(item => item.Name == "Shock Staff") ||
            WeaponCatalogView.Cast<WeaponCatalogItem>()
                .Any(item => item.Name == "Katana"))
        {
            throw new InvalidOperationException(
                "Editor catalog filters did not narrow equipment and weapon options.");
        }
        EquipmentCatalogFilter = "";
        WeaponCatalogFilter = "";
        SelectedEquipmentCatalogItem = Catalog.Equipment.Single(item =>
            item.Name == "Vintage Bulletproof Vest");
        SelectedWeaponCatalogItem = Catalog.Weapons.Single(item =>
            item.Name == "Shock Staff");
        SelectedTraitName = "Mutation";
        Character.Equipment.Add(new EquipmentItem
        {
            Name = "Wildcard smoke item",
            Cost = "*",
            Mass = "0",
            Count = "2"
        });
        Recalculate();
        if (SelectedEquipmentSourceLabel != "A Time of War Companion" ||
            !SelectedEquipmentSummary.Contains("Cost 500/100", StringComparison.Ordinal) ||
            SelectedEquipmentNotes.Length == 0 ||
            SelectedWeaponSourceLabel != "A Time of War Companion" ||
            !SelectedWeaponSummary.Contains("Damage 2E/6", StringComparison.Ordinal) ||
            !SelectedWeaponNotes.Contains("Shots", StringComparison.Ordinal) ||
            SelectedTraitSourceLabel != "A Time of War Companion" ||
            !SelectedTraitDescription.Contains("genetic conditions",
                StringComparison.Ordinal) ||
            Summary.UnresolvedInventoryPrices != 2 ||
            !InventoryStatus.Contains("2 inventory price(s)",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Companion catalog labels, notes, or inventory warnings were not shown in the editor.");
        }
        IncludeCompanionContent = false;
    }

    public void SelectTabForCapture(string name)
    {
        EditorTabs.SelectedItem = EditorTabs.Items
            .Cast<TabItem>()
            .FirstOrDefault(tab => string.Equals(
                tab.Header?.ToString(), name, StringComparison.OrdinalIgnoreCase));
        ApplyXpGridColumnWidths();
    }

    public void SmokeExportCharacterSheet(string path)
    {
        Character.Equipment.Add(new EquipmentItem
        {
            Name = "Smoke Patch Armor",
            Cost = "500/100",
            Mass = "3.1",
            Armor = "1/4/0/2",
            PatchCount = "2",
            Count = "1"
        });
        Character.Weapons.Add(new WeaponItem
        {
            Skill = "Small Arms",
            Name = "Smoke Ammo Weapon",
            Damage = "2B/3",
            Range = "8/18/40/90",
            Cost = "500",
            Mass = "0.5",
            Shots = "9",
            AmmoCost = "12",
            AmmoMass = "0.06",
            AmmoCount = "3",
            Count = "1"
        });
        CharacterSheetExporter.Export(Character, Catalog, path);
        var bytes = File.ReadAllBytes(path);
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var expectedPages =
            Character.Traits.Count > 8 || Character.Skills.Count > 30 ? 3 : 2;
        if (bytes.Length < 100_000 ||
            !text.StartsWith("%PDF-1.4", StringComparison.Ordinal) ||
            !text.Contains($"/Count {expectedPages}", StringComparison.Ordinal) ||
            !text.Contains(EscapeSmokeText(Character.Name),
                StringComparison.Ordinal) ||
            !text.Contains("Patches: 2", StringComparison.Ordinal) ||
            !text.Contains("Ammo packs: 3", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Character sheet PDF export did not produce a complete document.");
        }
    }

    public static string SmokeOperationErrorReport(string path) =>
        WriteOperationReport(
            new InvalidOperationException("Smoke editor operation error"),
            "Unable to save character",
            path);

    private static string EscapeSmokeText(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("(", "\\(", StringComparison.Ordinal)
            .Replace(")", "\\)", StringComparison.Ordinal);

    private void New_Click(object sender, RoutedEventArgs e)
    {
        Character = new Character();
        currentPath = null;
        UpdateFileStatus();
        Recalculate();
    }

    private void Wizard_Click(object sender, RoutedEventArgs e)
    {
        var wizard = new CharacterWizardWindow { Owner = this };
        if (wizard.ShowDialog() == true && wizard.CreatedCharacter is not null)
        {
            Character = wizard.CreatedCharacter;
            currentPath = null;
            UpdateFileStatus();
            Recalculate();
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "BattleTech characters (*.btcc)|*.btcc|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var loaded = LegacyCharacterSerializer.Load(dialog.FileName);
            Character = loaded;
            currentPath = dialog.FileName;
            UpdateFileStatus();
            Recalculate();
        }
        catch (Exception ex)
        {
            ShowOperationError(ex, "Unable to open character");
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (currentPath is null)
        {
            SaveAs_Click(sender, e);
            return;
        }
        SaveTo(currentPath);
    }

    private void SaveAs_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "BattleTech characters (*.btcc)|*.btcc",
            DefaultExt = ".btcc",
            AddExtension = true,
            FileName = currentPath is null ? $"{Character.Name}.btcc" : Path.GetFileName(currentPath)
        };
        if (dialog.ShowDialog(this) == true)
        {
            SaveTo(dialog.FileName);
        }
    }

    private void SaveTo(string path)
    {
        try
        {
            LegacyCharacterSerializer.Save(Character, path);
            currentPath = path;
            UpdateFileStatus();
            Recalculate();
        }
        catch (Exception ex)
        {
            ShowOperationError(ex, "Unable to save character");
        }
    }

    private void PreviewSheet_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var directory = Path.Combine(
                Path.GetTempPath(), "A-Time-of-War-Character-Sheets");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory,
                $"{SafeFileName(Character.Name)}-preview.pdf");
            CharacterSheetExporter.Export(Character, Catalog, path);
            Process.Start(new ProcessStartInfo(path)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            ShowOperationError(ex, "Unable to preview character sheet");
        }
    }

    private void ExportPdf_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Filter = "PDF documents (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            AddExtension = true,
            FileName = $"{SafeFileName(Character.Name)} Character Sheet.pdf"
        };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            CharacterSheetExporter.Export(Character, Catalog, dialog.FileName);
            FileStatus.Text = $"Exported {dialog.FileName}";
        }
        catch (Exception ex)
        {
            ShowOperationError(ex, "Unable to export character sheet");
        }
    }

    private void ShowOperationError(Exception exception, string title)
    {
        var reportPath = WriteOperationReport(exception, title);
        MessageBox.Show(this,
            $"{exception.Message}\n\nA report was saved here:\n{reportPath}",
            title,
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static string WriteOperationReport(
        Exception exception,
        string title,
        string? path = null) =>
        AppErrorReporter.WriteReport(exception, title, path);

    private static string SafeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(value
            .Select(character => invalid.Contains(character) ? '_' : character)
            .ToArray()).Trim();
        return cleaned.Length == 0 ? "Character" : cleaned;
    }

    private void AddSkill_Click(object sender, RoutedEventArgs e)
    {
        var name = SkillPicker.Text.Trim();
        if (name.Length > 0 && Character.Skills.All(item => item.Name != name))
        {
            Character.Skills.Add(new NamedValue(name, 0));
            Recalculate();
        }
    }

    private void RemoveSkill_Click(object sender, RoutedEventArgs e)
    {
        if (SkillsGrid.SelectedItem is XpEditorRow item)
        {
            Character.Skills.Remove(item.Source);
            Recalculate();
        }
    }

    private void AddTrait_Click(object sender, RoutedEventArgs e)
    {
        var name = TraitPicker.Text.Trim();
        if (name.Length > 0 && Character.Traits.All(item => item.Name != name))
        {
            Character.Traits.Add(new NamedValue(name, 0));
            Recalculate();
        }
    }

    private void RemoveTrait_Click(object sender, RoutedEventArgs e)
    {
        if (TraitsGrid.SelectedItem is XpEditorRow item)
        {
            Character.Traits.Remove(item.Source);
            Recalculate();
        }
    }

    private void AddEquipment_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedEquipmentCatalogItem is not { } item) return;
        Character.Equipment.Add(new EquipmentItem
        {
            Name = item.Name,
            Cost = item.Cost,
            Mass = item.Mass,
            Locations = item.Locations,
            Armor = item.Armor,
            Notes = item.Notes,
            PatchCount = "0",
            Count = "1"
        });
        Recalculate();
    }

    private void RemoveEquipment_Click(object sender, RoutedEventArgs e)
    {
        if (EquipmentGrid.SelectedItem is EquipmentItem item)
        {
            Character.Equipment.Remove(item);
            Recalculate();
        }
    }

    private void AddWeapon_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedWeaponCatalogItem is not { } item) return;
        Character.Weapons.Add(new WeaponItem
        {
            Skill = item.Skill,
            Name = item.Name,
            Damage = item.Damage,
            Range = item.Range,
            Cost = item.Cost,
            Mass = item.Mass,
            Shots = item.Shots,
            AmmoCost = item.AmmoCost,
            AmmoMass = item.AmmoMass,
            AmmoCount = "0",
            Notes = item.Notes,
            Count = "1"
        });
        Recalculate();
    }

    private void RemoveWeapon_Click(object sender, RoutedEventArgs e)
    {
        if (WeaponsGrid.SelectedItem is WeaponItem item)
        {
            Character.Weapons.Remove(item);
            Recalculate();
        }
    }

    private void ReloadCatalog()
    {
        Catalog = ResourceCatalog.Load(
            resourcePath,
            new ResourceCatalogOptions(includeCompanionContent));
        SelectedEquipmentCatalogItem = null;
        SelectedWeaponCatalogItem = null;
        SelectedSkillName = null;
        SelectedTraitName = null;
        RebuildCatalogViews();
        OnPropertyChanged(nameof(Catalog));
    }

    private void IncreaseXp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: XpEditorRow row })
        {
            AdjustXp(row, 5, true);
        }
    }

    private void DecreaseXp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: XpEditorRow row })
        {
            AdjustXp(row, -5, true);
        }
    }

    private bool AdjustXp(XpEditorRow row, int delta, bool showMessage)
    {
        if (delta > 0 && Summary.FreeXp < delta)
        {
            if (showMessage)
            {
                MessageBox.Show(this,
                    $"This change needs {delta} XP, but only {Summary.FreeXp} XP remains.",
                    "Not enough Free XP",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return false;
        }

        var minimum = row.Kind == XpKind.Trait ? -1000 : 0;
        var next = row.Source.Value + delta;
        if (next < minimum)
        {
            if (showMessage)
            {
                MessageBox.Show(this,
                    row.Kind == XpKind.Trait
                        ? "Trait XP cannot be reduced below -1000."
                        : $"{row.Kind} XP cannot be reduced below zero.",
                    "XP limit",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            return false;
        }

        row.Source.Value = next;
        Recalculate();
        return true;
    }

    private void Recalculate_Click(object sender, RoutedEventArgs e) => Recalculate();

    private void Recalculate()
    {
        Summary = CharacterRules.Calculate(Character);
        PrerequisiteIssues = PrerequisiteRules.Evaluate(Character);
        RebuildXpRows();
        var blocking = PrerequisiteIssues.Count(issue =>
            issue.Category == "Affiliation");
        RuleStatus = blocking > 0
            ? $"{blocking} blocking conflict(s) must be corrected."
            : PrerequisiteIssues.Count > 0
                ? $"{PrerequisiteIssues.Count} prerequisite warning(s) remain."
                : "No unmet prerequisites.";
        RuleStatusBrush = blocking > 0
            ? Brushes.Firebrick
            : PrerequisiteIssues.Count > 0
                ? Brushes.DarkGoldenrod
                : Brushes.DarkGreen;
        OnPropertyChanged(nameof(Character));
        OnPropertyChanged(nameof(CharacterName));
        OnPropertyChanged(nameof(SubAffiliation));
        OnPropertyChanged(nameof(BirthYear));
        OnPropertyChanged(nameof(GameYear));
    }

    private void RebuildXpRows()
    {
        AttributeRows.Clear();
        SkillRows.Clear();
        TraitRows.Clear();

        foreach (var item in Character.Attributes)
        {
            AttributeRows.Add(CreateXpRow(item, XpKind.Attribute));
        }
        foreach (var item in Character.Skills.OrderBy(item => item.Name))
        {
            SkillRows.Add(CreateXpRow(item, XpKind.Skill));
        }
        foreach (var item in Character.Traits.OrderBy(item => item.Name))
        {
            TraitRows.Add(CreateXpRow(item, XpKind.Trait));
        }
        SkillRowsView.Refresh();
        TraitRowsView.Refresh();
    }

    private XpEditorRow CreateXpRow(NamedValue item, XpKind kind)
    {
        var category = kind.ToString();
        var issue = PrerequisiteIssues.FirstOrDefault(candidate =>
            candidate.Category == category && candidate.Name == item.Name);
        var level = kind switch
        {
            XpKind.Attribute => CharacterRules.AttributeValue(item.Value),
            XpKind.Skill => CharacterRules.SkillLevel(item.Value, Character.Traits),
            XpKind.Trait => CharacterRules.TraitLevel(item.Name, item.Value),
            _ => 0
        };
        return new XpEditorRow(item, kind, level, issue?.RequiredXp);
    }

    private bool MatchesSkillFilter(object item) =>
        item is XpEditorRow row &&
        (SkillFilter.Length == 0 ||
         row.Name.Contains(SkillFilter, StringComparison.OrdinalIgnoreCase));

    private bool MatchesTraitFilter(object item) =>
        item is XpEditorRow row &&
        (TraitFilter.Length == 0 ||
         row.Name.Contains(TraitFilter, StringComparison.OrdinalIgnoreCase));

    private bool MatchesEquipmentCatalogFilter(object item) =>
        item is EquipmentCatalogItem equipment &&
        MatchesCatalogText(
            EquipmentCatalogFilter,
            equipment.Name,
            equipment.Category,
            equipment.Notes,
            equipment.SourceLabel);

    private bool MatchesWeaponCatalogFilter(object item) =>
        item is WeaponCatalogItem weapon &&
        MatchesCatalogText(
            WeaponCatalogFilter,
            weapon.Name,
            weapon.Category,
            weapon.Skill,
            weapon.Notes,
            weapon.SourceLabel);

    private static bool MatchesCatalogText(
        string filter,
        params string[] fields) =>
        filter.Length == 0 ||
        fields.Any(field => field.Contains(
            filter,
            StringComparison.OrdinalIgnoreCase));

    private void RebuildCatalogViews()
    {
        EquipmentCatalogView = CollectionViewSource.GetDefaultView(Catalog.Equipment);
        EquipmentCatalogView.Filter = MatchesEquipmentCatalogFilter;
        WeaponCatalogView = CollectionViewSource.GetDefaultView(Catalog.Weapons);
        WeaponCatalogView.Filter = MatchesWeaponCatalogFilter;
        OnPropertyChanged(nameof(EquipmentCatalogView));
        OnPropertyChanged(nameof(WeaponCatalogView));
    }

    private void EditorFieldChanged(object sender, KeyboardFocusChangedEventArgs e) =>
        Recalculate();

    private void EditorSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) Recalculate();
    }

    private void EditorTabs_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (!IsLoaded || e.Source != EditorTabs) return;
        Dispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            ApplyXpGridColumnWidths);
    }

    private void ApplyXpGridColumnWidths()
    {
        SetXpGridColumnWidths(AttributesGrid, 90);
        SetXpGridColumnWidths(SkillsGrid, 80);
        SetXpGridColumnWidths(TraitsGrid, 80);
    }

    private static void SetXpGridColumnWidths(DataGrid grid, double levelWidth)
    {
        if (grid.Columns.Count != 4) return;
        grid.Columns[0].MinWidth = 280;
        grid.Columns[1].MinWidth = levelWidth;
        grid.Columns[2].MinWidth = 150;
        grid.Columns[3].MinWidth = 155;
        grid.Columns[0].Width = new DataGridLength(
            1, DataGridLengthUnitType.Star);
        grid.Columns[1].Width = new DataGridLength(levelWidth);
        grid.Columns[2].Width = new DataGridLength(150);
        grid.Columns[3].Width = new DataGridLength(155);
        grid.InvalidateMeasure();
    }

    private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, Recalculate);
    }

    private void UpdateFileStatus() =>
        FileStatus.Text = currentPath is null ? "New character" : currentPath;

    private static void VerifyRoundTrip(Character expected, Character actual)
    {
        var sameIdentity =
            expected.Name == actual.Name &&
            expected.Affiliation == actual.Affiliation &&
            expected.SubAffiliation == actual.SubAffiliation &&
            expected.BirthAffiliation == actual.BirthAffiliation &&
            expected.BirthSubAffiliation == actual.BirthSubAffiliation &&
            expected.BirthYear == actual.BirthYear &&
            expected.GameYear == actual.GameYear &&
            expected.ClanCaste == actual.ClanCaste &&
            expected.ClanTrainingField == actual.ClanTrainingField &&
            expected.Phenotype == actual.Phenotype &&
            expected.EarlyChildhood == actual.EarlyChildhood &&
            expected.LateChildhood == actual.LateChildhood &&
            expected.School == actual.School &&
            expected.BasicSchool == actual.BasicSchool &&
            expected.AdvancedSchool == actual.AdvancedSchool &&
            expected.SpecialSchool == actual.SpecialSchool &&
            expected.RealLife == actual.RealLife &&
            expected.Notes == actual.Notes;
        var sameCollections =
            SameValues(expected.Attributes, actual.Attributes) &&
            SameValues(expected.Traits, actual.Traits) &&
            SameValues(expected.Skills, actual.Skills) &&
            SameValues(expected.PreAttributes, actual.PreAttributes) &&
            SameValues(expected.PreTraits, actual.PreTraits) &&
            SameValues(expected.PreSkills, actual.PreSkills) &&
            expected.RealLifeHistory.SequenceEqual(actual.RealLifeHistory);
        if (!sameIdentity || !sameCollections)
        {
            throw new InvalidOperationException(
                "The character changed after saving and reopening it.");
        }
    }

    private static bool SameValues(
        IEnumerable<NamedValue> expected,
        IEnumerable<NamedValue> actual) =>
        expected.Select(item => (item.Name, item.Value))
            .SequenceEqual(actual.Select(item => (item.Name, item.Value)));

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static string PlainCatalogText(string text) =>
        WebUtility.HtmlDecode(text)
            .Replace("<br>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br/>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
            .Replace("<br />", Environment.NewLine, StringComparison.OrdinalIgnoreCase);

    public enum XpKind
    {
        Attribute,
        Skill,
        Trait
    }

    public sealed class XpEditorRow(
        NamedValue source,
        XpKind kind,
        int level,
        int? requiredXp)
    {
        public NamedValue Source { get; } = source;
        public XpKind Kind { get; } = kind;
        public string Name => Source.Name;
        public int Xp => Source.Value;
        public int Level { get; } = level;
        public int? RequiredXp { get; } = requiredXp;
        public bool IsShort => RequiredXp is int required && Xp < required;
        public string TargetText => RequiredXp is int required
            ? IsShort ? $"Need {required} XP" : "Met"
            : "";
    }
}
