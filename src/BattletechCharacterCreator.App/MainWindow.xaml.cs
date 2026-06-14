using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

    public ObservableCollection<XpEditorRow> AttributeRows { get; } = [];
    public ObservableCollection<XpEditorRow> SkillRows { get; } = [];
    public ObservableCollection<XpEditorRow> TraitRows { get; } = [];
    public ICollectionView SkillRowsView { get; }
    public ICollectionView TraitRowsView { get; }

    public MainWindow() : this(new Character())
    {
    }

    public MainWindow(Character initialCharacter)
    {
        character = initialCharacter;
        var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        Catalog = ResourceCatalog.Load(resourcePath);
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

    public ResourceCatalog Catalog { get; }
    public string[] SexOptions { get; } = ["Male", "Female"];
    public CharacterSummary Summary
    {
        get => summary;
        private set
        {
            summary = value;
            OnPropertyChanged();
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
    public int Age { get => Character.Age; set => Character.Age = value; }
    public int CharacterHeight { get => Character.Height; set => Character.Height = value; }
    public int Weight { get => Character.Weight; set => Character.Weight = value; }
    public string Notes { get => Character.Notes; set => Character.Notes = value; }
    public object Equipment => Character.Equipment;
    public object Weapons => Character.Weapons;
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

    public event PropertyChangedEventHandler? PropertyChanged;

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
        CharacterSheetExporter.Export(Character, Catalog, path);
        var bytes = File.ReadAllBytes(path);
        var text = System.Text.Encoding.ASCII.GetString(bytes);
        var expectedPages =
            Character.Traits.Count > 8 || Character.Skills.Count > 30 ? 3 : 2;
        if (bytes.Length < 100_000 ||
            !text.StartsWith("%PDF-1.4", StringComparison.Ordinal) ||
            !text.Contains($"/Count {expectedPages}", StringComparison.Ordinal) ||
            !text.Contains(EscapeSmokeText(Character.Name),
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Character sheet PDF export did not produce a complete document.");
        }
    }

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
            MessageBox.Show(this, ex.Message, "Unable to open character",
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(this, ex.Message, "Unable to save character",
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(this, ex.Message, "Unable to preview character sheet",
                MessageBoxButton.OK, MessageBoxImage.Error);
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
            MessageBox.Show(this, ex.Message, "Unable to export character sheet",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

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
