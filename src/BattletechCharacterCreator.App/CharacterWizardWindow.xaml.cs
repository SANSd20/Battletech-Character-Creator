using System.IO;
using System.Windows;
using System.Windows.Controls;
using BattletechCharacterCreator.Core.LifePath;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;

namespace BattletechCharacterCreator.App;

public partial class CharacterWizardWindow : Window
{
    private readonly Dictionary<string, ChoiceInput> choiceControls = [];
    private readonly List<(FrameworkElement Element, int Step)> choiceGroups = [];
    private readonly FrameworkElement[] pages;
    private readonly TextBlock[] stepLabels;
    private int currentStep;
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
        pages =
        [
            BasicInfoPage, Stage0Page, Stage1Page, Stage2Page,
            Stage3Page, Stage4Page, ReviewPage
        ];
        stepLabels =
        [
            Step0Label, Step1Label, Step2Label, Step3Label,
            Step4Label, Step5Label, Step6Label
        ];

        var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        var resources = ResourceCatalog.Load(resourcePath);
        HomePlanetPicker.ItemsSource = resources.Planets;
        HairColorPicker.ItemsSource = resources.HairColors;
        EyeColorPicker.ItemsSource = resources.EyeColors;
        SexPicker.ItemsSource = new[] { "Male", "Female" };
        SexPicker.SelectedIndex = 0;

