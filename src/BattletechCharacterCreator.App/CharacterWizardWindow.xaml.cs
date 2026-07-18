using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BattletechCharacterCreator.Core.LifePath;
using BattletechCharacterCreator.Core.Models;
using BattletechCharacterCreator.Core.Resources;
using BattletechCharacterCreator.Core.Rules;

namespace BattletechCharacterCreator.App;

public partial class CharacterWizardWindow : Window
{
    private const int Stage4Step = 5;
    private const int FreeXpStep = 6;
    private const int LateChildhoodCompletionAge = 16;

    private readonly Dictionary<string, ChoiceInput> choiceControls = [];
    private readonly Dictionary<string, IReadOnlyList<ChoiceAllocation>>
        flexibleAllocationCache = [];
    private readonly List<(FrameworkElement Element, int Step)> choiceGroups = [];
    private readonly FrameworkElement[] pages;
    private readonly TextBlock[] stepLabels;
    private Character? lastTotalsCharacter;
    private string lastRunningFreeXp = "";
    private int currentStep;
    private bool refreshing;
    private IReadOnlyList<FreeXpTargetOption> allFreeXpTargetOptions = [];
    private readonly Dictionary<string, int> reviewFreeXpAllocations = [];

    private sealed record ChoiceInput(
        IReadOnlyList<ComboBox> Pickers,
        IReadOnlyList<TextBox>? Amounts = null,
        ModuleChoice? Choice = null,
        string ModuleName = "",
        int Step = -1,
        TextBlock? Remaining = null,
        TextBlock? Status = null,
        IReadOnlyList<string>? Options = null);

    private sealed record XpAdjustment(TextBox Input, int Delta);

    private sealed record FlexibleRowRequest(
        Panel RowHost,
        IReadOnlyList<string> Options,
        List<ComboBox> Pickers,
        List<TextBox> Amounts);

    private sealed record SelectedModule(
        LifePathModule Module,
        int Occurrence,
        bool IsStage4 = false);

    private sealed record SubAffiliationOption(
        string Name,
        LifePathModule? Module);

    private sealed record CareerAvailability(
        LifePathModule Module,
        IReadOnlyList<PrerequisiteIssue> Issues);

    private sealed record ReviewIssueRow(
        PrerequisiteIssue Issue,
        int MissingXp,
        bool CanSpendFreeXp,
        string ActionText)
    {
        public string Category => Issue.Category;
        public string Name => Issue.Name;
        public int RequiredXp => Issue.RequiredXp;
        public int ActualXp => Issue.ActualXp;
    }

    private sealed record CareerPrerequisiteRow(
        string Career,
        PrerequisiteIssue Issue,
        int MissingXp,
        bool CanSpendFreeXp,
        string ActionText)
    {
        public string Category => Issue.Category;
        public string Name => Issue.Name;
        public int RequiredXp => Issue.RequiredXp;
        public int ActualXp => Issue.ActualXp;
    }

    private sealed record FreeXpTargetOption(string Category, string Name)
    {
        public string DisplayName => $"{Category} / {Name}";
    }

    private sealed record FreeXpAllocationRow(
        string Key,
        string Category,
        string Name,
        int Xp);

    private static string IssueKey(PrerequisiteIssue issue) =>
        $"{issue.Category}|{issue.Name}|{issue.RequiredXp}|{issue.ActualXp}";

