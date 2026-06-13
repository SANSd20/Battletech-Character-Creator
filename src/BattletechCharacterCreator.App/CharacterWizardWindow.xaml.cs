using System.Windows;
using System.Windows.Controls;
using BattletechCharacterCreator.Core.LifePath;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Rules;

namespace BattletechCharacterCreator.App;

public partial class CharacterWizardWindow : Window
{
    private readonly Dictionary<string, ChoiceInput> choiceControls = [];
    private bool refreshing;

    private sealed record ChoiceInput(
        IReadOnlyList<ComboBox> Pickers,
        IReadOnlyList<TextBox>? Amounts = null);

    private sealed record SelectedModule(
        LifePathModule Module,
        int Occurrence,
        bool IsStage4 = false);

    public CharacterWizardWindow()
    {
        InitializeComponent();
        AffiliationPicker.ItemsSource = LifePathCatalog.Affiliations;
        ChildhoodPicker.ItemsSource = LifePathCatalog.Childhoods;
        LateChildhoodPicker.ItemsSource = LifePathCatalog.LateChildhoods;
        SchoolPicker.ItemsSource = LifePathCatalog.EducationSchools;
        RealLifePicker.ItemsSource = LifePathCatalog.RealLifeModules;
        SecondRealLifePicker.ItemsSource = LifePathCatalog.RealLifeModules;
        AffiliationPicker.SelectedIndex = 0;
        ChildhoodPicker.SelectedIndex = 0;
        LateChildhoodPicker.SelectedIndex = 0;
        SchoolPicker.SelectedIndex = -1;
        RealLifePicker.SelectedIndex = -1;
        SecondRealLifePicker.SelectedIndex = -1;
        Loaded += (_, _) => RefreshModules();
    }

    public Character? CreatedCharacter { get; private set; }

    public void SmokeAllSelections()
    {
        foreach (var affiliation in LifePathCatalog.Affiliations)
        {
            AffiliationPicker.SelectedItem = affiliation;
            RefreshModules();
            foreach (var subAffiliation in affiliation.SubAffiliations ?? [])
            {
                SubAffiliationPicker.SelectedItem = subAffiliation;
                BuildChoiceControls();
                UpdatePreview();
            }
            foreach (var caste in affiliation.Castes ?? [])
            {
                CastePicker.SelectedItem = caste;
                BuildChoiceControls();
                UpdatePreview();
            }
        }
        foreach (var childhood in LifePathCatalog.Childhoods)
        {
            ChildhoodPicker.SelectedItem = childhood;
            RefreshModules();
        }
        foreach (var lateChildhood in LifePathCatalog.LateChildhoods)
        {
            LateChildhoodPicker.SelectedItem = lateChildhood;
            RefreshModules();
        }
        foreach (var school in LifePathCatalog.EducationSchools)
        {
            SchoolPicker.SelectedItem = school;
            RefreshModules();
            foreach (var field in school.BasicFields ?? [])
            {
                BasicFieldPicker.SelectedItem = field;
                BuildChoiceControls();
                UpdatePreview();
            }
            foreach (var field in school.AdvancedFields ?? [])
            {
                AdvancedFieldPicker.SelectedItem = field;
                RefreshThirdField(school);
                foreach (var thirdField in SpecialistFieldPicker.Items.Cast<LifePathModule>())
                {
                    SpecialistFieldPicker.SelectedItem = thirdField;
                    BuildChoiceControls();
                    UpdatePreview();
                }
            }
        }
        foreach (var realLife in LifePathCatalog.RealLifeModules)
        {
            RealLifePicker.SelectedItem = realLife;
            BuildChoiceControls();
            UpdatePreview();
            SecondRealLifePicker.SelectedItem = realLife;
            BuildChoiceControls();
            UpdatePreview();
            SecondRealLifePicker.SelectedIndex = -1;
        }
        RealLifePicker.SelectedIndex = -1;
        BuildChoiceControls();
        UpdatePreview();
    }

    private LifePathModule? SelectedAffiliation => AffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSubAffiliation => SubAffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedCaste => CastePicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedChildhood => ChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedLateChildhood => LateChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSchool => SchoolPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedBasicField => BasicFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedAdvancedField => AdvancedFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSpecialistField => SpecialistFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedRealLife => RealLifePicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSecondRealLife =>
        SecondRealLifePicker.SelectedItem as LifePathModule;