        AffiliationPicker.ItemsSource = BirthAffiliations;
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
        Loaded += (_, _) =>
        {
            RefreshModules();
            ShowStep(0);
        };
    }

    public Character? CreatedCharacter { get; private set; }

    public void ShowStepForCapture(int step) => ShowStep(step);

    public void SelectAffiliationForCapture(string affiliationId)
    {
        if (affiliationId is "comstar" or "word-of-blake")
        {
            OrderMemberCheck.IsChecked = true;
            ComStarRadio.IsChecked = affiliationId == "comstar";
            WordOfBlakeRadio.IsChecked = affiliationId == "word-of-blake";
        }
        else
        {
            OrderMemberCheck.IsChecked = false;
            AffiliationPicker.SelectedItem = BirthAffiliations
                .First(module => module.Id == affiliationId);
        }
        RefreshModules();
    }

    public void SelectLateChildhoodForCapture(string lateChildhoodId)
    {
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
            .First(module => module.Id == lateChildhoodId);
        RefreshModules();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        if (currentStep > 0) ShowStep(currentStep - 1);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateStep(currentStep)) return;
        if (currentStep < pages.Length - 1) ShowStep(currentStep + 1);
    }

    private void ShowStep(int step)
    {
        currentStep = Math.Clamp(step, 0, pages.Length - 1);
        for (var index = 0; index < pages.Length; index++)
        {
            pages[index].Visibility = index == currentStep
                ? Visibility.Visible
                : Visibility.Collapsed;
            stepLabels[index].Foreground = index == currentStep
                ? System.Windows.Media.Brushes.White
                : index < currentStep
                    ? new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(168, 178, 124))
                    : new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromRgb(189, 189, 185));
            stepLabels[index].FontWeight = index == currentStep
                ? FontWeights.Bold
                : FontWeights.Normal;
        }

        ChoicesHost.Visibility = currentStep is >= 1 and <= 5
            ? Visibility.Visible
            : Visibility.Collapsed;
        ChoicesHost.Width = currentStep == 1 ? 360 : 450;
        TotalsHost.Visibility = currentStep > 1
            ? Visibility.Visible
            : Visibility.Collapsed;
        TotalsGapRow.Height = currentStep > 1
            ? new GridLength(18)
            : new GridLength(0);
        TotalsRow.Height = currentStep > 1
            ? new GridLength(225)
            : new GridLength(0);
        UpdateChoiceGroupVisibility();
        BackButton.IsEnabled = currentStep > 0;
        NextButton.Visibility = currentStep < pages.Length - 1
            ? Visibility.Visible
            : Visibility.Collapsed;
        CreateButton.Visibility = currentStep == pages.Length - 1
            ? Visibility.Visible
            : Visibility.Collapsed;
        NextButton.IsDefault = currentStep < pages.Length - 1;
        CreateButton.IsDefault = currentStep == pages.Length - 1;

        if (currentStep > 0) UpdatePreview();
    }

    private bool ValidateStep(int step)
    {
        string? message = step switch
        {
            0 when string.IsNullOrWhiteSpace(CharacterName.Text) =>
                "Enter a character name.",
            0 when !TryReadPositiveNumber(AgeInput, out _) =>
                "Enter a valid age.",
            0 when !TryReadPositiveNumber(HeightInput, out _) =>
                "Enter a valid height.",
            0 when !TryReadPositiveNumber(WeightInput, out _) =>
                "Enter a valid weight.",
            1 when SelectedAffiliation is null =>
                "Choose an affiliation.",
            1 when LanguagePicker.SelectedItem is null =>
                "Choose a primary language.",
            2 when SelectedChildhood is null =>
                "Choose an early childhood.",
            3 when SelectedLateChildhood is null =>
                "Choose a late childhood.",
            4 when SelectedSchool is not null && SelectedBasicField is null =>
                "Choose one Basic Field for the selected school.",
            _ => null
        };
        if (message is null) return true;

        MessageBox.Show(this, message, "Complete this step",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private static bool TryReadPositiveNumber(TextBox input, out int value) =>
        int.TryParse(input.Text, out value) && value > 0;

    public void SmokeAllSelections()
    {
        foreach (var affiliation in BirthAffiliations)
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
        foreach (var orderId in new[] { "comstar", "word-of-blake" })
        {
            SelectAffiliationForCapture(orderId);
            BuildChoiceControls();
            UpdatePreview();
        }
        OrderMemberCheck.IsChecked = false;
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

    private LifePathModule? SelectedBirthAffiliation =>
        AffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedOrderAffiliation =>
        OrderMemberCheck.IsChecked == true
            ? LifePathCatalog.Affiliations.First(module =>
                module.Id == (WordOfBlakeRadio.IsChecked == true
                    ? "word-of-blake"
                    : "comstar"))
            : null;
    private LifePathModule? SelectedAffiliation =>
        SelectedOrderAffiliation ?? SelectedBirthAffiliation;
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
    private static IReadOnlyList<LifePathModule> BirthAffiliations =>
        LifePathCatalog.Affiliations
            .Where(module => module.Id is not ("comstar" or "word-of-blake"))
            .ToArray();

    private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RefreshModules();
    }

    private void RefreshModules()
    {
        refreshing = true;
        var affiliation = SelectedAffiliation;
        var birthAffiliation = SelectedBirthAffiliation;
        var childhood = SelectedChildhood;
        var lateChildhood = SelectedLateChildhood;
        var school = SelectedSchool;
        AffiliationDescription.Text = SelectedOrderAffiliation is null
            ? birthAffiliation?.Description ?? ""
            : $"{affiliation?.Description}{Environment.NewLine}{birthAffiliation?.Description}";
        ChildhoodDescription.Text = childhood?.Description ?? "";
        ChildhoodModuleCost.Text = childhood?.ModuleCost.ToString() ?? "";
        LateChildhoodDescription.Text = lateChildhood?.Description ?? "";
        LateChildhoodModuleCost.Text = lateChildhood?.ModuleCost.ToString() ?? "";
        SchoolDescription.Text = school?.Description ?? "";
        RealLifeDescription.Text = SelectedRealLife?.Description ?? "";
        SecondRealLifeDescription.Text =
            SelectedSecondRealLife?.Description ?? "";
        SubAffiliationPicker.ItemsSource = birthAffiliation?.SubAffiliations ?? [];
        SubAffiliationPanel.Visibility = SubAffiliationPicker.Items.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (SubAffiliationPicker.Items.Count > 0) SubAffiliationPicker.SelectedIndex = 0;

        CastePicker.ItemsSource = birthAffiliation?.Castes ?? [];
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

    private void OrderSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        RefreshModules();
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
            choiceGroups.Clear();

            foreach (var selectedModule in SelectedModuleEntries())
            {
                var module = selectedModule.Module;
                if (module.Choices.Count == 0) continue;

                var modulePanel = new StackPanel();
                ChoicesPanel.Children.Add(modulePanel);
                choiceGroups.Add((modulePanel, ChoiceStep(selectedModule)));
                foreach (var choice in module.Choices)
                {
                    var choiceXp = choice.Xp * choice.Count;
                    var header = new TextBlock
                    {
                        Text = $"{module.Name}: {choice.Label} ({choiceXp:+#;-#;0} XP)",
                        FontWeight = FontWeights.SemiBold,
                        Margin = new Thickness(0, 7, 0, 2)
                    };
                    modulePanel.Children.Add(header);

                    var controls = new List<ComboBox>();
                    var options = ResolveChoiceOptions(choice);
                    if (choice.Target == EffectTarget.Flexible &&
                        !choice.FixedFlexibleSelections)
                    {
                        var amounts = new List<TextBox>();
                        var totalXp = choiceXp;
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
                            modulePanel.Children.Add(row);
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
                        modulePanel.Children.Add(picker);
                    }
                    choiceControls[Key(selectedModule, choice)] =
                        new ChoiceInput(controls);
                }
            }
        }
        finally
        {
            refreshing = wasRefreshing;
            UpdateChoiceGroupVisibility();
        }
    }

    private int ChoiceStep(SelectedModule selectedModule)
    {
        if (selectedModule.IsStage4) return 5;
        if (selectedModule.Module == SelectedChildhood) return 2;
        if (selectedModule.Module == SelectedLateChildhood) return 3;
        if (selectedModule.Module == SelectedSchool ||
            selectedModule.Module == SelectedBasicField ||
            selectedModule.Module == SelectedAdvancedField ||
            selectedModule.Module == SelectedSpecialistField)
        {
            return 4;
        }
        return 1;
    }

    private void UpdateChoiceGroupVisibility()
    {
        foreach (var group in choiceGroups)
        {
            group.Element.Visibility = group.Step == currentStep
                ? Visibility.Visible
                : Visibility.Collapsed;
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
            Stage0Attributes.ItemsSource = character.Attributes;
            Stage0Skills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            Stage0Traits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ModuleCost.Text =
                LifePathEngine.CalculateModuleCost(character, SelectedModules()).ToString();
            SpentXp.Text = summary.SpentXp.ToString();
            FreeXp.Text = summary.FreeXp.ToString();
            RunningFreeXp.Text = summary.FreeXp.ToString();
        }
        catch (InvalidOperationException)
        {
            PreviewAttributes.ItemsSource = null;
            PreviewSkills.ItemsSource = null;
            PreviewTraits.ItemsSource = null;
            Stage0Attributes.ItemsSource = null;
            Stage0Skills.ItemsSource = null;
            Stage0Traits.ItemsSource = null;
            RunningFreeXp.Text = "";
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
        character.HomePlanet = HomePlanetPicker.Text.Trim();
        character.Sex = SexPicker.SelectedItem as string ?? "Male";
        character.Age = TryReadPositiveNumber(AgeInput, out var age) ? age : 21;
        character.Height = TryReadPositiveNumber(HeightInput, out var height)
            ? height : 175;
        character.Weight = TryReadPositiveNumber(WeightInput, out var weight)
            ? weight : 80;
        character.HairColor = HairColorPicker.Text.Trim();
        character.EyeColor = EyeColorPicker.Text.Trim();
        character.Affiliation = affiliation.Name;
        character.SubAffiliation = SelectedOrderAffiliation is null
            ? SelectedSubAffiliation?.Name ?? ""
            : "";
        character.BirthAffiliation = SelectedOrderAffiliation is null
            ? ""
            : SelectedBirthAffiliation?.Name ?? "";
        character.BirthSubAffiliation = SelectedOrderAffiliation is null
            ? ""
            : SelectedSubAffiliation?.Name ?? "";
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
            $"\nBirth affiliation: {character.BirthAffiliation}" +
            $"\nBirth sub-affiliation: {character.BirthSubAffiliation}" +
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
                     (SelectedBirthAffiliation, false),
                     (SelectedSubAffiliation, false),
                     (SelectedOrderAffiliation, false),
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
