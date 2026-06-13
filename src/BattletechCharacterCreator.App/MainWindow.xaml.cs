using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
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

    public MainWindow() : this(new Character())
    {
    }

    public MainWindow(Character initialCharacter)
    {
        character = initialCharacter;
        var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        Catalog = ResourceCatalog.Load(resourcePath);
        summary = CharacterRules.Calculate(character);
        DataContext = this;
        InitializeComponent();
        UpdateFileStatus();
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

    public string CharacterName { get => Character.Name; set => Character.Name = value; }
    public string SubAffiliation { get => Character.SubAffiliation; set => Character.SubAffiliation = value; }
    public string Sex { get => Character.Sex; set => Character.Sex = value; }
    public int Age { get => Character.Age; set => Character.Age = value; }
    public int CharacterHeight { get => Character.Height; set => Character.Height = value; }
    public int Weight { get => Character.Weight; set => Character.Weight = value; }
    public string Notes { get => Character.Notes; set => Character.Notes = value; }
    public object Attributes => Character.Attributes;
    public object Skills => Character.Skills;
    public object Traits => Character.Traits;
    public object Equipment => Character.Equipment;
    public object Weapons => Character.Weapons;

    public event PropertyChangedEventHandler? PropertyChanged;

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
        if (SkillsGrid.SelectedItem is NamedValue item)
        {
            Character.Skills.Remove(item);
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
        if (TraitsGrid.SelectedItem is NamedValue item)
        {
            Character.Traits.Remove(item);
            Recalculate();
        }
    }

    private void Recalculate_Click(object sender, RoutedEventArgs e) => Recalculate();

    private void Recalculate()
    {
        Summary = CharacterRules.Calculate(Character);
        PrerequisiteIssues = PrerequisiteRules.Evaluate(Character);
    }

    private void UpdateFileStatus() =>
        FileStatus.Text = currentPath is null ? "New character" : currentPath;

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