    private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RefreshModules();
    }

    private void RefreshModules()
    {
        refreshing = true;
        var affiliation = SelectedAffiliation;
        var childhood = SelectedChildhood;
        var lateChildhood = SelectedLateChildhood;
        var school = SelectedSchool;
        AffiliationDescription.Text = affiliation?.Description ?? "";
        ChildhoodDescription.Text = childhood?.Description ?? "";
        LateChildhoodDescription.Text = lateChildhood?.Description ?? "";
        SchoolDescription.Text = school?.Description ?? "";
        RealLifeDescription.Text = SelectedRealLife?.Description ?? "";
        SecondRealLifeDescription.Text =
            SelectedSecondRealLife?.Description ?? "";

        SubAffiliationPicker.ItemsSource = affiliation?.SubAffiliations ?? [];
        SubAffiliationPanel.Visibility = SubAffiliationPicker.Items.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (SubAffiliationPicker.Items.Count > 0) SubAffiliationPicker.SelectedIndex = 0;

        CastePicker.ItemsSource = affiliation?.Castes ?? [];
        CastePanel.Visibility = CastePicker.Items.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (CastePicker.Items.Count > 0) CastePicker.SelectedIndex = 0;

        LanguagePicker.ItemsSource = affiliation?.Languages ?? [];
        if (LanguagePicker.Items.Count > 0) LanguagePicker.SelectedIndex = 0;

        RefreshEducationFields(school);
        BuildChoiceControls();
        refreshing = false;
        UpdatePreview();
    }

    private void SchoolSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        RefreshModules();
    }

    private void RealLifeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        RealLifeDescription.Text = SelectedRealLife?.Description ?? "";
        SecondRealLifeDescription.Text =
            SelectedSecondRealLife?.Description ?? "";
        BuildChoiceControls();
        UpdatePreview();
    }

    private void RefreshEducationFields(LifePathModule? school)
    {
        SetDependentPicker(BasicFieldPicker, BasicFieldPanel, school?.BasicFields, true);
        SetDependentPicker(AdvancedFieldPicker, AdvancedFieldPanel, school?.AdvancedFields, false);
        RefreshThirdField(school);
    }

    private void RefreshThirdField(LifePathModule? school)
    {
        var fields = SelectedAdvancedField is null
            ? []
            : (school?.AdvancedFields ?? [])
                .Where(field => field.Id != SelectedAdvancedField.Id)
                .Concat(school?.SpecialistFields ?? [])
                .ToArray();
        SetDependentPicker(SpecialistFieldPicker, SpecialistFieldPanel, fields, false);
    }

    private static void SetDependentPicker(
        ComboBox picker, FrameworkElement panel, IReadOnlyList<LifePathModule>? modules,
        bool required)
    {
        picker.ItemsSource = modules ?? [];
        panel.Visibility = picker.Items.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        picker.SelectedIndex = required && picker.Items.Count > 0 ? 0 : -1;
    }

    private void BuildChoiceControls()
    {
        var wasRefreshing = refreshing;
        refreshing = true;
        try
        {
            ChoicesPanel.Children.Clear();
            choiceControls.Clear();

            foreach (var selectedModule in SelectedModuleEntries())
            {
                var module = selectedModule.Module;
                foreach (var choice in module.Choices)
                {
                    var header = new TextBlock
                    {
                        Text = $"{module.Name}: {choice.Label} ({choice.Xp:+#;-#;0} XP)",
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 7, 0, 2)
                    };
                    ChoicesPanel.Children.Add(header);

                    var controls = new List<ComboBox>();
                    var options = ResolveChoiceOptions(choice);
                    if (choice.Target == EffectTarget.Flexible &&
                        !choice.FixedFlexibleSelections)
                    {
                        var amounts = new List<TextBox>();
                        var totalXp = choice.Xp * choice.Count;
                        var educationOptions = ResolveEducationFieldOptions(choice);
                        for (var i = 0; i < 6; i++)
                        {
                            var row = new Grid { Margin = new Thickness(0, 2, 12, 2) };
                            row.ColumnDefinitions.Add(new ColumnDefinition());
                            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(82) });
                            var picker = new ComboBox { ItemsSource = options };
                            picker.SelectionChanged += ChoiceSelectionChanged;
                            var defaultName = i switch
                            {
                                0 when choice.MinimumEducationFieldSkillXp > 0 =>
                                    educationOptions.FirstOrDefault(),
                                1 when choice.MinimumEducationFieldSkillXp > 0 =>
                                    choice.Options.FirstOrDefault(),
                                0 => options.FirstOrDefault(),
                                _ => null
                            };
                            picker.SelectedItem = defaultName;
                            var defaultXp = i switch
                            {
                                0 when choice.MinimumEducationFieldSkillXp > 0 =>
                                    choice.MinimumEducationFieldSkillXp,
                                1 when choice.MinimumEducationFieldSkillXp > 0 =>
                                    totalXp - choice.MinimumEducationFieldSkillXp,
                                0 => totalXp,
                                _ => 0
                            };
                            var amount = new TextBox
                            {
                                Text = defaultXp.ToString(),
                                Margin = new Thickness(6, 0, 0, 0)
                            };
                            Grid.SetColumn(amount, 1);
                            row.Children.Add(picker);
                            row.Children.Add(amount);
                            amount.TextChanged += ChoiceAmountChanged;
                            controls.Add(picker);
                            amounts.Add(amount);
                            ChoicesPanel.Children.Add(row);
                        }
                        choiceControls[Key(selectedModule, choice)] =
                            new ChoiceInput(controls, amounts);
                        continue;
                    }

                    for (var i = 0; i < choice.Count; i++)
                    {
                        var picker = new ComboBox
                        {
                            ItemsSource = choice.Options,
                            Margin = new Thickness(0, 2, 12, 2)
                        };
                        picker.SelectionChanged += ChoiceSelectionChanged;
                        picker.ItemsSource = options;
                        picker.SelectedIndex = Math.Min(i, options.Count - 1);
                        controls.Add(picker);
                        ChoicesPanel.Children.Add(picker);
                    }
                    choiceControls[Key(selectedModule, choice)] =
                        new ChoiceInput(controls);
                }
            }
        }
        finally
        {
            refreshing = wasRefreshing;
        }
    }

    private void ChoiceSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded && !refreshing) UpdatePreview();
    }

    private void ChoiceAmountChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded && !refreshing) UpdatePreview();
    }

    private IReadOnlyList<string> ResolveChoiceOptions(ModuleChoice choice)
    {
        if (choice.SolarisInternshipFieldSkillsOnly)
        {
            var internshipSkills =
                LifePathCatalog.ResolveSolarisInternshipFieldSkills(new Character
                {
                    School = SelectedSchool?.Name ?? "",
                    Affiliation = SelectedAffiliation?.Name ?? "",
                    BasicSchool = SelectedBasicField?.Name ?? "",
                    AdvancedSchool = SelectedAdvancedField?.Name ?? "",
                    SpecialSchool = SelectedSpecialistField?.Name ?? ""
                });
            return internshipSkills.Count == 0
                ? choice.Options
                : internshipSkills;
        }
        if (choice.SelectedEducationFieldSkillsOnly)
        {
            var selectedFieldSkills =
                LifePathCatalog.ResolveSelectedEducationFieldSkills(new Character
                {
                    Affiliation = SelectedAffiliation?.Name ?? "",
                    BasicSchool = SelectedBasicField?.Name ?? "",
                    AdvancedSchool = SelectedAdvancedField?.Name ?? "",
                    SpecialSchool = SelectedSpecialistField?.Name ?? ""
                });
            return selectedFieldSkills.Count == 0
                ? choice.Options
                : selectedFieldSkills;
        }
        var options = choice.Options.AsEnumerable();
        if (choice.EducationFieldNames is not null)
        {
            options = options.Concat(ResolveEducationFieldOptions(choice));
        }
        if (choice.ClanWarriorFieldSkillsOnly)
        {
            options = options.Concat(
                LifePathCatalog.ResolveClanWarriorFieldSkills(new Character
            {
                ClanTrainingField = GetSelectedChoice(SelectedLateChildhood, "branch")
            }));
        }
        return options
            .Distinct(StringComparer.Ordinal)
            .OrderBy(option => option)
            .ToArray();
    }

    private IReadOnlyList<string> ResolveEducationFieldOptions(ModuleChoice choice)
    {
        if (choice.EducationFieldNames is null) return [];
        return LifePathCatalog.ResolveEducationFieldSkills(
            new Character
            {
                Affiliation = SelectedAffiliation?.Name ?? "",
                BasicSchool = SelectedBasicField?.Name ?? "",
                AdvancedSchool = SelectedAdvancedField?.Name ?? "",
                SpecialSchool = SelectedSpecialistField?.Name ?? ""
            },
            choice.EducationFieldNames);
    }

    private string GetSelectedChoice(LifePathModule? module, string choiceId)
    {
        if (module is null) return "";
        var choice = module.Choices.FirstOrDefault(item => item.Id == choiceId);
        if (choice is null ||
            !choiceControls.TryGetValue(Key(module, choice, 0), out var input))
        {
            return "";
        }
        return input.Pickers.FirstOrDefault()?.SelectedItem as string ?? "";
    }

    private void DependentSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        refreshing = true;
        BuildChoiceControls();
        refreshing = false;
        UpdatePreview();
    }

    private void AdvancedFieldSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        refreshing = true;
        RefreshThirdField(SelectedSchool);
        BuildChoiceControls();
        refreshing = false;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        try
        {
            var character = BuildCharacter();
            var summary = CharacterRules.Calculate(character);
            PreviewAttributes.ItemsSource = character.Attributes;
            PreviewSkills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            PreviewTraits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ModuleCost.Text =
                LifePathEngine.CalculateModuleCost(character, SelectedModules()).ToString();
            SpentXp.Text = summary.SpentXp.ToString();
            FreeXp.Text = summary.FreeXp.ToString();
        }
        catch (InvalidOperationException)
        {
            PreviewAttributes.ItemsSource = null;
            PreviewSkills.ItemsSource = null;
            PreviewTraits.ItemsSource = null;
        }
    }

    private Character BuildCharacter()
    {
        var affiliation = SelectedAffiliation ??
            throw new InvalidOperationException("Choose an affiliation.");
        var childhood = SelectedChildhood ??
            throw new InvalidOperationException("Choose an early childhood.");
        var lateChildhood = SelectedLateChildhood ??
            throw new InvalidOperationException("Choose a late childhood.");
        var school = SelectedSchool;
        if (school is not null && SelectedBasicField is null)
        {
            throw new InvalidOperationException("Choose one Basic Field for the selected school.");
        }
        if (SelectedSpecialistField is not null && SelectedAdvancedField is null)
        {
            throw new InvalidOperationException(
                "Choose an Advanced Field before adding a third field.");
        }
        var language = LanguagePicker.SelectedItem as string ??
            throw new InvalidOperationException("Choose a primary language.");

        var character = LifePathEngine.CreateBase(CharacterName.Text.Trim(), language);
        character.Affiliation = affiliation.Name;
        character.SubAffiliation = SelectedSubAffiliation?.Name ?? "";
        character.ClanCaste = SelectedCaste?.Name ?? "";
        character.ClanTrainingField = GetSelectedChoice(SelectedLateChildhood, "branch");
        character.EarlyChildhood = childhood.Name;
        character.LateChildhood = lateChildhood.Name;
        character.School = school?.Name ?? "";
        character.BasicSchool = SelectedBasicField?.Name ?? "";
        character.AdvancedSchool = SelectedAdvancedField?.Name ?? "";
        character.SpecialSchool = SelectedSpecialistField?.Name ?? "";
        character.RealLife =
            SelectedSecondRealLife?.Name ?? SelectedRealLife?.Name ?? "";
        var phenotype = GetSelectedChoice(SelectedChildhood, "phenotype");
        if (phenotype.Length > 0) character.Phenotype = phenotype;

        var selectedModules = SelectedModuleEntries().ToArray();
        var modules = selectedModules.Select(entry => entry.Module).ToArray();
        foreach (var selectedModule in selectedModules)
        {
            var module = selectedModule.Module;
            var selection = CreateSelection(selectedModule);
            if (selectedModule.IsStage4)
            {
                LifePathEngine.ApplyStage4(character, selection);
                ApplyCareerState(character, module.Name);
            }
            else
            {
                LifePathEngine.Apply(character, selection);
            }
        }
        LifePathEngine.ApplyAffiliationContext(character, affiliation, childhood, language);
        LifePathEngine.ApplyAffiliationContext(character, affiliation, lateChildhood, language);
        if (school is not null)
        {
            LifePathEngine.ApplyAffiliationContext(character, affiliation, school, language);
        }
        foreach (var field in modules.Where(module =>
                     module.Id.StartsWith("field-", StringComparison.Ordinal)))
        {
            LifePathEngine.ApplyAffiliationContext(character, affiliation, field, language);
        }
        foreach (var career in selectedModules.Where(entry => entry.IsStage4))
        {
            LifePathEngine.ApplyAffiliationContext(
                character, affiliation, career.Module, language);
        }
        LifePathEngine.ApplyModuleAccounting(character, modules);
        character.Notes =
            $"-----Life Path-----\nAffiliation: {affiliation.Name}" +
            $"\nSub-affiliation: {character.SubAffiliation}" +
            $"\nClan caste: {character.ClanCaste}" +
            $"\nEarly Childhood: {childhood.Name}" +
            $"\nLate Childhood: {lateChildhood.Name}" +
            $"\nSchool: {character.School}" +
            $"\nBasic Field: {character.BasicSchool}" +
            $"\nAdvanced Field: {character.AdvancedSchool}" +
            $"\nSpecialist Field: {character.SpecialSchool}" +
            $"\nCareers: {string.Join(" -> ", character.RealLifeHistory)}";
        return character;
    }

    private ModuleSelection CreateSelection(SelectedModule selectedModule)
    {
        var module = selectedModule.Module;
        var choices = new Dictionary<string, IReadOnlyList<string>>();
        var allocations = new Dictionary<string, IReadOnlyList<ChoiceAllocation>>();
        foreach (var choice in module.Choices)
        {
            if (!choiceControls.TryGetValue(
                    Key(selectedModule, choice), out var input))
            {
                throw new InvalidOperationException(
                    $"{module.Name}: choices are still loading. Please try again.");
            }
            if (choice.Target == EffectTarget.Flexible &&
                !choice.FixedFlexibleSelections &&
                input.Amounts is not null)
            {
                allocations[choice.Id] = input.Pickers
                    .Zip(input.Amounts)
                    .Select(pair => new ChoiceAllocation(
                        pair.First.SelectedItem as string ?? "",
                        int.TryParse(pair.Second.Text, out var xp) ? xp : 0))
                    .Where(allocation => allocation.Xp > 0)
                    .ToArray();
                choices[choice.Id] = [];
                continue;
            }

            var selected = input.Pickers
                .Select(control => control.SelectedItem as string ?? "")
                .Where(value => value.Length > 0)
                .ToArray();
            choices[choice.Id] = selected;
        }
        return new ModuleSelection(module, choices, allocations);
    }

    private IEnumerable<LifePathModule> SelectedModules()
    {
        return SelectedModuleEntries().Select(entry => entry.Module);
    }

    private IEnumerable<SelectedModule> SelectedModuleEntries()
    {
        var occurrences = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (var entry in new[]
                 {
                     (SelectedAffiliation, false),
                     (SelectedSubAffiliation, false),
                     (SelectedCaste, false),
                     (SelectedChildhood, false),
                     (SelectedLateChildhood, false),
                     (SelectedSchool, false),
                     (SelectedBasicField, false),
                     (SelectedAdvancedField, false),
                     (SelectedSpecialistField, false),
                     (SelectedRealLife, true),
                     (SelectedSecondRealLife, true)
                 })
        {
            if (entry.Item1 is null) continue;
            occurrences.TryGetValue(entry.Item1.Id, out var occurrence);
            yield return new SelectedModule(entry.Item1, occurrence, entry.Item2);
            occurrences[entry.Item1.Id] = occurrence + 1;
        }
    }

    private static void ApplyCareerState(Character character, string career)
    {
        if (career.StartsWith("Clan Warrior Washout - ",
                StringComparison.Ordinal))
        {
            character.ClanCaste =
                career["Clan Warrior Washout - ".Length..];
        }
        if (career == "Dark Caste")
        {
            character.ClanCaste = "Dark Caste";
        }
    }

    private static string Key(
        SelectedModule module,
        ModuleChoice choice) =>
        Key(module.Module, choice, module.Occurrence);

    private static string Key(
        LifePathModule module,
        ModuleChoice choice,
        int occurrence) =>
        $"{module.Id}:{occurrence}:{choice.Id}";

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            CreatedCharacter = BuildCharacter();
            var issues = PrerequisiteRules.Evaluate(CreatedCharacter);
            var blockingIssues = issues
                .Where(issue => issue.Category == "Affiliation")
                .ToArray();
            if (blockingIssues.Length > 0)
            {
                var details = string.Join(Environment.NewLine,
                    blockingIssues.Select(issue => $"{issue.Category}: {issue.Name}"));
                CreatedCharacter = null;
                MessageBox.Show(this,
                    $"This life path has unmet prerequisites:{Environment.NewLine}{details}",
                    "Prerequisites not met", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (issues.Count > 0)
            {
                var details = string.Join(Environment.NewLine,
                    issues.Select(issue => $"{issue.Category}: {issue.Name}"));
                MessageBox.Show(this,
                    $"These prerequisites must be met as the character develops:{Environment.NewLine}{details}",
                    "Later prerequisites", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            DialogResult = true;
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(this, ex.Message, "Incomplete character",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