    public CharacterWizardWindow()
    {
        InitializeComponent();
        pages =
        [
            BasicInfoPage, Stage0Page, Stage1Page, Stage2Page,
            Stage3Page, Stage4Page, FreeXpPage, ReviewPage
        ];
        stepLabels =
        [
            Step0Label, Step1Label, Step2Label, Step3Label,
            Step4Label, Step5Label, Step6Label, Step7Label
        ];

        var resourcePath = Path.Combine(AppContext.BaseDirectory, "Resources");
        var resources = ResourceCatalog.Load(resourcePath);
        HairColorPicker.ItemsSource = resources.HairColors;
        EyeColorPicker.ItemsSource = resources.EyeColors;
        SexPicker.ItemsSource = new[] { "Male", "Female" };
        SexPicker.SelectedIndex = 0;
        allFreeXpTargetOptions = BuildFreeXpTargetOptions(
            resources.TraitNames,
            resources.SkillNames);
        RefreshFreeXpTargetOptions();

        RefreshEraAvailability();
        AffiliationPicker.ItemsSource = BirthAffiliations;
        SchoolPicker.ItemsSource = LifePathCatalog.EducationSchools;
        SecondSchoolPicker.ItemsSource = LifePathCatalog.EducationSchools;
        ThirdSchoolPicker.ItemsSource = LifePathCatalog.EducationSchools;
        RealLifePicker.ItemsSource = LifePathCatalog.RealLifeModules;
        SecondRealLifePicker.ItemsSource = LifePathCatalog.RealLifeModules;
        AffiliationPicker.SelectedIndex = 0;
        RefreshChildhoodAvailability(null, null);
        SchoolPicker.SelectedIndex = -1;
        SecondSchoolPicker.SelectedIndex = -1;
        ThirdSchoolPicker.SelectedIndex = -1;
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
        EnsureAffiliationAvailableForCapture(affiliationId);
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

    private void SelectSubAffiliationForCapture(string subAffiliationName)
    {
        SubAffiliationPicker.SelectedItem = SubAffiliationPicker.Items
            .Cast<SubAffiliationOption>()
            .First(option => option.Module?.Name == subAffiliationName);
    }

    private string[] AvailableSubAffiliationNames() =>
        SubAffiliationPicker.Items
            .Cast<SubAffiliationOption>()
            .Where(option => option.Module is not null)
            .Select(option => option.Name)
            .ToArray();

    private void EnsureAffiliationAvailableForCapture(string affiliationId)
    {
        if (EraAvailabilityCatalog.EarliestAffiliationYear(affiliationId) is not { } earliest ||
            CurrentGameYear >= earliest)
        {
            return;
        }

        GameYearInput.Text = earliest.ToString();
        RefreshEraAvailability();
    }

    public void SelectInvadingClanTestPath()
    {
        SelectClanTestPath("invading-clan", "Ghost Bear");
    }

    public void SelectHomeworldClanTestPath()
    {
        SelectClanTestPath("homeworld-clan", "Goliath Scorpion");
    }

    private void SelectClanTestPath(
        string affiliationId,
        string subAffiliationName)
    {
        SelectAffiliationForCapture(affiliationId);
        ChildhoodPicker.SelectedItem = LifePathCatalog.Childhoods
            .First(module => module.Id == "trueborn-creche");
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
            .First(module => module.Id == "late-trueborn-sibko");
        RefreshModules();
        SelectSubAffiliationForCapture(subAffiliationName);
        CastePicker.SelectedItem = CastePicker.Items
            .Cast<LifePathModule>()
            .First(module => module.Name == "MechWarrior");
        BuildChoiceControls();
        SelectModuleChoice(SelectedChildhood!, "phenotype",
            "Phenotype/MechWarrior");
        SelectModuleChoice(SelectedLateChildhood!, "branch", "MechWarrior");
        UpdatePreview();
    }

    public void SmokeInvadingClanCharacter()
    {
        SelectInvadingClanTestPath();
        var character = BuildCharacter();
        if (character.Affiliation != "Invading Clan" ||
            character.SubAffiliation != "Ghost Bear" ||
            character.ClanCaste != "MechWarrior" ||
            character.Phenotype != "Phenotype/MechWarrior" ||
            character.ClanTrainingField != "MechWarrior")
        {
            throw new InvalidOperationException(
                "The Invading Clan test path did not preserve its selections.");
        }
        if (!character.Skills.Any(skill =>
                skill.Name == "Gunnery/Mech" && skill.Value >= 15) ||
            !character.Skills.Any(skill =>
                skill.Name == "Piloting/BattleMech" && skill.Value >= 15))
        {
            throw new InvalidOperationException(
                "The Trueborn MechWarrior branch did not apply its skills.");
        }
        if (PrerequisiteRules.Evaluate(character)
            .Any(issue => issue.Category == "Affiliation"))
        {
            throw new InvalidOperationException(
                "The Invading Clan test path has an affiliation conflict.");
        }
        CreatedCharacter = character;
    }

    public void SmokeHomeworldClanCharacter()
    {
        SelectHomeworldClanTestPath();
        var character = BuildCharacter();
        if (character.Affiliation != "Homeworld Clan" ||
            character.SubAffiliation != "Goliath Scorpion" ||
            character.ClanCaste != "MechWarrior" ||
            character.Phenotype != "Phenotype/MechWarrior" ||
            character.ClanTrainingField != "MechWarrior")
        {
            throw new InvalidOperationException(
                "The Homeworld Clan test path did not preserve its selections.");
        }
        if (!character.Skills.Any(skill =>
                skill.Name == "Gunnery/Mech" && skill.Value >= 15) ||
            !character.Skills.Any(skill =>
                skill.Name == "Piloting/BattleMech" && skill.Value >= 15))
        {
            throw new InvalidOperationException(
                "The Homeworld Trueborn MechWarrior branch did not apply its skills.");
        }
        if (PrerequisiteRules.Evaluate(character)
            .Any(issue => issue.Category == "Affiliation"))
        {
            throw new InvalidOperationException(
                "The Homeworld Clan test path has an affiliation conflict.");
        }
        CreatedCharacter = character;
    }

    public void SmokeCampaignYearEraSelection()
    {
        GameYearInput.Text = "2750";
        RefreshEraAvailability();
        if (!InferredEraLabel.Text.Contains("Star League", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The campaign year did not infer the Star League era.");
        }
        if (AffiliationPicker.Items
            .Cast<LifePathModule>()
            .Any(module => module.Id == "invading-clan") ||
            OrderMemberCheck.IsEnabled)
        {
            throw new InvalidOperationException(
                "Star League era availability did not hide later-era affiliations.");
        }

        GameYearInput.Text = "3025";
        RefreshEraAvailability();
        if (!AffiliationPicker.Items
                .Cast<LifePathModule>()
                .Any(module => module.Id == "invading-clan") ||
            !AffiliationPicker.Items
                .Cast<LifePathModule>()
                .Any(module => module.Id == "homeworld-clan"))
        {
            throw new InvalidOperationException(
                "Pre-invasion era availability did not show both Clan origins.");
        }

        GameYearInput.Text = "3052";
        RefreshEraAvailability();
        if (GameYearInput.Text != "3052")
        {
            throw new InvalidOperationException(
                "The campaign year did not remain set to Clan Invasion.");
        }

        var character = BuildCharacter();
        if (character.GameYear != 3052 ||
            character.Age != character.GameYear - character.BirthYear)
        {
            throw new InvalidOperationException(
                "Campaign year era selection was not applied to the created character.");
        }
        if (!AffiliationPicker.Items
            .Cast<LifePathModule>()
            .Any(module => module.Id == "invading-clan") ||
            !OrderMemberCheck.IsEnabled ||
            !WordOfBlakeRadio.IsEnabled)
        {
            throw new InvalidOperationException(
                "Clan Invasion era availability did not reveal later-era affiliations.");
        }

        GameYearInput.Text = "3045";
        RefreshEraAvailability();
        SelectAffiliationForCapture("rasalhague");
        if (SubAffiliationPicker.Items.Count != 0)
        {
            throw new InvalidOperationException(
                "Pre-Invasion Rasalhague sub-affiliation availability did not hide later options.");
        }

        GameYearInput.Text = "3052";
        RefreshEraAvailability();
        SelectAffiliationForCapture("rasalhague");
        var clanInvasionSubAffiliations = AvailableSubAffiliationNames();
        if (!clanInvasionSubAffiliations.Contains("Clan War Expatriate") ||
            clanInvasionSubAffiliations.Contains("Ghost Bear Dominion"))
        {
            throw new InvalidOperationException(
                "Clan Invasion Rasalhague sub-affiliation availability is wrong.");
        }

        GameYearInput.Text = "3062";
        RefreshEraAvailability();
        SelectAffiliationForCapture("rasalhague");
        if (!AvailableSubAffiliationNames().Contains("Ghost Bear Dominion"))
        {
            throw new InvalidOperationException(
                "Civil War Rasalhague sub-affiliation availability did not reveal Ghost Bear Dominion.");
        }
    }

    public void SmokeStageLimitedPreview()
    {
        GameYearInput.Text = "3052";
        RefreshEraAvailability();
        SelectAffiliationForCapture("fed-suns");
        SelectSubAffiliationForCapture("Capellan March");
        ChildhoodPicker.SelectedItem = LifePathCatalog.Childhoods
            .First(module => module.Id == "back-woods");
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
            .First(module => module.Id == "late-back-woods");
        RefreshModules();
        BuildChoiceControls();
        ShowStep(1);
        var stage0Attributes = Stage0Attributes.ItemsSource
            .Cast<NamedValue>()
            .ToDictionary(item => item.Name, item => item.Value);
        if (stage0Attributes["STR"] != 100 ||
            stage0Attributes["WIL"] != 140 ||
            stage0Attributes["BOD"] != 100)
        {
            throw new InvalidOperationException(
                "Stage 0 preview must only include base, affiliation, and sub-affiliation attributes.");
        }

        ShowStep(2);
        var stage1Attributes = PreviewAttributes.ItemsSource
            .Cast<NamedValue>()
            .ToDictionary(item => item.Name, item => item.Value);
        var expectedStage1Attributes = BuildCharacter(
                2,
                allowIncompleteCurrentStepPreview: true)
            .Attributes
            .ToDictionary(item => item.Name, item => item.Value);
        if (!stage1Attributes.SequenceEqual(expectedStage1Attributes))
        {
            throw new InvalidOperationException(
                "Stage 1 preview must match the stage-limited character totals.");
        }

        ShowStep(3);
        var stage2Character = BuildCharacter(
            3,
            allowIncompleteCurrentStepPreview: true);
        var stage2ModuleCost = LifePathEngine.CalculateModuleCost(
            stage2Character, SelectedModules(3));
        var expectedStage2FreeXp = LifePathEngine.StartingXp - stage2ModuleCost;
        if (stage2Character.Age != LateChildhoodCompletionAge)
        {
            throw new InvalidOperationException(
                "Stage 2 preview must leave the character at age 16 after late childhood.");
        }
        if (LateChildhoodModuleCost.Text != SelectedLateChildhood!.ModuleCost.ToString() ||
            RunningFreeXp.Text != expectedStage2FreeXp.ToString())
        {
            throw new InvalidOperationException(
                "Stage 2 preview must show the late-childhood module cost and remove it from running Free XP.");
        }

        CompleteFlexibleChoicesForSmoke(3);
        SelectEducationForCapture("trade-school");
        ShowStep(4);
        var stage3Character = BuildCharacter(
            4,
            allowIncompleteCurrentStepPreview: true);
        var expectedStage3Age = LateChildhoodCompletionAge +
            SelectedEducationFields().Sum(module => module.TimeYears);
        if (stage3Character.Age != expectedStage3Age)
        {
            throw new InvalidOperationException(
                "Stage 3 preview must add only selected education time to the age 16 baseline.");
        }
    }

    private void CompleteFlexibleChoicesForSmoke(int step)
    {
        foreach (var input in choiceControls.Values)
        {
            if (input.Step != step ||
                input.Choice?.Target != EffectTarget.Flexible ||
                input.Choice.FixedFlexibleSelections ||
                input.Amounts is null ||
                input.Options is null ||
                input.Pickers.Count == 0)
            {
                continue;
            }

            var target = input.Options.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(target))
            {
                continue;
            }

            input.Pickers[0].SelectedItem = target;
            input.Amounts[0].Text =
                (input.Choice.Xp * input.Choice.Count).ToString();
            for (var i = 1; i < input.Pickers.Count; i++)
            {
                input.Pickers[i].SelectedItem = null;
                input.Amounts[i].Text = "0";
            }
        }
        CaptureFlexibleAllocations();
        UpdateFlexibleChoiceDisplays();
        UpdatePreview();
    }

    public void SmokeAffiliationFilteredChildhoods()
    {
        SelectAffiliationForCapture("fed-suns");
        var innerSphereChildhoods = ChildhoodPicker.Items
            .Cast<LifePathModule>()
            .Select(module => module.Id)
            .ToArray();
        var innerSphereLateChildhoods = LateChildhoodPicker.Items
            .Cast<LifePathModule>()
            .Select(module => module.Id)
            .ToArray();
        if (innerSphereChildhoods.Contains("trueborn-creche") ||
            innerSphereLateChildhoods.Contains("late-clan-apprenticeship") ||
            innerSphereLateChildhoods.Contains("late-freeborn-sibko") ||
            innerSphereLateChildhoods.Contains("late-trueborn-sibko"))
        {
            throw new InvalidOperationException(
                "Inner Sphere affiliations must not offer Clan-only childhood modules.");
        }

        SelectAffiliationForCapture("invading-clan");
        var clanChildhoods = ChildhoodPicker.Items
            .Cast<LifePathModule>()
            .Select(module => module.Id)
            .ToArray();
        var clanLateChildhoods = LateChildhoodPicker.Items
            .Cast<LifePathModule>()
            .Select(module => module.Id)
            .ToArray();
        if (!clanChildhoods.Contains("trueborn-creche") ||
            !clanLateChildhoods.Contains("late-freeborn-sibko") ||
            !clanLateChildhoods.Contains("late-trueborn-sibko") ||
            clanLateChildhoods.Contains("late-high-school"))
        {
            throw new InvalidOperationException(
                "Clan affiliations must offer Clan childhood modules and hide non-Clan High School.");
        }
    }

    public void SelectLateChildhoodForCapture(string lateChildhoodId)
    {
        LateChildhoodPicker.SelectedItem = AvailableLateChildhoods()
            .First(module => module.Id == lateChildhoodId);
        RefreshModules();
    }

    public void SelectEducationForCapture(
        string schoolId,
        string? advancedField = null,
        string? thirdField = null)
    {
        EducationCheck.IsChecked = true;
        SchoolPicker.SelectedItem = LifePathCatalog.EducationSchools
            .First(module => module.Id == schoolId);
        RefreshModules();
        if (!string.IsNullOrWhiteSpace(advancedField))
        {
            AdvancedFieldPicker.SelectedItem = AdvancedFieldPicker.Items
                .Cast<LifePathModule>()
                .First(module => module.Name == advancedField);
            RefreshThirdField(SelectedSchool);
        }
        if (!string.IsNullOrWhiteSpace(thirdField))
        {
            SpecialistFieldPicker.SelectedItem = SpecialistFieldPicker.Items
                .Cast<LifePathModule>()
                .First(module => module.Name == thirdField);
        }
        BuildChoiceControls();
        UpdateEducationSummary();
        UpdatePreview();
    }

    public void SelectCareersForCapture(
        string firstCareerId,
        string? secondCareerId = null)
    {
        FirstCareerCheck.IsChecked = true;
        RefreshCareerOptions();
        RealLifePicker.SelectedItem = LifePathCatalog.RealLifeModules
            .First(module => module.Id == firstCareerId);
        RefreshCareerOptions();
        if (!string.IsNullOrWhiteSpace(secondCareerId))
        {
            SecondCareerCheck.IsChecked = true;
            SecondRealLifePicker.SelectedItem = SecondRealLifePicker.Items
                .Cast<LifePathModule>()
                .First(module => module.Id == secondCareerId);
        }
        else
        {
            SecondCareerCheck.IsChecked = false;
        }
        UpdateCareerSummary();
        BuildChoiceControls();
        UpdatePreview();
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
        TotalsHost.Visibility = currentStep is > 1 and < 6
            ? Visibility.Visible
            : Visibility.Collapsed;
        TotalsGapRow.Height = currentStep is > 1 and < 6
            ? new GridLength(18)
            : new GridLength(0);
        TotalsRow.Height = currentStep is > 1 and < 6
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

        if (currentStep == Stage4Step)
        {
            refreshing = true;
            RefreshCareerOptions();
            BuildChoiceControls();
            refreshing = false;
        }
        if (currentStep > 0) UpdatePreview();
    }

    private bool ValidateStep(int step)
    {
        string? message = step switch
        {
            0 when string.IsNullOrWhiteSpace(CharacterName.Text) =>
                "Enter a character name.",
            0 when !TryReadPositiveNumber(GameYearInput, out _) =>
                "Enter a valid campaign year.",
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
            4 when SelectedSecondSchool is not null && SelectedSecondBasicField is null =>
                "Choose one Basic Field for the second selected school.",
            4 when SelectedThirdSchool is not null && SelectedThirdBasicField is null =>
                "Choose one Basic Field for the third selected school.",
            4 when SelectedSpecialistField is not null && SelectedAdvancedField is null =>
                "Choose an Advanced Field before adding a third field.",
            4 when SelectedSecondSpecialistField is not null &&
                SelectedSecondAdvancedField is null =>
                "Choose an Advanced Field before adding a third field to the second education.",
            4 when SelectedThirdSpecialistField is not null &&
                SelectedThirdAdvancedField is null =>
                "Choose an Advanced Field before adding a third field to the third education.",
            _ => null
        };
        message ??= choiceControls.Values
            .Where(input => input.Step == step && input.Choice is not null)
            .Select(input => EvaluateFlexibleChoice(input))
            .FirstOrDefault(result => !result.IsValid)
            ?.Message;
        if (message is null) return true;

        MessageBox.Show(this, message, "Complete this step",
            MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private static bool TryReadPositiveNumber(TextBox input, out int value) =>
        int.TryParse(input.Text, out value) && value > 0;

    public void SmokeAllSelections()
    {
        GameYearInput.Text = "3145";
        RefreshEraAvailability();
        foreach (var affiliation in BirthAffiliations)
        {
            AffiliationPicker.SelectedItem = affiliation;
            RefreshModules();
            foreach (var subAffiliation in affiliation.SubAffiliations ?? [])
            {
                SelectSubAffiliationForCapture(subAffiliation.Name);
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
        EducationCheck.IsChecked = true;
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
        EducationCheck.IsChecked = false;
        FirstCareerCheck.IsChecked = true;
        foreach (var realLife in LifePathCatalog.RealLifeModules)
        {
            RealLifePicker.SelectedItem = realLife;
            RefreshSecondCareerOptions();
            BuildChoiceControls();
            UpdatePreview();
            if (realLife.Repeatable)
            {
                SecondCareerCheck.IsChecked = true;
                SecondRealLifePicker.SelectedItem = realLife;
                BuildChoiceControls();
                UpdatePreview();
                SecondCareerCheck.IsChecked = false;
            }
        }
        FirstCareerCheck.IsChecked = false;
        BuildChoiceControls();
        UpdatePreview();
    }

    public void SmokeCreateCharacter()
    {
        var character = BuildCharacter();
        if (PrerequisiteRules.Evaluate(character)
            .Any(issue => issue.Category == "Affiliation"))
        {
            throw new InvalidOperationException(
                "The default wizard path has a blocking affiliation conflict.");
        }
        CreatedCharacter = character;
    }

    public void SmokeReviewFreeXpFix()
    {
        reviewFreeXpAllocations.Clear();
        FirstCareerCheck.IsChecked = true;
        RefreshCareerOptions();
        if (RealLifePicker.Items.Count != LifePathCatalog.RealLifeModules.Count)
        {
            throw new InvalidOperationException(
                "Stage 4 career filtering did not show the full first career list.");
        }

        var character = BuildCharacter();
        var bod = character.Attributes.Single(item => item.Name == "BOD");
        var issue = new PrerequisiteIssue("Attribute", "BOD", bod.Value + 100, bod.Value);
        var row = BuildReviewIssueRows(
                [issue],
                CharacterRules.Calculate(character).FreeXp)
            .Single();
        if (!row.CanSpendFreeXp || row.MissingXp != 100)
        {
            throw new InvalidOperationException(
                "Review rule-check XP fix was not available for an affordable Attribute gap.");
        }

        SpendReviewFreeXp_Click(new Button { Tag = row }, new RoutedEventArgs());
        var fixedCharacter = BuildCharacter();
        var fixedBod = fixedCharacter.Attributes.Single(item => item.Name == "BOD");
        if (fixedBod.Value != issue.RequiredXp)
        {
            throw new InvalidOperationException(
                "Review rule-check XP fix did not spend Free XP on the target Attribute.");
        }

        ResetReviewXp_Click(this, new RoutedEventArgs());
        var resetBod = BuildCharacter()
            .Attributes
            .Single(item => item.Name == "BOD");
        if (resetBod.Value != bod.Value)
        {
            throw new InvalidOperationException(
                "Reset Review XP did not remove Review-screen XP allocations.");
        }

        var careerRows = BuildCareerPrerequisiteRows(
            CharacterRules.Calculate(BuildCharacter()).FreeXp);
        var careerRow = careerRows.FirstOrDefault(item => item.CanSpendFreeXp);
        if (careerRow is null)
        {
            throw new InvalidOperationException(
                "Free XP career planner did not expose any spendable career prerequisite gaps.");
        }

        SpendCareerPrerequisiteFreeXp_Click(
            new Button { Tag = careerRow }, new RoutedEventArgs());
        if (!BuildFreeXpAllocationRows().Any(item =>
                item.Category == careerRow.Category &&
                item.Name == careerRow.Name &&
                item.Xp >= careerRow.MissingXp))
        {
            throw new InvalidOperationException(
                "Free XP career planner did not allocate XP to the selected prerequisite gap.");
        }

        ResetReviewXp_Click(this, new RoutedEventArgs());

        FreeXpTargetPicker.SelectedItem = FreeXpTargetPicker.Items
            .Cast<FreeXpTargetOption>()
            .First(item => item.Category == "Skill" && item.Name == "Acrobatics");
        FreeXpAmount.Text = "25";
        AddFreeXp_Click(this, new RoutedEventArgs());
        var manualCharacter = BuildCharacter();
        if (FreeXpTargetPicker.Items
            .Cast<FreeXpTargetOption>()
            .Any(item => item.Category == "Skill" && item.Name == "Acrobatics"))
        {
            throw new InvalidOperationException(
                "Manual Free XP targets did not hide an already selected Skill.");
        }

        if (!manualCharacter.Skills.Any(item =>
                item.Name == "Acrobatics" && item.Value >= 25) ||
            CharacterRules.Calculate(manualCharacter).FreeXp !=
            CharacterRules.Calculate(character).FreeXp - 25)
        {
            throw new InvalidOperationException(
                "Manual Free XP spending did not apply to the selected Skill.");
        }

        var freeXpRow = BuildFreeXpAllocationRows()
            .Single(item => item.Category == "Skill" && item.Name == "Acrobatics");
        RemoveFreeXp_Click(new Button { Tag = freeXpRow }, new RoutedEventArgs());
        var removedCharacter = BuildCharacter();
        if (BuildFreeXpAllocationRows().Any(item =>
                item.Category == "Skill" && item.Name == "Acrobatics") ||
            !FreeXpTargetPicker.Items
                .Cast<FreeXpTargetOption>()
                .Any(item => item.Category == "Skill" && item.Name == "Acrobatics") ||
            CharacterRules.Calculate(removedCharacter).FreeXp !=
            CharacterRules.Calculate(character).FreeXp)
        {
            throw new InvalidOperationException(
                "Removing a Free XP allocation did not restore the spent XP.");
        }

        FreeXpTargetPicker.SelectedItem = FreeXpTargetPicker.Items
            .Cast<FreeXpTargetOption>()
            .First(item => item.Category == "Attribute" && item.Name == "STR");
        FreeXpAmount.Text = "10";
        AddFreeXp_Click(this, new RoutedEventArgs());
        var attributeCharacter = BuildCharacter();
        if (!attributeCharacter.Attributes.Any(item =>
                item.Name == "STR" &&
                item.Value == manualCharacter.Attributes
                    .Single(attribute => attribute.Name == "STR").Value + 10) ||
            CharacterRules.Calculate(attributeCharacter).FreeXp !=
            CharacterRules.Calculate(manualCharacter).FreeXp - 10)
        {
            throw new InvalidOperationException(
                "Manual Free XP spending did not apply to the selected Attribute.");
        }

        ResetReviewXp_Click(this, new RoutedEventArgs());
    }

    public void SmokeStage4CareerTotalsRefresh()
    {
        SelectAffiliationForCapture("lyran");
        EducationCheck.IsChecked = false;
        SchoolPicker.SelectedIndex = -1;
        SecondEducationCheck.IsChecked = false;
        SecondSchoolPicker.SelectedIndex = -1;
        ThirdEducationCheck.IsChecked = false;
        ThirdSchoolPicker.SelectedIndex = -1;
        ChildhoodPicker.SelectedItem = LifePathCatalog.Childhoods
            .First(module => module.Id == "street");
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
            .First(module => module.Id == "late-street");
        RefreshModules();
        BuildChoiceControls();
        CompleteFlexibleChoicesForSmoke(3);
        FirstCareerCheck.IsChecked = true;
        RefreshCareerOptions();
        RealLifePicker.SelectedItem = LifePathCatalog.RealLifeModules
            .First(module => module.Id == "real-agitator");
        RefreshCareerOptions();
        BuildChoiceControls();
        var agitator = BuildCharacter(
            Stage4Step,
            allowIncompleteCurrentStepPreview: true);
        if (!agitator.Traits.Any(item =>
                item.Name == "Gregarious" && item.Value > 0))
        {
            throw new InvalidOperationException(
                "Stage 4 Agitator totals did not include Gregarious.");
        }

        RealLifePicker.SelectedItem = LifePathCatalog.RealLifeModules
            .First(module => module.Id == "real-explorer");
        RefreshCareerOptions();
        BuildChoiceControls();
        var explorer = BuildCharacter(
            Stage4Step,
            allowIncompleteCurrentStepPreview: true);
        if (explorer.Traits.Any(item =>
                item.Name == "Gregarious" && item.Value > 0))
        {
            throw new InvalidOperationException(
                "Stage 4 career totals retained Agitator traits after switching careers.");
        }
        if (!explorer.Skills.Any(item =>
                item.Name == "Sensor Operations" && item.Value > 0))
        {
            throw new InvalidOperationException(
                "Stage 4 Explorer totals did not apply the replacement career.");
        }

        RealLifePicker.SelectedItem = LifePathCatalog.RealLifeModules
            .First(module => module.Id == "real-neer-do-well");
        RefreshCareerOptions();
        BuildChoiceControls();
        UpdatePreview();
        var previewAttributes = PreviewAttributes.ItemsSource?
            .Cast<NamedValue>()
            .ToArray() ?? [];
        var previewTraits = PreviewTraits.ItemsSource?
            .Cast<NamedValue>()
            .ToArray() ?? [];
        var previewSkills = PreviewSkills.ItemsSource?
            .Cast<NamedValue>()
            .ToArray() ?? [];
        if (!previewAttributes.Any(item =>
                item.Name == "EDG" && item.Value > 100) ||
            !previewTraits.Any(item => item.Name == "Extra Income") ||
            !previewSkills.Any(item => item.Name == "Acting"))
        {
            throw new InvalidOperationException(
                "Stage 4 preview totals disappeared before career Flexible XP was fully assigned.");
        }
    }

    public IReadOnlyList<Character> SmokeRepresentativeLifePaths()
    {
        var characters = new List<Character>
        {
            BuildSmokeLifePath(
                "lyran", "street", "late-street", "real-agitator"),
            BuildSmokeLifePath(
                "comstar", "street", "late-street",
                "real-comstar-service"),
            BuildSmokeLifePath(
                "major-periphery", "street", "late-street",
                "real-explorer")
        };

        SelectHomeworldClanTestPath();
        SelectCareersForCapture("real-goliath-scorpion-seeker");
        var clanCharacter = BuildCharacter();
        ValidateSmokeLifePath(clanCharacter, "Homeworld Clan",
            "Goliath Scorpion Seeker");
        characters.Add(clanCharacter);

        ValidateSmokeEffect(characters[0], "Protocol/Lyran",
            "Lyran Alliance affiliation");
        ValidateSmokeEffect(characters[0], "Leadership", "Agitator career");
        ValidateSmokeEffect(characters[1], "Protocol/ComStar",
            "ComStar affiliation");
        ValidateSmokeEffect(characters[1], "Communications/HPG",
            "ComStar Service career");
        if (!characters[2].Traits.Any(item =>
                item.Name == "Equipped" && item.Value != 0))
        {
            throw new InvalidOperationException(
                "Major Periphery affiliation did not apply Equipped.");
        }
        ValidateSmokeEffect(characters[2], "Sensor Operations",
            "Explorer career");
        ValidateSmokeEffect(clanCharacter, "Interests/Star League History",
            "Goliath Scorpion Seeker career");

        return characters;
    }

    private Character BuildSmokeLifePath(
        string affiliationId,
        string childhoodId,
        string lateChildhoodId,
        string careerId)
    {
        if (affiliationId is "comstar" or "word-of-blake")
        {
            AffiliationPicker.SelectedItem = BirthAffiliations
                .First(module => module.Id == "capellan");
        }
        SelectAffiliationForCapture(affiliationId);
        ChildhoodPicker.SelectedItem = LifePathCatalog.Childhoods
            .First(module => module.Id == childhoodId);
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
            .First(module => module.Id == lateChildhoodId);
        RefreshModules();
        SelectCareersForCapture(careerId);
        var character = BuildCharacter();
        ValidateSmokeLifePath(character,
            SelectedAffiliation!.Name, SelectedRealLife!.Name);
        return character;
    }

    private void ValidateSmokeLifePath(
        Character character,
        string affiliation,
        string career)
    {
        if (character.Affiliation != affiliation ||
            character.RealLife != career ||
            character.RealLifeHistory.LastOrDefault() != career)
        {
            throw new InvalidOperationException(
                $"{affiliation} / {career} did not preserve its life path.");
        }
        var issues = PrerequisiteRules.Evaluate(character);
        var blockingIssues = issues
            .Where(issue => issue.Category == "Affiliation")
            .ToArray();
        if (blockingIssues.Length > 0)
        {
            throw new InvalidOperationException(
                $"{affiliation} / {career} has unmet prerequisites: " +
                string.Join(", ", blockingIssues.Select(issue =>
                    $"{issue.Category} {issue.Name}")));
        }
        var expectedFreeXp = LifePathEngine.StartingXp -
            LifePathEngine.CalculateModuleCost(character, SelectedModules());
        if (CharacterRules.Calculate(character).FreeXp != expectedFreeXp)
        {
            throw new InvalidOperationException(
                $"{affiliation} / {career} has incorrect XP accounting.");
        }
        var expectedAge = LateChildhoodCompletionAge +
            SelectedEducationFields().Sum(module => module.TimeYears) +
            new[]
                {
                    SelectedRealLife,
                    SelectedSecondRealLife
                }
                .Where(module => module is not null)
                .Sum(module => module!.TimeYears);
        if (character.Age != expectedAge ||
            character.BirthYear != character.GameYear - expectedAge)
        {
            throw new InvalidOperationException(
                $"{affiliation} / {career} has incorrect age accounting.");
        }
    }

    private static void ValidateSmokeEffect(
        Character character,
        string skill,
        string source)
    {
        if (!character.Skills.Any(item =>
                item.Name == skill && item.Value != 0))
        {
            throw new InvalidOperationException(
                $"{source} did not apply {skill}.");
        }
    }

    private LifePathModule? SelectedBirthAffiliation =>
        AffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedOrderAffiliation =>
        OrderMemberCheck.IsChecked == true &&
        IsAffiliationAvailable(WordOfBlakeRadio.IsChecked == true
            ? "word-of-blake"
            : "comstar")
            ? LifePathCatalog.Affiliations.First(module =>
                module.Id == (WordOfBlakeRadio.IsChecked == true
                    ? "word-of-blake"
                    : "comstar"))
            : null;
    private LifePathModule? SelectedAffiliation =>
        SelectedOrderAffiliation ?? SelectedBirthAffiliation;
    private LifePathModule? SelectedSubAffiliation =>
        (SubAffiliationPicker.SelectedItem as SubAffiliationOption)?.Module ??
        SubAffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedCaste => CastePicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedChildhood => ChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedLateChildhood => LateChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSchool => EducationCheck.IsChecked == true
        ? SchoolPicker.SelectedItem as LifePathModule
        : null;
    private LifePathModule? SelectedBasicField => BasicFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedAdvancedField => AdvancedFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSpecialistField => SpecialistFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSecondSchool => SecondEducationCheck.IsChecked == true
        ? SecondSchoolPicker.SelectedItem as LifePathModule
        : null;
    private LifePathModule? SelectedSecondBasicField => SecondBasicFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSecondAdvancedField => SecondAdvancedFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSecondSpecialistField => SecondSpecialistFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedThirdSchool => ThirdEducationCheck.IsChecked == true
        ? ThirdSchoolPicker.SelectedItem as LifePathModule
        : null;
    private LifePathModule? SelectedThirdBasicField => ThirdBasicFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedThirdAdvancedField => ThirdAdvancedFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedThirdSpecialistField => ThirdSpecialistFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedRealLife => FirstCareerCheck.IsChecked == true
        ? RealLifePicker.SelectedItem as LifePathModule
        : null;
    private LifePathModule? SelectedSecondRealLife =>
        SecondCareerCheck.IsChecked == true
            ? SecondRealLifePicker.SelectedItem as LifePathModule
            : null;
    private int CurrentGameYear =>
        TryReadPositiveNumber(GameYearInput, out var gameYear)
            ? gameYear
            : 3045;
    private IReadOnlyList<LifePathModule> BirthAffiliations =>
        EraAvailabilityCatalog.FilterAffiliations(
            LifePathCatalog.Affiliations
            .Where(module => module.Id is not ("comstar" or "word-of-blake"))
            .ToArray(),
            CurrentGameYear);

    private bool SelectedAffiliationIsClan =>
        SelectedAffiliation?.Id is "invading-clan" or "homeworld-clan";

    private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        RefreshModules();
    }

    private void GameYearInputChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RefreshEraAvailability();
        if (currentStep > 0)
        {
            RefreshModules();
        }
    }

    private void RefreshEraAvailability()
    {
        if (AffiliationPicker is null) return;

        var birthAffiliation = SelectedBirthAffiliation;
        var availableBirthAffiliations = BirthAffiliations;
        AffiliationPicker.ItemsSource = availableBirthAffiliations;
        if (birthAffiliation is not null &&
            availableBirthAffiliations.Any(module => module.Id == birthAffiliation.Id))
        {
            AffiliationPicker.SelectedItem = availableBirthAffiliations
                .First(module => module.Id == birthAffiliation.Id);
        }
        else if (availableBirthAffiliations.Count > 0)
        {
            AffiliationPicker.SelectedIndex = 0;
        }

        var comStarAvailable = IsAffiliationAvailable("comstar");
        var wordOfBlakeAvailable = IsAffiliationAvailable("word-of-blake");
        ComStarRadio.IsEnabled = comStarAvailable;
        WordOfBlakeRadio.IsEnabled = wordOfBlakeAvailable;
        OrderMemberCheck.IsEnabled = comStarAvailable || wordOfBlakeAvailable;
        if (OrderMemberCheck.IsChecked == true)
        {
            if (ComStarRadio.IsChecked == true && !comStarAvailable)
            {
                ComStarRadio.IsChecked = false;
                WordOfBlakeRadio.IsChecked = wordOfBlakeAvailable;
            }
            if (WordOfBlakeRadio.IsChecked == true && !wordOfBlakeAvailable)
            {
                WordOfBlakeRadio.IsChecked = false;
                ComStarRadio.IsChecked = comStarAvailable;
            }
            if ((ComStarRadio.IsChecked != true && WordOfBlakeRadio.IsChecked != true) ||
                !OrderMemberCheck.IsEnabled)
            {
                OrderMemberCheck.IsChecked = false;
            }
        }

        EraAvailabilitySummary.Text = EraAvailabilityCatalog.BuildAffiliationSummary(
            LifePathCatalog.Affiliations,
            CurrentGameYear);
        InferredEraLabel.Text = EraPresetCatalog.BuildInferredEraLabel(
            CurrentGameYear);
    }

    private bool IsAffiliationAvailable(string id) =>
        LifePathCatalog.Affiliations.FirstOrDefault(module => module.Id == id) is not { } module ||
        EraAvailabilityCatalog.IsAffiliationAvailable(module, CurrentGameYear);

    private void RefreshModules()
    {
        refreshing = true;
        var affiliation = SelectedAffiliation;
        var birthAffiliation = SelectedBirthAffiliation;
        var subAffiliation = SelectedSubAffiliation;
        var childhood = SelectedChildhood;
        var lateChildhood = SelectedLateChildhood;
        var school = SelectedSchool;
        var secondSchool = SelectedSecondSchool;
        var thirdSchool = SelectedThirdSchool;
        RefreshChildhoodAvailability(childhood, lateChildhood);
        childhood = SelectedChildhood;
        lateChildhood = SelectedLateChildhood;
        AffiliationDescription.Text = BuildAffiliationDescription(
            affiliation,
            birthAffiliation);
        ChildhoodDescription.Text = childhood?.Description ?? "";
        ChildhoodModuleCost.Text = childhood?.ModuleCost.ToString() ?? "";
        LateChildhoodDescription.Text = lateChildhood?.Description ?? "";
        LateChildhoodModuleCost.Text = lateChildhood?.ModuleCost.ToString() ?? "";
        UpdateCareerSummary();
        var subAffiliations = birthAffiliation is null
            ? Array.Empty<LifePathModule>()
            : EraAvailabilityCatalog.FilterSubAffiliations(
                birthAffiliation.Id,
                birthAffiliation.SubAffiliations ?? [],
                CurrentGameYear);
        var subAffiliationOptions = subAffiliations.Count == 0
            ? Array.Empty<SubAffiliationOption>()
            : new[] { new SubAffiliationOption("None", null) }
                .Concat(subAffiliations.Select(module =>
                    new SubAffiliationOption(module.Name, module)))
                .ToArray();
        SubAffiliationPicker.ItemsSource = subAffiliationOptions;
        SubAffiliationPanel.Visibility = subAffiliationOptions.Length > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (subAffiliation is not null &&
            subAffiliations.Any(module => module.Id == subAffiliation.Id))
        {
            SubAffiliationPicker.SelectedItem = subAffiliationOptions
                .First(option => option.Module?.Id == subAffiliation.Id);
        }
        else if (subAffiliationOptions.Length > 0)
        {
            SubAffiliationPicker.SelectedIndex = 0;
        }

        CastePicker.ItemsSource = birthAffiliation?.Castes ?? [];
        CastePanel.Visibility = CastePicker.Items.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (CastePicker.Items.Count > 0) CastePicker.SelectedIndex = 0;

        LanguagePicker.ItemsSource = affiliation?.Languages ?? [];
        if (LanguagePicker.Items.Count > 0) LanguagePicker.SelectedIndex = 0;

        RefreshEducationFields(school);
        RefreshSecondEducationFields(secondSchool);
        RefreshThirdEducationFields(thirdSchool);
        UpdateEducationSummary();
        BuildChoiceControls();
        RefreshCareerOptions();
        BuildChoiceControls();
        refreshing = false;
        UpdatePreview();
    }

    private void RefreshChildhoodAvailability(
        LifePathModule? previousChildhood,
        LifePathModule? previousLateChildhood)
    {
        var childhoods = AvailableChildhoods();
        ChildhoodPicker.ItemsSource = childhoods;
        SelectAvailableModule(ChildhoodPicker, childhoods, previousChildhood);

        var lateChildhoods = AvailableLateChildhoods();
        LateChildhoodPicker.ItemsSource = lateChildhoods;
        SelectAvailableModule(
            LateChildhoodPicker, lateChildhoods, previousLateChildhood);
    }

    private IReadOnlyList<LifePathModule> AvailableChildhoods()
    {
        return LifePathAvailability.FilterChildhoods(
            LifePathCatalog.Childhoods,
            SelectedAffiliationIsClan);
    }

    private IReadOnlyList<LifePathModule> AvailableLateChildhoods()
    {
        return LifePathAvailability.FilterLateChildhoods(
            LifePathCatalog.LateChildhoods,
            SelectedAffiliationIsClan);
    }

    private static void SelectAvailableModule(
        ComboBox picker,
        IReadOnlyList<LifePathModule> modules,
        LifePathModule? previous)
    {
        if (previous is not null &&
            modules.Any(module => module.Id == previous.Id))
        {
            picker.SelectedItem = modules.First(module => module.Id == previous.Id);
        }
        else
        {
            picker.SelectedIndex = modules.Count > 0 ? 0 : -1;
        }
    }

    private string BuildAffiliationDescription(
        LifePathModule? affiliation,
        LifePathModule? birthAffiliation)
    {
        var description = SelectedOrderAffiliation is null
            ? birthAffiliation?.Description ?? ""
            : $"{affiliation?.Description}{Environment.NewLine}{birthAffiliation?.Description}";
        if (affiliation is null)
        {
            return description;
        }

        var notes = new List<string>
        {
            EraAvailabilityCatalog.BuildModuleNote(affiliation, CurrentGameYear)
        };
        if (birthAffiliation is not null)
        {
            notes.Add(EraAvailabilityCatalog.BuildSubAffiliationSummary(
                birthAffiliation,
                CurrentGameYear));
        }
        if (SelectedSubAffiliation is { } subAffiliation)
        {
            notes.Add(EraAvailabilityCatalog.BuildSubAffiliationNote(
                birthAffiliation?.Id ?? affiliation.Id,
                subAffiliation,
                CurrentGameYear));
        }

        return $"{description}{Environment.NewLine}{Environment.NewLine}" +
            string.Join(Environment.NewLine, notes);
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

    private void EducationSelectionChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        if (EducationCheck.IsChecked == true && SchoolPicker.SelectedItem is null)
        {
            SchoolPicker.SelectedIndex = 0;
        }
        if (SecondEducationCheck.IsChecked == true &&
            SecondSchoolPicker.SelectedItem is null)
        {
            SecondSchoolPicker.SelectedIndex = 0;
        }
        if (ThirdEducationCheck.IsChecked == true &&
            ThirdSchoolPicker.SelectedItem is null)
        {
            ThirdSchoolPicker.SelectedIndex = 0;
        }
        RefreshModules();
    }

    private void RealLifeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        if (sender == RealLifePicker)
        {
            RefreshCareerOptions();
        }
        UpdateCareerSummary();
        BuildChoiceControls();
        UpdatePreview();
    }

    private void CareerToggleChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        refreshing = true;
        if (FirstCareerCheck.IsChecked == true &&
            RealLifePicker.SelectedItem is null)
        {
            RealLifePicker.SelectedIndex = 0;
        }
        if (FirstCareerCheck.IsChecked != true)
        {
            SecondCareerCheck.IsChecked = false;
        }
        RefreshCareerOptions();
        if (SecondCareerCheck.IsChecked == true &&
            SecondRealLifePicker.SelectedItem is null)
        {
            SecondRealLifePicker.SelectedIndex = 0;
        }
        refreshing = false;
        UpdateCareerSummary();
        BuildChoiceControls();
        UpdatePreview();
    }

    private void RefreshCareerOptions()
    {
        var firstPreviousId = (RealLifePicker.SelectedItem as LifePathModule)?.Id;
        var firstAvailability = EvaluateCareerAvailability(
            null, includeFreeXpAllocations: false, out var message);
        var firstOptions = firstAvailability
            .Select(item => item.Module)
            .ToArray();
        RealLifePicker.ItemsSource = firstOptions;
        RealLifePicker.SelectedItem = firstOptions
            .FirstOrDefault(module => module.Id == firstPreviousId);
        if (FirstCareerCheck.IsChecked == true &&
            RealLifePicker.SelectedItem is null &&
            firstOptions.Length > 0)
        {
            RealLifePicker.SelectedIndex = 0;
        }
        RefreshSecondCareerOptions();
        UpdateCareerAvailabilitySummary(firstAvailability, message);
    }

    private void RefreshSecondCareerOptions()
    {
        var previousId = (SecondRealLifePicker.SelectedItem as LifePathModule)?.Id;
        var firstCareer = SelectedRealLife;
        var secondAvailability = EvaluateCareerAvailability(
            firstCareer, includeFreeXpAllocations: false, out var message);
        var options = secondAvailability
            .Select(item => item.Module)
            .ToArray();
        SecondRealLifePicker.ItemsSource = options;
        SecondRealLifePicker.SelectedItem = options
            .FirstOrDefault(module => module.Id == previousId);
    }

    private IReadOnlyList<CareerAvailability> EvaluateCareerAvailability(
        LifePathModule? firstCareer,
        bool includeFreeXpAllocations,
        out string? message)
    {
        var candidates = LifePathCatalog.RealLifeModules
            .Where(module => firstCareer is null ||
                firstCareer.Repeatable ||
                module.Id != firstCareer.Id)
            .ToArray();
        try
        {
            var baseCharacter = BuildCharacter(4);
            if (includeFreeXpAllocations)
            {
                ApplyReviewFreeXp(baseCharacter);
            }
            if (firstCareer is not null)
            {
                ApplyCareerForAvailability(baseCharacter, firstCareer);
            }
            var baselineIssues = PrerequisiteRules.Evaluate(baseCharacter)
                .Select(IssueKey)
                .ToHashSet(StringComparer.Ordinal);
            message = null;
            return candidates
                .Select(module =>
                {
                    try
                    {
                        var probe = CloneCharacter(baseCharacter);
                        ApplyCareerForAvailability(probe, module);
                        var issues = PrerequisiteRules.Evaluate(probe)
                            .Where(issue => !baselineIssues.Contains(IssueKey(issue)))
                            .ToArray();
                        return new CareerAvailability(module, issues);
                    }
                    catch (InvalidOperationException)
                    {
                        return new CareerAvailability(module,
                            [new PrerequisiteIssue("Choice",
                                "Required field choices", 0, 0)]);
                    }
                })
                .ToArray();
        }
        catch (InvalidOperationException)
        {
            message = "Complete the earlier stage choices to filter career prerequisites.";
            return candidates
                .Select(module => new CareerAvailability(
                    module, Array.Empty<PrerequisiteIssue>()))
                .ToArray();
        }
    }

    private void ApplyCareerForAvailability(
        Character character,
        LifePathModule career)
    {
        var selection = CreateProbeSelection(career);
        LifePathEngine.ApplyStage4(character, selection);
        ApplyCareerState(character, career.Name);
        if (SelectedAffiliation is not null &&
            LanguagePicker.SelectedItem is string language)
        {
            LifePathEngine.ApplyAffiliationContext(
                character, SelectedAffiliation, career, language);
        }
    }

    private ModuleSelection CreateProbeSelection(LifePathModule module)
    {
        var choices = new Dictionary<string, IReadOnlyList<string>>();
        var allocations = new Dictionary<string, IReadOnlyList<ChoiceAllocation>>();
        foreach (var choice in module.Choices)
        {
            var options = ResolveChoiceOptions(choice);
            if (choice.Target == EffectTarget.Flexible &&
                !choice.FixedFlexibleSelections)
            {
                var educationOptions =
                    LifePathCatalog.ResolveEducationFieldSkills(
                        BuildCharacter(4), choice.EducationFieldNames ?? []);
                allocations[choice.Id] = CreateProbeFlexibleAllocations(
                    choice, options, educationOptions);
                choices[choice.Id] = [];
                continue;
            }
            if (choice.Target == EffectTarget.Flexible &&
                choice.FixedFlexibleSelections)
            {
                allocations[choice.Id] = options
                    .DefaultIfEmpty("Perception")
                    .Take(choice.Count)
                    .Select(name => new ChoiceAllocation(name, choice.Xp))
                    .ToArray();
                choices[choice.Id] = [];
                continue;
            }
            choices[choice.Id] = options
                .DefaultIfEmpty("Perception")
                .Take(choice.Count)
                .ToArray();
        }
        return new ModuleSelection(module, choices, allocations);
    }

    private static IReadOnlyList<ChoiceAllocation> CreateProbeFlexibleAllocations(
        ModuleChoice choice,
        IReadOnlyList<string> options,
        IReadOnlyList<string> educationOptions)
    {
        var allocations = new List<ChoiceAllocation>();
        var remaining = choice.Xp * choice.Count;
        if (choice.MinimumEducationFieldSkillXp > 0 &&
            educationOptions.FirstOrDefault() is { } educationTarget)
        {
            var xp = Math.Min(remaining, choice.MinimumEducationFieldSkillXp);
            allocations.Add(new ChoiceAllocation(educationTarget, xp));
            remaining -= xp;
        }
        if (choice.MinimumAttributeOrTraitXp > 0)
        {
            var target = options.FirstOrDefault(option =>
                LifePathEngine.ClassifyFlexibleTarget(option) is
                    EffectTarget.Trait or EffectTarget.Attribute);
            if (target is not null)
            {
                var xp = Math.Min(remaining, choice.MinimumAttributeOrTraitXp);
                allocations.Add(new ChoiceAllocation(target, xp));
                remaining -= xp;
            }
        }
        foreach (var target in options.Where(option => option.Length > 0))
        {
            if (remaining <= 0) break;
            var maximum = LifePathEngine.ClassifyFlexibleTarget(target) switch
            {
                EffectTarget.Attribute => choice.AttributeMaximumXp,
                EffectTarget.Trait => choice.TraitMaximumXp,
                EffectTarget.Skill => choice.SkillMaximumXp,
                _ => null
            };
            var current = allocations
                .Where(allocation => allocation.Name == target)
                .Sum(allocation => allocation.Xp);
            var room = maximum is int limit
                ? Math.Max(0, limit - current)
                : remaining;
            var xp = Math.Min(remaining, room);
            if (xp <= 0) continue;
            allocations.Add(new ChoiceAllocation(target, xp));
            remaining -= xp;
        }
        if (remaining > 0 && options.FirstOrDefault() is { } fallback)
        {
            allocations.Add(new ChoiceAllocation(fallback, remaining));
        }
        return allocations
            .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
            .Select(group => new ChoiceAllocation(
                group.Key, group.Sum(allocation => allocation.Xp)))
            .ToArray();
    }

    private static Character CloneCharacter(Character source)
    {
        var clone = new Character
        {
            Name = source.Name,
            Affiliation = source.Affiliation,
            SubAffiliation = source.SubAffiliation,
            BirthAffiliation = source.BirthAffiliation,
            BirthSubAffiliation = source.BirthSubAffiliation,
            ClanCaste = source.ClanCaste,
            ClanTrainingField = source.ClanTrainingField,
            EarlyChildhood = source.EarlyChildhood,
            LateChildhood = source.LateChildhood,
            School = source.School,
            BasicSchool = source.BasicSchool,
            AdvancedSchool = source.AdvancedSchool,
            SpecialSchool = source.SpecialSchool,
            RealLife = source.RealLife,
            Phenotype = source.Phenotype,
            HomePlanet = source.HomePlanet,
            Sex = source.Sex,
            BirthYear = source.BirthYear,
            GameYear = source.GameYear,
            HairColor = source.HairColor,
            EyeColor = source.EyeColor,
            Height = source.Height,
            Weight = source.Weight,
            GmXpModifier = source.GmXpModifier,
            CBillModifier = source.CBillModifier,
            Notes = source.Notes
        };
        CopyValues(source.Attributes, clone.Attributes);
        CopyValues(source.Skills, clone.Skills);
        CopyValues(source.Traits, clone.Traits);
        CopyValues(source.PreAttributes, clone.PreAttributes);
        CopyValues(source.PreSkills, clone.PreSkills);
        CopyValues(source.PreTraits, clone.PreTraits);
        foreach (var education in source.EducationHistory) clone.EducationHistory.Add(education);
        foreach (var field in source.EducationFields) clone.EducationFields.Add(field);
        foreach (var career in source.RealLifeHistory) clone.RealLifeHistory.Add(career);
        foreach (var item in source.Equipment) clone.Equipment.Add(item);
        foreach (var item in source.Weapons) clone.Weapons.Add(item);
        foreach (var weapon in source.EquippedWeapons) clone.EquippedWeapons.Add(weapon);
        foreach (var location in source.EquipmentLocations)
        {
            clone.EquipmentLocations[location.Key] = location.Value;
        }
        return clone;
    }

    private static void CopyValues(
        IEnumerable<NamedValue> source,
        ICollection<NamedValue> target)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(new NamedValue(item.Name, item.Value));
        }
    }

    private void UpdateCareerAvailabilitySummary(
        IReadOnlyList<CareerAvailability> availability,
        string? message)
    {
        if (message is not null)
        {
            CareerAvailabilitySummary.Text = message;
            return;
        }
        var availableCount = availability.Count(item => item.Issues.Count == 0);
        var blocked = availability.Where(item => item.Issues.Count > 0).ToArray();
        if (blocked.Length == 0)
        {
            CareerAvailabilitySummary.Text =
                $"{availableCount} career module(s) available.";
            CareerAvailabilitySummary.ToolTip = null;
            return;
        }
        var blockers = blocked
            .SelectMany(item => item.Issues.Select(issue =>
                $"{issue.Category}: {issue.Name}"))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        CareerAvailabilitySummary.Text =
            $"{availableCount} career module(s) available; " +
            $"{blocked.Length} need prerequisites. " +
            $"First unmet: {blockers.FirstOrDefault() ?? "None"}.";
        CareerAvailabilitySummary.ToolTip = string.Join(Environment.NewLine,
            blocked
                .SelectMany(item => item.Issues.Select(issue =>
                    $"{item.Module.Name}: {issue.Category} {issue.Name}"))
                .Distinct(StringComparer.Ordinal)
                .Take(12));
    }

    private void UpdateCareerSummary()
    {
        var first = SelectedRealLife;
        var second = SelectedSecondRealLife;
        RealLifeDescription.Text = first is null
            ? "No career selected."
            : $"{first.Name} ({first.TimeYears} years){Environment.NewLine}{first.Description}";
        SecondRealLifeDescription.Text = second is null
            ? ""
            : $"{second.Name} ({second.TimeYears} years){Environment.NewLine}{second.Description}";
        CareerModuleCost.Text = new[] { first, second }
            .Where(module => module is not null)
            .Sum(module => module!.ModuleCost)
            .ToString();
        CareerYears.Text = new[] { first, second }
            .Where(module => module is not null)
            .Sum(module => module!.TimeYears)
            .ToString();
    }

    private void RefreshEducationFields(LifePathModule? school)
    {
        SetDependentPicker(BasicFieldPicker, BasicFieldPanel, school?.BasicFields, true);
        SetDependentPicker(AdvancedFieldPicker, AdvancedFieldPanel, school?.AdvancedFields, false);
        RefreshThirdField(school);
    }

    private void RefreshSecondEducationFields(LifePathModule? school)
    {
        SetDependentPicker(SecondBasicFieldPicker, SecondBasicFieldPanel,
            school?.BasicFields, true);
        SetDependentPicker(SecondAdvancedFieldPicker, SecondAdvancedFieldPanel,
            school?.AdvancedFields, false);
        RefreshSecondThirdField(school);
    }

    private void RefreshThirdEducationFields(LifePathModule? school)
    {
        SetDependentPicker(ThirdBasicFieldPicker, ThirdBasicFieldPanel,
            school?.BasicFields, true);
        SetDependentPicker(ThirdAdvancedFieldPicker, ThirdAdvancedFieldPanel,
            school?.AdvancedFields, false);
        RefreshThirdEducationThirdField(school);
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

    private void RefreshSecondThirdField(LifePathModule? school)
    {
        var fields = SelectedSecondAdvancedField is null
            ? []
            : (school?.AdvancedFields ?? [])
                .Where(field => field.Id != SelectedSecondAdvancedField.Id)
                .Concat(school?.SpecialistFields ?? [])
                .ToArray();
        SetDependentPicker(SecondSpecialistFieldPicker,
            SecondSpecialistFieldPanel, fields, false);
    }

    private void RefreshThirdEducationThirdField(LifePathModule? school)
    {
        var fields = SelectedThirdAdvancedField is null
            ? []
            : (school?.AdvancedFields ?? [])
                .Where(field => field.Id != SelectedThirdAdvancedField.Id)
                .Concat(school?.SpecialistFields ?? [])
                .ToArray();
        SetDependentPicker(ThirdSpecialistFieldPicker,
            ThirdSpecialistFieldPanel, fields, false);
    }

    private void UpdateEducationSummary()
    {
        var school = SelectedSchool;
        var secondSchool = SelectedSecondSchool;
        var thirdSchool = SelectedThirdSchool;
        SchoolDescription.Text = string.Join(Environment.NewLine + Environment.NewLine,
            new[] { school, secondSchool, thirdSchool }
                .Where(module => module is not null)
                .Select((module, index) =>
                    $"Education {index + 1}: {module!.Name} ({module.TimeYears} years){Environment.NewLine}{module.Description}"));
        if (SchoolDescription.Text.Length == 0)
        {
            SchoolDescription.Text =
            "No formal education selected.";
        }
        var fields = EducationSummaryEntries().ToArray();
        EducationFieldsSummary.Text = string.Join(Environment.NewLine, fields);
        EducationModuleCost.Text = SelectedEducationModules()
            .Sum(module => module.ModuleCost)
            .ToString();
    }

    private IEnumerable<string> EducationSummaryEntries()
    {
        return new[]
            {
                ("Education 1 basic field", SelectedBasicField),
                ("Education 1 advanced field", SelectedAdvancedField),
                ("Education 1 third field", SelectedSpecialistField),
                ("Education 2 basic field", SelectedSecondBasicField),
                ("Education 2 advanced field", SelectedSecondAdvancedField),
                ("Education 2 third field", SelectedSecondSpecialistField),
                ("Education 3 basic field", SelectedThirdBasicField),
                ("Education 3 advanced field", SelectedThirdAdvancedField),
                ("Education 3 third field", SelectedThirdSpecialistField)
            }
            .Where(entry => entry.Item2 is not null)
            .Select(entry => $"{entry.Item1}: {entry.Item2!.Name}");
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
            CaptureFlexibleAllocations();
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
                        var key = Key(selectedModule, choice);
                        var defaults = flexibleAllocationCache
                            .TryGetValue(key, out var cachedAllocations) &&
                            cachedAllocations.Count > 0
                            ? cachedAllocations
                            : CreateDefaultFlexibleAllocations(
                                choice, options, educationOptions);
                        var allocator = new Border
                        {
                            Background = new SolidColorBrush(
                                Color.FromRgb(246, 245, 241)),
                            BorderBrush = new SolidColorBrush(
                                Color.FromRgb(191, 189, 181)),
                            BorderThickness = new Thickness(1),
                            Padding = new Thickness(12),
                            Margin = new Thickness(0, 4, 10, 12)
                        };
                        var allocatorPanel = new StackPanel();
                        allocator.Child = allocatorPanel;

                        var summary = new Grid();
                        summary.ColumnDefinitions.Add(new ColumnDefinition());
                        summary.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(90)
                        });
                        summary.Children.Add(new TextBlock
                        {
                            Text = "ALLOCATE FLEXIBLE XP",
                            FontFamily = new FontFamily("Montserrat"),
                            FontWeight = FontWeights.Black,
                            FontSize = 14,
                            VerticalAlignment = VerticalAlignment.Center
                        });
                        var remaining = new TextBlock
                        {
                            Text = totalXp.ToString(),
                            FontFamily = new FontFamily("Montserrat"),
                            FontWeight = FontWeights.Black,
                            FontSize = 20,
                            HorizontalAlignment = HorizontalAlignment.Right
                        };
                        var remainingPanel = new StackPanel
                        {
                            HorizontalAlignment = HorizontalAlignment.Right
                        };
                        remainingPanel.Children.Add(new TextBlock
                        {
                            Text = "REMAINING",
                            FontSize = 10,
                            HorizontalAlignment = HorizontalAlignment.Right
                        });
                        remainingPanel.Children.Add(remaining);
                        Grid.SetColumn(remainingPanel, 1);
                        summary.Children.Add(remainingPanel);
                        allocatorPanel.Children.Add(summary);

                        var rules = DescribeFlexibleRestrictions(choice);
                        if (rules.Length > 0)
                        {
                            allocatorPanel.Children.Add(new TextBlock
                            {
                                Text = rules,
                                TextWrapping = TextWrapping.Wrap,
                                Foreground = new SolidColorBrush(
                                    Color.FromRgb(82, 82, 78)),
                                FontSize = 11,
                                Margin = new Thickness(0, 5, 0, 8)
                            });
                        }

                        var columnHeader = new Grid
                        {
                            Margin = new Thickness(0, 2, 0, 2)
                        };
                        columnHeader.ColumnDefinitions.Add(new ColumnDefinition());
                        columnHeader.ColumnDefinitions.Add(new ColumnDefinition
                        {
                            Width = new GridLength(116)
                        });
                        columnHeader.Children.Add(new TextBlock
                        {
                            Text = "TARGET",
                            FontSize = 10,
                            FontWeight = FontWeights.SemiBold
                        });
                        var xpHeader = new TextBlock
                        {
                            Text = "XP",
                            FontSize = 10,
                            FontWeight = FontWeights.SemiBold,
                            HorizontalAlignment = HorizontalAlignment.Center
                        };
                        Grid.SetColumn(xpHeader, 1);
                        columnHeader.Children.Add(xpHeader);
                        allocatorPanel.Children.Add(columnHeader);

                        var rowHost = new StackPanel();
                        allocatorPanel.Children.Add(rowHost);
                        for (var i = 0; i < defaults.Count; i++)
                        {
                            AddFlexibleAllocationRow(
                                rowHost, options, controls, amounts,
                                defaults[i].Name, defaults[i].Xp);
                        }
                        var addTarget = new Button
                        {
                            Content = "Add target",
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(0, 6, 0, 0),
                            Padding = new Thickness(10, 3, 10, 3),
                            Tag = new FlexibleRowRequest(
                                rowHost, options, controls, amounts)
                        };
                        addTarget.Click += FlexibleRowAdd_Click;
                        allocatorPanel.Children.Add(addTarget);
                        var status = new TextBlock
                        {
                            TextWrapping = TextWrapping.Wrap,
                            FontWeight = FontWeights.SemiBold,
                            FontSize = 11,
                            Margin = new Thickness(0, 8, 0, 0)
                        };
                        allocatorPanel.Children.Add(status);
                        modulePanel.Children.Add(allocator);
                        var input = new ChoiceInput(
                            controls, amounts, choice, module.Name,
                            ChoiceStep(selectedModule), remaining, status,
                            options);
                        choiceControls[key] = input;
                        RefreshFlexiblePickerOptions(input);
                        UpdateFlexibleChoiceStatus(input);
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
                        choice.Target == EffectTarget.Flexible &&
                        choice.FixedFlexibleSelections
                            ? new ChoiceInput(
                                controls,
                                Choice: choice,
                                ModuleName: module.Name,
                                Step: ChoiceStep(selectedModule))
                            : new ChoiceInput(controls);
                }
            }
        }
        finally
        {
            refreshing = wasRefreshing;
            UpdateChoiceGroupVisibility();
        }
    }

    private void CaptureFlexibleAllocations()
    {
        foreach (var entry in choiceControls)
        {
            var input = entry.Value;
            if (input.Choice?.Target != EffectTarget.Flexible ||
                input.Choice.FixedFlexibleSelections ||
                input.Amounts is null)
            {
                continue;
            }
            flexibleAllocationCache[entry.Key] = input.Pickers
                .Zip(input.Amounts)
                .Select(pair => new ChoiceAllocation(
                    pair.First.SelectedItem as string ?? "",
                    int.TryParse(pair.Second.Text, out var xp) ? xp : 0))
                .Where(allocation =>
                    allocation.Name.Length > 0 || allocation.Xp > 0)
                .ToArray();
        }
    }

    private int ChoiceStep(SelectedModule selectedModule)
    {
        if (selectedModule.IsStage4) return 5;
        if (selectedModule.Module == SelectedChildhood) return 2;
        if (selectedModule.Module == SelectedLateChildhood) return 3;
        if (SelectedEducationModules().Any(module =>
                module.Id == selectedModule.Module.Id))
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
        if (IsLoaded && !refreshing)
        {
            UpdateFlexibleChoiceDisplays();
            UpdatePreview();
        }
    }

    private void ChoiceAmountChanged(object sender, TextChangedEventArgs e)
    {
        if (IsLoaded && !refreshing)
        {
            UpdateFlexibleChoiceDisplays();
            UpdatePreview();
        }
    }

    private void FlexibleXpAdjust_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: XpAdjustment adjustment }) return;
        var current = int.TryParse(adjustment.Input.Text, out var value)
            ? value
            : 0;
        adjustment.Input.Text = Math.Max(0, current + adjustment.Delta).ToString();
        adjustment.Input.Focus();
        adjustment.Input.SelectAll();
    }

    private void FlexibleRowAdd_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FlexibleRowRequest request }) return;
        AddFlexibleAllocationRow(
            request.RowHost,
            request.Options,
            request.Pickers,
            request.Amounts,
            "",
            0);
        UpdateFlexibleChoiceDisplays();
        UpdatePreview();
    }

    private void AddFlexibleAllocationRow(
        Panel rowHost,
        IReadOnlyList<string> options,
        List<ComboBox> controls,
        List<TextBox> amounts,
        string selectedName,
        int xp)
    {
        var row = new Grid { Margin = new Thickness(0, 2, 0, 2) };
        row.ColumnDefinitions.Add(new ColumnDefinition());
        row.ColumnDefinitions.Add(new ColumnDefinition
        {
            Width = new GridLength(116)
        });
        var picker = new ComboBox
        {
            ItemsSource = options,
            Margin = new Thickness(0, 0, 6, 0)
        };
        picker.SelectionChanged += ChoiceSelectionChanged;
        picker.SelectedItem = selectedName;
        var amount = new TextBox
        {
            Text = xp.ToString(),
            Width = 48,
            Margin = new Thickness(2, 0, 2, 0),
            HorizontalContentAlignment = HorizontalAlignment.Right
        };
        var xpControls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var subtract = new Button
        {
            Content = "−",
            Width = 27,
            Height = 27,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "Remove 5 XP",
            Tag = new XpAdjustment(amount, -5)
        };
        subtract.Click += FlexibleXpAdjust_Click;
        var add = new Button
        {
            Content = "+",
            Width = 27,
            Height = 27,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            ToolTip = "Add 5 XP",
            Tag = new XpAdjustment(amount, 5)
        };
        add.Click += FlexibleXpAdjust_Click;
        xpControls.Children.Add(subtract);
        xpControls.Children.Add(amount);
        xpControls.Children.Add(add);
        Grid.SetColumn(xpControls, 1);
        row.Children.Add(picker);
        row.Children.Add(xpControls);
        amount.TextChanged += ChoiceAmountChanged;
        controls.Add(picker);
        amounts.Add(amount);
        rowHost.Children.Add(row);
    }

    private void UpdateFlexibleChoiceDisplays()
    {
        foreach (var input in choiceControls.Values.Where(
                     input => input.Choice is not null))
        {
            RefreshFlexiblePickerOptions(input);
            UpdateFlexibleChoiceStatus(input);
        }
    }

    private void RefreshFlexiblePickerOptions(ChoiceInput input)
    {
        if (input.Choice?.Target != EffectTarget.Flexible ||
            input.Choice.FixedFlexibleSelections ||
            input.Options is null)
        {
            return;
        }

        var selectedTargets = input.Pickers
            .Select(picker => picker.SelectedItem as string ?? "")
            .Where(name => name.Length > 0)
            .ToArray();
        var wasRefreshing = refreshing;
        refreshing = true;
        try
        {
            foreach (var picker in input.Pickers)
            {
                var current = picker.SelectedItem as string ?? "";
                var available = input.Options
                    .Where(option => option == current ||
                        !selectedTargets.Contains(option, StringComparer.Ordinal))
                    .ToArray();
                picker.ItemsSource = available;
                picker.SelectedItem = current.Length > 0 &&
                    available.Contains(current, StringComparer.Ordinal)
                        ? current
                        : null;
            }
        }
        finally
        {
            refreshing = wasRefreshing;
        }
    }

    private void UpdateFlexibleChoiceStatus(ChoiceInput input)
    {
        var result = EvaluateFlexibleChoice(input);
        if (input.Remaining is not null)
        {
            input.Remaining.Text = result.Remaining.ToString();
            input.Remaining.Foreground = result.Remaining == 0
                ? Brushes.DarkGreen
                : result.Remaining < 0
                    ? Brushes.Firebrick
                    : Brushes.DarkGoldenrod;
        }
        if (input.Status is not null)
        {
            input.Status.Text = result.IsValid
                ? "Ready. The complete pool is allocated."
                : result.Message;
            input.Status.Foreground = result.IsValid
                ? Brushes.DarkGreen
                : Brushes.Firebrick;
        }
    }

    private FlexibleChoiceResult EvaluateFlexibleChoice(ChoiceInput input)
    {
        var choice = input.Choice!;
        var requiredXp = choice.Xp * choice.Count;
        if (choice.FixedFlexibleSelections)
        {
            var selected = input.Pickers
                .Select(picker => picker.SelectedItem as string ?? "")
                .Where(name => name.Length > 0)
                .ToArray();
            var duplicateFixedTarget = selected
                .GroupBy(name => name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Count() > 1);
            if (duplicateFixedTarget is not null)
            {
                return new FlexibleChoiceResult(false, requiredXp,
                    $"{input.ModuleName}: choose each flexible XP target only once.");
            }

            var missing = choice.Count - selected.Length;
            if (missing > 0)
            {
                var targetWord = missing == 1 ? "target" : "targets";
                return new FlexibleChoiceResult(false, missing * choice.Xp,
                    $"{input.ModuleName}: choose {missing} more flexible XP " +
                    $"{targetWord} before continuing.");
            }

            return new FlexibleChoiceResult(true, 0, "");
        }

        var allocations = input.Pickers.Zip(input.Amounts ?? [])
            .Select(pair => new
            {
                Name = pair.First.SelectedItem as string ?? "",
                ValidXp = int.TryParse(pair.Second.Text, out var xp),
                Xp = int.TryParse(pair.Second.Text, out xp) ? xp : 0
            })
            .ToArray();
        if (allocations.Any(item => !item.ValidXp || item.Xp < 0))
        {
            return new FlexibleChoiceResult(false, requiredXp,
                $"{input.ModuleName}: enter a non-negative XP amount.");
        }
        if (allocations.Any(item => item.Xp > 0 && item.Name.Length == 0))
        {
            return new FlexibleChoiceResult(false,
                requiredXp - allocations.Sum(item => item.Xp),
                $"{input.ModuleName}: choose a target for every XP amount.");
        }

        var active = allocations.Where(item => item.Xp > 0).ToArray();
        var duplicateTarget = active
            .GroupBy(item => item.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateTarget is not null)
        {
            return new FlexibleChoiceResult(false, requiredXp,
                $"{input.ModuleName}: choose each flexible XP target only once.");
        }

        var spent = active.Sum(item => item.Xp);
        var remaining = requiredXp - spent;
        if (remaining != 0)
        {
            var direction = remaining > 0 ? "still needs" : "is over by";
            return new FlexibleChoiceResult(false, remaining,
                $"{input.ModuleName}: this pool {direction} {Math.Abs(remaining)} XP.");
        }

        var attributeOrTraitXp = active
            .Where(item => LifePathEngine.ClassifyFlexibleTarget(item.Name) is
                EffectTarget.Attribute or EffectTarget.Trait)
            .Sum(item => item.Xp);
        if (attributeOrTraitXp < choice.MinimumAttributeOrTraitXp)
        {
            return new FlexibleChoiceResult(false, 0,
                $"{input.ModuleName}: assign at least " +
                $"{choice.MinimumAttributeOrTraitXp} XP to Attributes or Traits.");
        }

        if (choice.AttributeMaximumXp is int maximum)
        {
            var excessiveAttribute = active
                .Where(item => LifePathEngine.ClassifyFlexibleTarget(item.Name) ==
                    EffectTarget.Attribute)
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Sum(item => item.Xp) > maximum);
            if (excessiveAttribute is not null)
            {
                return new FlexibleChoiceResult(false, 0,
                    $"{input.ModuleName}: no Attribute may receive more than " +
                    $"{maximum} XP from this pool.");
            }
        }
        if (choice.TraitMaximumXp is int traitMaximum)
        {
            var excessiveTrait = active
                .Where(item => LifePathEngine.ClassifyFlexibleTarget(item.Name) ==
                    EffectTarget.Trait)
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Sum(item => item.Xp) > traitMaximum);
            if (excessiveTrait is not null)
            {
                return new FlexibleChoiceResult(false, 0,
                    $"{input.ModuleName}: no Trait may receive more than " +
                    $"{traitMaximum} XP from this pool.");
            }
        }
        if (choice.SkillMaximumXp is int skillMaximum)
        {
            var excessiveSkill = active
                .Where(item => LifePathEngine.ClassifyFlexibleTarget(item.Name) ==
                    EffectTarget.Skill)
                .GroupBy(item => item.Name, StringComparer.Ordinal)
                .FirstOrDefault(group => group.Sum(item => item.Xp) > skillMaximum);
            if (excessiveSkill is not null)
            {
                return new FlexibleChoiceResult(false, 0,
                    $"{input.ModuleName}: no Skill may receive more than " +
                    $"{skillMaximum} XP from this pool.");
            }
        }

        var educationOptions = ResolveEducationFieldOptions(choice);
        var educationAllocations = active.Where(item =>
            educationOptions.Contains(item.Name, StringComparer.Ordinal)).ToArray();
        if (educationAllocations.Sum(item => item.Xp) <
            choice.MinimumEducationFieldSkillXp)
        {
            return new FlexibleChoiceResult(false, 0,
                $"{input.ModuleName}: assign at least " +
                $"{choice.MinimumEducationFieldSkillXp} XP to selected Field skills.");
        }
        if (educationAllocations.Select(item => item.Name)
                .Distinct(StringComparer.Ordinal).Count() >
            choice.MaximumEducationFieldSkillTargets)
        {
            return new FlexibleChoiceResult(false, 0,
                $"{input.ModuleName}: use no more than " +
                $"{choice.MaximumEducationFieldSkillTargets} Field skill targets.");
        }

        return new FlexibleChoiceResult(true, 0, "");
    }

    private static string DescribeFlexibleRestrictions(ModuleChoice choice)
    {
        var rules = new List<string>
        {
            $"Divide all {choice.Xp * choice.Count} XP among the eligible targets."
        };
        if (choice.MinimumAttributeOrTraitXp > 0)
        {
            rules.Add($"At least {choice.MinimumAttributeOrTraitXp} XP must go to Attributes or Traits.");
        }
        if (choice.AttributeMaximumXp is int maximum)
        {
            rules.Add($"Maximum {maximum} XP per Attribute.");
        }
        if (choice.TraitMaximumXp is int traitMaximum)
        {
            rules.Add($"Maximum {traitMaximum} XP per Trait.");
        }
        if (choice.SkillMaximumXp is int skillMaximum)
        {
            rules.Add($"Maximum {skillMaximum} XP per Skill.");
        }
        if (choice.MinimumEducationFieldSkillXp > 0)
        {
            rules.Add($"At least {choice.MinimumEducationFieldSkillXp} XP must go to selected Field skills.");
        }
        if (choice.MaximumEducationFieldSkillTargets < int.MaxValue)
        {
            rules.Add($"Use no more than {choice.MaximumEducationFieldSkillTargets} Field skill targets.");
        }
        return string.Join(" ", rules);
    }

    private static IReadOnlyList<ChoiceAllocation> CreateDefaultFlexibleAllocations(
        ModuleChoice choice,
        IReadOnlyList<string> options,
        IReadOnlyList<string> educationOptions)
    {
        var allocations = new List<ChoiceAllocation>();
        var remaining = choice.Xp * choice.Count;

        if (choice.MinimumEducationFieldSkillXp > 0 &&
            educationOptions.FirstOrDefault() is { } educationTarget)
        {
            var xp = Math.Min(remaining, choice.MinimumEducationFieldSkillXp);
            allocations.Add(new ChoiceAllocation(educationTarget, xp));
            remaining -= xp;
        }

        if (choice.MinimumAttributeOrTraitXp > 0)
        {
            var alreadyAllocated = allocations
                .Where(item => LifePathEngine.ClassifyFlexibleTarget(item.Name) is
                    EffectTarget.Attribute or EffectTarget.Trait)
                .Sum(item => item.Xp);
            var needed = Math.Min(
                remaining,
                Math.Max(0, choice.MinimumAttributeOrTraitXp - alreadyAllocated));
            if (needed > 0)
            {
                var target = options.FirstOrDefault(option =>
                    LifePathEngine.ClassifyFlexibleTarget(option) ==
                    EffectTarget.Trait);
                target ??= options.FirstOrDefault(option =>
                    LifePathEngine.ClassifyFlexibleTarget(option) ==
                    EffectTarget.Attribute);
                if (target is not null)
                {
                    allocations.Add(new ChoiceAllocation(target, needed));
                    remaining -= needed;
                }
            }
        }

        allocations = allocations
            .Where(allocation => allocation.Xp > 0)
            .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
            .Select(group => new ChoiceAllocation(
                group.Key, group.Sum(allocation => allocation.Xp)))
            .ToList();
        var rowCount = Math.Max(choice.Count, allocations.Count);
        while (allocations.Count < rowCount)
        {
            allocations.Add(new ChoiceAllocation("", 0));
        }
        return allocations.Take(rowCount).ToArray();
    }

    private sealed record FlexibleChoiceResult(
        bool IsValid,
        int Remaining,
        string Message);

    private IReadOnlyList<string> ResolveChoiceOptions(ModuleChoice choice)
    {
        if (choice.SolarisInternshipFieldSkillsOnly)
        {
            var internshipSkills =
                LifePathCatalog.ResolveSolarisInternshipFieldSkills(
                    BuildEducationContextCharacter());
            return internshipSkills.Count == 0
                ? SortChoiceOptions(choice, choice.Options)
                : SortChoiceOptions(choice, internshipSkills);
        }
        if (choice.SelectedEducationFieldSkillsOnly)
        {
            var educationContext = BuildEducationContextCharacter();
            var selectedFieldSkills =
                LifePathCatalog.ResolveSelectedEducationFieldSkills(
                    educationContext);
            return selectedFieldSkills.Count == 0
                ? SortChoiceOptions(
                    choice,
                    LifePathCatalog.FilterEraAvailableSkillOptions(
                        educationContext,
                        choice.Options))
                : SortChoiceOptions(choice, selectedFieldSkills);
        }
        var context = BuildEducationContextCharacter();
        var options = LifePathCatalog.FilterEraAvailableSkillOptions(
                context,
                choice.Options)
            .AsEnumerable();
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
        return SortChoiceOptions(choice, options);
    }

    private static IReadOnlyList<string> SortChoiceOptions(
        ModuleChoice choice,
        IEnumerable<string> options)
    {
        if (choice.Target != EffectTarget.Flexible)
        {
            return options
                .Distinct(StringComparer.Ordinal)
                .OrderBy(option => option)
                .ToArray();
        }
        return options
            .Distinct(StringComparer.Ordinal)
            .OrderBy(FlexibleOptionGroup)
            .ThenBy(AttributeSortOrder)
            .ThenBy(option => option)
            .ToArray();
    }

    private static int FlexibleOptionGroup(string option) =>
        LifePathEngine.ClassifyFlexibleTarget(option) switch
        {
            EffectTarget.Attribute => 0,
            EffectTarget.Trait => 1,
            _ => 2
        };

    private static int AttributeSortOrder(string option) =>
        option switch
        {
            "STR" => 0,
            "BOD" => 1,
            "RFL" => 2,
            "DEX" => 3,
            "INT" => 4,
            "WIL" => 5,
            "CHA" => 6,
            "EDG" => 7,
            _ => 99
        };

    private IReadOnlyList<string> ResolveEducationFieldOptions(ModuleChoice choice)
    {
        if (choice.EducationFieldNames is null) return [];
        var context = BuildEducationContextCharacter();
        var selectedFieldSkills = LifePathCatalog.ResolveEducationFieldSkills(
            context,
            choice.EducationFieldNames);
        return selectedFieldSkills.Count > 0
            ? selectedFieldSkills
            : LifePathCatalog.ResolveEducationFieldSkillPool(
                context,
                choice.EducationFieldNames);
    }

    private Character BuildEducationContextCharacter()
    {
        var character = new Character
        {
            School = SelectedSchool?.Name ?? "",
            Affiliation = SelectedAffiliation?.Name ?? "",
            BasicSchool = SelectedBasicField?.Name ?? "",
            AdvancedSchool = SelectedAdvancedField?.Name ?? "",
            SpecialSchool = SelectedSpecialistField?.Name ?? "",
            GameYear = CurrentGameYear
        };
        foreach (var school in SelectedEducationSchools())
        {
            character.EducationHistory.Add(school.Name);
        }
        foreach (var field in SelectedEducationFields())
        {
            character.EducationFields.Add(field.Name);
        }
        return character;
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

    private void SelectModuleChoice(
        LifePathModule module,
        string choiceId,
        string option)
    {
        var choice = module.Choices.First(item => item.Id == choiceId);
        if (!choiceControls.TryGetValue(
                Key(module, choice, 0), out var input))
        {
            throw new InvalidOperationException(
                $"{module.Name}: {choice.Label} is not available.");
        }
        input.Pickers[0].SelectedItem = option;
    }

    private void DependentSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        refreshing = true;
        BuildChoiceControls();
        refreshing = false;
        UpdateEducationSummary();
        UpdatePreview();
    }

    private void AdvancedFieldSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        refreshing = true;
        if (sender == ThirdAdvancedFieldPicker)
        {
            RefreshThirdEducationThirdField(SelectedThirdSchool);
        }
        else if (sender == SecondAdvancedFieldPicker)
        {
            RefreshSecondThirdField(SelectedSecondSchool);
        }
        else
        {
            RefreshThirdField(SelectedSchool);
        }
        BuildChoiceControls();
        refreshing = false;
        UpdateEducationSummary();
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        UpdateFlexibleChoiceDisplays();
        try
        {
            var character = BuildCharacter(
                currentStep,
                allowIncompleteCurrentStepPreview: true);
            var summary = CharacterRules.Calculate(character);
            var moduleCost = LifePathEngine.CalculateModuleCost(
                character, SelectedModules(currentStep));
            var moduleRemainingXp = LifePathEngine.StartingXp - moduleCost;
            PreviewAttributes.ItemsSource = character.Attributes;
            PreviewSkills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            PreviewTraits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            Stage0Attributes.ItemsSource = character.Attributes;
            Stage0Skills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            Stage0Traits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ReviewAttributes.ItemsSource = character.Attributes;
            ReviewSkills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            ReviewTraits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ModuleCost.Text = moduleCost.ToString();
            SpentXp.Text = summary.SpentXp.ToString();
            FreeXp.Text = summary.FreeXp.ToString();
            FreeXpModuleCost.Text = ModuleCost.Text;
            FreeXpSpentXp.Text = SpentXp.Text;
            FreeXpRemainingXp.Text = FreeXp.Text;
            RunningFreeXp.Text = moduleRemainingXp.ToString();
            if (TotalsHost.Visibility == Visibility.Visible)
            {
                lastTotalsCharacter = character;
                lastRunningFreeXp = RunningFreeXp.Text;
            }
            UpdateReview(character);
        }
        catch (InvalidOperationException)
        {
            var restoredReview = false;
            var canRestoreTotals = currentStep != Stage4Step;
            if (canRestoreTotals &&
                TotalsHost.Visibility == Visibility.Visible &&
                lastTotalsCharacter is not null)
            {
                PreviewAttributes.ItemsSource = lastTotalsCharacter.Attributes;
                PreviewSkills.ItemsSource = lastTotalsCharacter.Skills
                    .OrderBy(item => item.Name);
                PreviewTraits.ItemsSource = lastTotalsCharacter.Traits
                    .OrderBy(item => item.Name);
                RunningFreeXp.Text = lastRunningFreeXp;
            }
            if (currentStep == pages.Length - 1 &&
                lastTotalsCharacter is not null)
            {
                ReviewAttributes.ItemsSource = lastTotalsCharacter.Attributes;
                ReviewSkills.ItemsSource = lastTotalsCharacter.Skills
                    .OrderBy(item => item.Name);
                ReviewTraits.ItemsSource = lastTotalsCharacter.Traits
                    .OrderBy(item => item.Name);
                UpdateReview(lastTotalsCharacter);
                restoredReview = true;
            }
            if (TotalsHost.Visibility != Visibility.Visible || !canRestoreTotals)
            {
                PreviewAttributes.ItemsSource = null;
                PreviewSkills.ItemsSource = null;
                PreviewTraits.ItemsSource = null;
            }
            if (currentStep == 1)
            {
                Stage0Attributes.ItemsSource = null;
                Stage0Skills.ItemsSource = null;
                Stage0Traits.ItemsSource = null;
            }
            if (!restoredReview)
            {
                ReviewAttributes.ItemsSource = null;
                ReviewSkills.ItemsSource = null;
                ReviewTraits.ItemsSource = null;
                ReviewIssues.ItemsSource = null;
                CareerPrerequisiteIssues.ItemsSource = null;
                ReviewCharacterSummary.Text = "";
                ReviewLifePath.Text = "";
                ReviewRuleStatus.Text = "Complete the earlier stages to review this character.";
                CareerPrerequisiteStatus.Text = "";
                ReviewFinalStatus.Text = "";
                FreeXpModuleCost.Text = "";
                FreeXpSpentXp.Text = "";
                FreeXpRemainingXp.Text = "";
                RefreshFreeXpAllocationLists();
                ResetReviewXpButton.Visibility = Visibility.Collapsed;
            }
            if (TotalsHost.Visibility != Visibility.Visible)
            {
                RunningFreeXp.Text = "";
            }
        }
    }

    private void UpdateReview(Character character)
    {
        var issues = PrerequisiteRules.Evaluate(character);
        var summary = CharacterRules.Calculate(character);
        var blockingCount = issues.Count(issue => issue.Category == "Affiliation");
        ReviewIssues.ItemsSource = BuildReviewIssueRows(issues, summary.FreeXp);
        var careerRows = BuildCareerPrerequisiteRows(summary.FreeXp);
        CareerPrerequisiteIssues.ItemsSource = careerRows;
        UpdateCareerPrerequisiteStatus(careerRows);
        var reviewXp = reviewFreeXpAllocations.Values.Sum();
        ReviewRuleStatus.Text = blockingCount > 0
            ? $"{blockingCount} blocking conflict(s) must be corrected."
            : issues.Count > 0
                ? $"{issues.Count} prerequisite warning(s) remain."
                : "Ready to create. No unmet prerequisites found.";
        if (reviewXp > 0)
        {
            ReviewRuleStatus.Text += $" Review Free XP spent: {reviewXp}.";
        }
        ReviewRuleStatus.Foreground = blockingCount > 0
            ? System.Windows.Media.Brushes.Firebrick
            : issues.Count > 0
                ? System.Windows.Media.Brushes.DarkGoldenrod
                : System.Windows.Media.Brushes.DarkGreen;
        ReviewFinalStatus.Text = ReviewRuleStatus.Text;
        ReviewFinalStatus.Foreground = ReviewRuleStatus.Foreground;
        CreateButton.IsEnabled = blockingCount == 0;
        ResetReviewXpButton.Visibility = reviewXp > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        RefreshFreeXpAllocationLists();

        ReviewCharacterSummary.Text =
            $"{character.Name}{Environment.NewLine}" +
            $"{character.Sex}, campaign year {character.GameYear}, age {character.Age}{Environment.NewLine}" +
            $"Height / Weight: {character.Height} cm / {character.Weight} kg{Environment.NewLine}" +
            $"Hair / Eyes: {ValueOrDash(character.HairColor)} / {ValueOrDash(character.EyeColor)}";

        var affiliation = character.BirthAffiliation.Length > 0
            ? $"{character.Affiliation} (born {character.BirthAffiliation})"
            : character.Affiliation;
        var educationFields = character.EducationFields.Count == 0
            ? ""
            : $" ({string.Join(" / ", character.EducationFields)})";
        var education = character.EducationHistory.Count == 0
            ? "None"
            : $"{string.Join(" -> ", character.EducationHistory)}{educationFields}";
        ReviewLifePath.Text =
            $"Affiliation: {affiliation}{Environment.NewLine}" +
            $"Sub-affiliation: {ValueOrDash(character.SubAffiliation.Length > 0 ? character.SubAffiliation : character.BirthSubAffiliation)}{Environment.NewLine}" +
            $"Early childhood: {character.EarlyChildhood}{Environment.NewLine}" +
            $"Late childhood: {character.LateChildhood}{Environment.NewLine}" +
            $"Education: {education}{Environment.NewLine}" +
            $"Careers: {(character.RealLifeHistory.Count == 0 ? "None" : string.Join(" -> ", character.RealLifeHistory))}";
    }

    private IReadOnlyList<ReviewIssueRow> BuildReviewIssueRows(
        IReadOnlyList<PrerequisiteIssue> issues,
        int freeXp)
    {
        return issues
            .Select(issue =>
            {
                var missing = Math.Max(0, issue.RequiredXp - issue.ActualXp);
                var canSpend = IsReviewFreeXpIssue(issue) &&
                    missing > 0 &&
                    missing <= freeXp;
                var action = IsReviewFreeXpIssue(issue)
                    ? canSpend
                        ? $"Spend {missing}"
                        : missing > 0
                            ? "Need XP"
                            : "Fixed"
                    : "Edit stage";
                return new ReviewIssueRow(issue, missing, canSpend, action);
            })
            .ToArray();
    }

    private IReadOnlyList<CareerPrerequisiteRow> BuildCareerPrerequisiteRows(
        int freeXp)
    {
        var availability = EvaluateCareerAvailability(
            null, includeFreeXpAllocations: true, out var message);
        if (message is not null)
        {
            return [];
        }

        return availability
            .Where(item => item.Issues.Count > 0)
            .SelectMany(item => item.Issues.Select(issue =>
            {
                var missing = Math.Max(0, issue.RequiredXp - issue.ActualXp);
                var canSpend = IsReviewFreeXpIssue(issue) &&
                    missing > 0 &&
                    missing <= freeXp;
                var action = IsReviewFreeXpIssue(issue)
                    ? canSpend
                        ? $"Spend {missing}"
                        : missing > 0
                            ? "Need XP"
                            : "Fixed"
                    : "Edit stage";
                return new CareerPrerequisiteRow(
                    item.Module.Name,
                    issue,
                    missing,
                    canSpend,
                    action);
            }))
            .OrderBy(row => row.Career)
            .ThenBy(row => row.Category)
            .ThenBy(row => row.Name)
            .ToArray();
    }

    private void UpdateCareerPrerequisiteStatus(
        IReadOnlyList<CareerPrerequisiteRow> rows)
    {
        if (rows.Count == 0)
        {
            CareerPrerequisiteStatus.Text =
                "Career planner: all currently available careers meet prerequisites.";
            CareerPrerequisiteStatus.Foreground =
                System.Windows.Media.Brushes.DarkGreen;
            return;
        }

        var careers = rows
            .Select(row => row.Career)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var fixable = rows.Count(row => row.CanSpendFreeXp);
        CareerPrerequisiteStatus.Text =
            $"Career planner: {careers} hidden career(s) have unmet prerequisites; " +
            $"{fixable} gap(s) can be filled with current Free XP.";
        CareerPrerequisiteStatus.Foreground = fixable > 0
            ? System.Windows.Media.Brushes.DarkGoldenrod
            : System.Windows.Media.Brushes.Firebrick;
    }

    private static bool IsReviewFreeXpIssue(PrerequisiteIssue issue) =>
        issue.Category is "Attribute" or "Skill" or "Trait";

    private static string ReviewAllocationKey(string category, string name) =>
        $"{category}|{name}";

    private void SpendReviewFreeXp_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not ReviewIssueRow row ||
            !row.CanSpendFreeXp)
        {
            return;
        }

        var character = BuildCharacter();
        var freeXp = CharacterRules.Calculate(character).FreeXp;
        if (row.MissingXp > freeXp)
        {
            MessageBox.Show(
                $"This fix needs {row.MissingXp} XP, but only {freeXp} Free XP remains.",
                "Not enough Free XP",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            UpdatePreview();
            return;
        }

        var key = ReviewAllocationKey(row.Category, row.Name);
        reviewFreeXpAllocations[key] =
            reviewFreeXpAllocations.GetValueOrDefault(key) + row.MissingXp;
        UpdatePreview();
    }

    private void SpendCareerPrerequisiteFreeXp_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.Tag is not CareerPrerequisiteRow row ||
            !row.CanSpendFreeXp)
        {
            return;
        }

        SpendFreeXpOnRequirement(row.Category, row.Name, row.MissingXp);
    }

    private void AddFreeXp_Click(object sender, RoutedEventArgs e)
    {
        if (FreeXpTargetPicker.SelectedItem is not FreeXpTargetOption target)
        {
            MessageBox.Show(
                "Choose a target before spending Free XP.",
                "Choose a target",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        AddManualFreeXp(target.Category, target.Name, FreeXpAmount);
    }

    private void AddManualFreeXp(
        string category,
        string target,
        TextBox amountBox)
    {
        target = target.Trim();
        if (target.Length == 0)
        {
            MessageBox.Show(
                "Choose a target before spending Free XP.",
                "Choose a target",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        if (!int.TryParse(amountBox.Text, out var xp) || xp <= 0)
        {
            MessageBox.Show(
                "Enter a positive XP amount.",
                "Invalid XP",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var freeXp = CharacterRules.Calculate(BuildCharacter()).FreeXp;
        if (xp > freeXp)
        {
            MessageBox.Show(
                $"This spend needs {xp} XP, but only {freeXp} Free XP remains.",
                "Not enough Free XP",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            UpdatePreview();
            return;
        }

        var key = ReviewAllocationKey(category, target);
        reviewFreeXpAllocations[key] =
            reviewFreeXpAllocations.GetValueOrDefault(key) + xp;
        amountBox.Text = "0";
        UpdatePreview();
    }

    private void SpendFreeXpOnRequirement(string category, string name, int xp)
    {
        var character = BuildCharacter();
        var freeXp = CharacterRules.Calculate(character).FreeXp;
        if (xp > freeXp)
        {
            MessageBox.Show(
                $"This fix needs {xp} XP, but only {freeXp} Free XP remains.",
                "Not enough Free XP",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            UpdatePreview();
            return;
        }

        var key = ReviewAllocationKey(category, name);
        reviewFreeXpAllocations[key] =
            reviewFreeXpAllocations.GetValueOrDefault(key) + xp;
        UpdatePreview();
    }

    private void ResetReviewXp_Click(object sender, RoutedEventArgs e)
    {
        reviewFreeXpAllocations.Clear();
        UpdatePreview();
    }

    private void RemoveFreeXp_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: FreeXpAllocationRow row })
        {
            return;
        }

        reviewFreeXpAllocations.Remove(row.Key);
        UpdatePreview();
    }

    private void RefreshFreeXpAllocationLists()
    {
        RefreshFreeXpTargetOptions();
        FreeXpAllocations.ItemsSource = BuildFreeXpAllocationRows();
    }

    private void RefreshFreeXpTargetOptions()
    {
        var selectedKey = FreeXpTargetPicker.SelectedItem is FreeXpTargetOption selected
            ? ReviewAllocationKey(selected.Category, selected.Name)
            : null;
        var usedKeys = reviewFreeXpAllocations
            .Where(allocation => allocation.Value != 0)
            .Select(allocation => allocation.Key)
            .ToHashSet(StringComparer.Ordinal);
        var available = allFreeXpTargetOptions
            .Where(option => !usedKeys.Contains(
                ReviewAllocationKey(option.Category, option.Name)))
            .ToArray();

        FreeXpTargetPicker.ItemsSource = available;
        FreeXpTargetPicker.SelectedItem =
            selectedKey is null || usedKeys.Contains(selectedKey)
                ? null
                : available.FirstOrDefault(option =>
                    ReviewAllocationKey(option.Category, option.Name) == selectedKey);
    }

    private static IReadOnlyList<FreeXpTargetOption> BuildFreeXpTargetOptions(
        IReadOnlyList<string> traitNames,
        IReadOnlyList<string> skillNames) =>
    [
        .. new[] { "STR", "BOD", "RFL", "DEX", "INT", "WIL", "CHA", "EDG" }
            .Select(name => new FreeXpTargetOption("Attribute", name)),
        .. traitNames.Select(name => new FreeXpTargetOption("Trait", name)),
        .. skillNames.Select(name => new FreeXpTargetOption("Skill", name))
    ];

    private IReadOnlyList<FreeXpAllocationRow> BuildFreeXpAllocationRows() =>
        reviewFreeXpAllocations
            .Select(allocation => new
            {
                Parts = allocation.Key.Split('|', 2),
                allocation.Value
            })
            .Where(allocation => allocation.Parts.Length == 2 &&
                allocation.Value != 0)
            .Select(allocation => new FreeXpAllocationRow(
                allocation.Parts[0] + "|" + allocation.Parts[1],
                allocation.Parts[0],
                allocation.Parts[1],
                allocation.Value))
            .OrderBy(row => row.Category switch
            {
                "Attribute" => 0,
                "Trait" => 1,
                "Skill" => 2,
                _ => 3
            })
            .ThenBy(row => row.Name)
            .ToArray();

    private static string ValueOrDash(string value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

    private Character BuildCharacter() => BuildCharacter(pages.Length - 1);

    private Character BuildCharacter(
        int throughStep,
        bool allowIncompleteCurrentStepPreview = false)
    {
        var affiliation = SelectedAffiliation ??
            throw new InvalidOperationException("Choose an affiliation.");
        var childhood = throughStep >= 2
            ? SelectedChildhood ??
              throw new InvalidOperationException("Choose an early childhood.")
            : null;
        var lateChildhood = throughStep >= 3
            ? SelectedLateChildhood ??
              throw new InvalidOperationException("Choose a late childhood.")
            : null;
        var school = throughStep >= 4 ? SelectedSchool : null;
        if (school is not null && SelectedBasicField is null)
        {
            throw new InvalidOperationException("Choose one Basic Field for the selected school.");
        }
        var secondSchool = throughStep >= 4 ? SelectedSecondSchool : null;
        if (secondSchool is not null && SelectedSecondBasicField is null)
        {
            throw new InvalidOperationException("Choose one Basic Field for the second selected school.");
        }
        var thirdSchool = throughStep >= 4 ? SelectedThirdSchool : null;
        if (thirdSchool is not null && SelectedThirdBasicField is null)
        {
            throw new InvalidOperationException("Choose one Basic Field for the third selected school.");
        }
        if (throughStep >= 4 &&
            SelectedSpecialistField is not null &&
            SelectedAdvancedField is null)
        {
            throw new InvalidOperationException(
                "Choose an Advanced Field before adding a third field.");
        }
        if (throughStep >= 4 &&
            SelectedSecondSpecialistField is not null &&
            SelectedSecondAdvancedField is null)
        {
            throw new InvalidOperationException(
                "Choose an Advanced Field before adding a third field to the second education.");
        }
        if (throughStep >= 4 &&
            SelectedThirdSpecialistField is not null &&
            SelectedThirdAdvancedField is null)
        {
            throw new InvalidOperationException(
                "Choose an Advanced Field before adding a third field to the third education.");
        }
        var language = LanguagePicker.SelectedItem as string ??
            throw new InvalidOperationException("Choose a primary language.");

        var character = LifePathEngine.CreateBase(CharacterName.Text.Trim(), language);
        character.Sex = SexPicker.SelectedItem as string ?? "Male";
        character.GameYear = TryReadPositiveNumber(GameYearInput, out var gameYear)
            ? gameYear : 3045;
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
        character.ClanTrainingField = lateChildhood is null
            ? ""
            : GetSelectedChoice(lateChildhood, "branch");
        character.EarlyChildhood = childhood?.Name ?? "";
        character.LateChildhood = lateChildhood?.Name ?? "";
        character.School = school?.Name ?? "";
        character.BasicSchool = throughStep >= 4 ? SelectedBasicField?.Name ?? "" : "";
        character.AdvancedSchool = throughStep >= 4 ? SelectedAdvancedField?.Name ?? "" : "";
        character.SpecialSchool = throughStep >= 4 ? SelectedSpecialistField?.Name ?? "" : "";
        if (throughStep >= 4)
        {
            foreach (var educationSchool in SelectedEducationSchools())
            {
                character.EducationHistory.Add(educationSchool.Name);
            }
            foreach (var educationField in SelectedEducationFields())
            {
                character.EducationFields.Add(educationField.Name);
            }
        }
        character.RealLife =
            throughStep >= 5
                ? SelectedSecondRealLife?.Name ?? SelectedRealLife?.Name ?? ""
                : "";
        var phenotype = childhood is null ? "" : GetSelectedChoice(childhood, "phenotype");
        if (phenotype.Length > 0) character.Phenotype = phenotype;

        var selectedModules = SelectedModuleEntries()
            .Where(entry => ChoiceStep(entry) <= throughStep)
            .ToArray();
        var modules = selectedModules.Select(entry => entry.Module).ToArray();
        var calculatedAge = CalculateLifePathAge(throughStep, selectedModules);
        if (calculatedAge > 0)
        {
            character.Age = calculatedAge;
        }
        foreach (var selectedModule in selectedModules)
        {
            var module = selectedModule.Module;
            var selection = CreateSelection(selectedModule);
            if (selectedModule.IsStage4)
            {
                if (allowIncompleteCurrentStepPreview &&
                    ChoiceStep(selectedModule) == throughStep)
                {
                    LifePathEngine.ApplyStage4Preview(character, selection);
                }
                else
                {
                    LifePathEngine.ApplyStage4(character, selection);
                }
                ApplyCareerState(character, module.Name);
            }
            else
            {
                if (allowIncompleteCurrentStepPreview &&
                    ChoiceStep(selectedModule) == throughStep)
                {
                    LifePathEngine.ApplyPreview(character, selection);
                }
                else
                {
                    LifePathEngine.Apply(character, selection);
                }
            }
        }
        if (childhood is not null)
        {
            LifePathEngine.ApplyAffiliationContext(character, affiliation, childhood, language);
        }
        if (lateChildhood is not null)
        {
            LifePathEngine.ApplyAffiliationContext(character, affiliation, lateChildhood, language);
        }
        foreach (var educationSchool in throughStep >= 4
                     ? SelectedEducationSchools()
                     : Enumerable.Empty<LifePathModule>())
        {
            LifePathEngine.ApplyAffiliationContext(character, affiliation,
                educationSchool, language);
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
        if (throughStep >= FreeXpStep)
        {
            ApplyReviewFreeXp(character);
        }
        character.Notes =
            $"-----Life Path-----\nAffiliation: {affiliation.Name}" +
            $"\nSub-affiliation: {character.SubAffiliation}" +
            $"\nBirth affiliation: {character.BirthAffiliation}" +
            $"\nBirth sub-affiliation: {character.BirthSubAffiliation}" +
            $"\nClan caste: {character.ClanCaste}" +
            $"\nEarly Childhood: {character.EarlyChildhood}" +
            $"\nLate Childhood: {character.LateChildhood}" +
            $"\nEducation: {string.Join(" -> ", character.EducationHistory)}" +
            $"\nEducation Fields: {string.Join(" / ", character.EducationFields)}" +
            $"\nCareers: {string.Join(" -> ", character.RealLifeHistory)}";
        return character;
    }

    private void ApplyReviewFreeXp(Character character)
    {
        foreach (var allocation in reviewFreeXpAllocations)
        {
            var parts = allocation.Key.Split('|', 2);
            if (parts.Length != 2 || allocation.Value <= 0) continue;
            var (category, name) = (parts[0], parts[1]);
            var values = category switch
            {
                "Attribute" => character.Attributes,
                "Skill" => character.Skills,
                "Trait" => character.Traits,
                _ => null
            };
            if (values is null) continue;
            AddReviewXp(values, name, allocation.Value);
        }
    }

    private static void AddReviewXp(
        ICollection<NamedValue> values,
        string name,
        int xp)
    {
        var existing = values.FirstOrDefault(item => item.Name == name);
        if (existing is null)
        {
            values.Add(new NamedValue(name, xp));
        }
        else
        {
            existing.Value += xp;
        }
    }

    private static int CalculateLifePathAge(
        int throughStep,
        IReadOnlyList<SelectedModule> selectedModules)
    {
        if (throughStep < 3)
        {
            return 0;
        }

        var age = LateChildhoodCompletionAge;
        if (throughStep >= 4)
        {
            age += selectedModules
                .Where(entry => !entry.IsStage4 &&
                    entry.Module.Id.StartsWith("field-", StringComparison.Ordinal))
                .Sum(entry => entry.Module.TimeYears);
        }
        if (throughStep >= 5)
        {
            age += selectedModules
                .Where(entry => entry.IsStage4)
                .Sum(entry => entry.Module.TimeYears);
        }
        return age;
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
            if (choice.Target == EffectTarget.Flexible &&
                choice.FixedFlexibleSelections)
            {
                allocations[choice.Id] = input.Pickers
                    .Select(picker => picker.SelectedItem as string ?? "")
                    .Where(name => name.Length > 0)
                    .Select(name => new ChoiceAllocation(name, choice.Xp))
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

    private IEnumerable<LifePathModule> SelectedModules(int throughStep)
    {
        return SelectedModuleEntries()
            .Where(entry => ChoiceStep(entry) <= throughStep)
            .Select(entry => entry.Module);
    }

    private IEnumerable<LifePathModule> SelectedEducationModules()
    {
        foreach (var module in new[]
                 {
                     SelectedSchool,
                     SelectedBasicField,
                     SelectedAdvancedField,
                     SelectedSpecialistField,
                     SelectedSecondSchool,
                     SelectedSecondBasicField,
                     SelectedSecondAdvancedField,
                     SelectedSecondSpecialistField,
                     SelectedThirdSchool,
                     SelectedThirdBasicField,
                     SelectedThirdAdvancedField,
                     SelectedThirdSpecialistField
                 })
        {
            if (module is not null) yield return module;
        }
    }

    private IEnumerable<LifePathModule> SelectedEducationSchools()
    {
        if (SelectedSchool is not null) yield return SelectedSchool;
        if (SelectedSecondSchool is not null) yield return SelectedSecondSchool;
        if (SelectedThirdSchool is not null) yield return SelectedThirdSchool;
    }

    private IEnumerable<LifePathModule> SelectedEducationFields()
    {
        foreach (var module in new[]
                 {
                     SelectedBasicField,
                     SelectedAdvancedField,
                     SelectedSpecialistField,
                     SelectedSecondBasicField,
                     SelectedSecondAdvancedField,
                     SelectedSecondSpecialistField,
                     SelectedThirdBasicField,
                     SelectedThirdAdvancedField,
                     SelectedThirdSpecialistField
                 })
        {
            if (module is not null) yield return module;
        }
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
                     (SelectedSecondSchool, false),
                     (SelectedSecondBasicField, false),
                     (SelectedSecondAdvancedField, false),
                     (SelectedSecondSpecialistField, false),
                     (SelectedThirdSchool, false),
                     (SelectedThirdBasicField, false),
                     (SelectedThirdAdvancedField, false),
                     (SelectedThirdSpecialistField, false),
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
