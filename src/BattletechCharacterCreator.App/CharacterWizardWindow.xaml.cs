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
    private readonly Dictionary<string, ChoiceInput> choiceControls = [];
    private readonly List<(FrameworkElement Element, int Step)> choiceGroups = [];
    private readonly FrameworkElement[] pages;
    private readonly TextBlock[] stepLabels;
    private int currentStep;
    private bool refreshing;

    private sealed record ChoiceInput(
        IReadOnlyList<ComboBox> Pickers,
        IReadOnlyList<TextBox>? Amounts = null,
        ModuleChoice? Choice = null,
        string ModuleName = "",
        int Step = -1,
        TextBlock? Remaining = null,
        TextBlock? Status = null);

    private sealed record XpAdjustment(TextBox Input, int Delta);

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
        HairColorPicker.ItemsSource = resources.HairColors;
        EyeColorPicker.ItemsSource = resources.EyeColors;
        EraPresetPicker.ItemsSource = EraPresetCatalog.Presets;
        SexPicker.ItemsSource = new[] { "Male", "Female" };
        SexPicker.SelectedIndex = 0;

        RefreshEraAvailability();
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
        SubAffiliationPicker.SelectedItem = SubAffiliationPicker.Items
            .Cast<LifePathModule>()
            .First(module => module.Name == subAffiliationName);
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

    public void SmokeEraPresetSelection()
    {
        EraPresetPicker.SelectedItem = EraPresetCatalog.Presets
            .Single(preset => preset.Name == "Star League");
        if (AffiliationPicker.Items
            .Cast<LifePathModule>()
            .Any(module => module.Id == "invading-clan") ||
            OrderMemberCheck.IsEnabled)
        {
            throw new InvalidOperationException(
                "Star League era availability did not hide later-era affiliations.");
        }

        EraPresetPicker.SelectedItem = EraPresetCatalog.Presets
            .Single(preset => preset.Name == "Clan Invasion");
        if (GameYearInput.Text != "3052")
        {
            throw new InvalidOperationException(
                "The Clan Invasion era preset did not set the game year.");
        }

        var character = BuildCharacter();
        if (character.GameYear != 3052 ||
            character.Age != character.GameYear - character.BirthYear)
        {
            throw new InvalidOperationException(
                "Era preset game year was not applied to the created character.");
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

        EraPresetPicker.SelectedItem = EraPresetCatalog.Presets
            .Single(preset => preset.Name == "Clan Invasion");
        SelectAffiliationForCapture("rasalhague");
        var clanInvasionSubAffiliations = SubAffiliationPicker.Items
            .Cast<LifePathModule>()
            .Select(module => module.Name)
            .ToArray();
        if (!clanInvasionSubAffiliations.Contains("Clan War Expatriate") ||
            clanInvasionSubAffiliations.Contains("Ghost Bear Dominion"))
        {
            throw new InvalidOperationException(
                "Clan Invasion Rasalhague sub-affiliation availability is wrong.");
        }

        EraPresetPicker.SelectedItem = EraPresetCatalog.Presets
            .Single(preset => preset.Name == "Civil War");
        SelectAffiliationForCapture("rasalhague");
        if (!SubAffiliationPicker.Items
            .Cast<LifePathModule>()
            .Any(module => module.Name == "Ghost Bear Dominion"))
        {
            throw new InvalidOperationException(
                "Civil War Rasalhague sub-affiliation availability did not reveal Ghost Bear Dominion.");
        }
    }

    public void SelectLateChildhoodForCapture(string lateChildhoodId)
    {
        LateChildhoodPicker.SelectedItem = LifePathCatalog.LateChildhoods
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
        RealLifePicker.SelectedItem = LifePathCatalog.RealLifeModules
            .First(module => module.Id == firstCareerId);
        RefreshSecondCareerOptions();
        if (!string.IsNullOrWhiteSpace(secondCareerId))
        {
            SecondCareerCheck.IsChecked = true;
            SecondRealLifePicker.SelectedItem = SecondRealLifePicker.Items
                .Cast<LifePathModule>()
                .First(module => module.Id == secondCareerId);
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

        if (currentStep > 0) UpdatePreview();
    }

    private bool ValidateStep(int step)
    {
        string? message = step switch
        {
            0 when string.IsNullOrWhiteSpace(CharacterName.Text) =>
                "Enter a character name.",
            0 when !TryReadPositiveNumber(BirthYearInput, out _) =>
                "Enter a valid year of birth.",
            0 when !TryReadPositiveNumber(GameYearInput, out _) =>
                "Enter a valid game year.",
            0 when TryReadPositiveNumber(BirthYearInput, out var birthYear) &&
                TryReadPositiveNumber(GameYearInput, out var gameYear) &&
                gameYear < birthYear =>
                "Enter a game year that is the same as or later than the year of birth.",
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
    private LifePathModule? SelectedSubAffiliation => SubAffiliationPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedCaste => CastePicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedChildhood => ChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedLateChildhood => LateChildhoodPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSchool => EducationCheck.IsChecked == true
        ? SchoolPicker.SelectedItem as LifePathModule
        : null;
    private LifePathModule? SelectedBasicField => BasicFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedAdvancedField => AdvancedFieldPicker.SelectedItem as LifePathModule;
    private LifePathModule? SelectedSpecialistField => SpecialistFieldPicker.SelectedItem as LifePathModule;
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

    private void ModuleSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RefreshModules();
    }

    private void EraPresetSelectionChanged(
        object sender,
        SelectionChangedEventArgs e)
    {
        if (EraPresetPicker.SelectedItem is EraPreset preset)
        {
            GameYearInput.Text = preset.DefaultYear.ToString();
            RefreshEraAvailability();
        }
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
        SubAffiliationPicker.ItemsSource = subAffiliations;
        SubAffiliationPanel.Visibility = SubAffiliationPicker.Items.Count > 0
            ? Visibility.Visible : Visibility.Collapsed;
        if (subAffiliation is not null &&
            subAffiliations.Any(module => module.Id == subAffiliation.Id))
        {
            SubAffiliationPicker.SelectedItem = subAffiliations
                .First(module => module.Id == subAffiliation.Id);
        }
        else if (SubAffiliationPicker.Items.Count > 0)
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
        UpdateEducationSummary();
        BuildChoiceControls();
        refreshing = false;
        UpdatePreview();
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
        RefreshModules();
    }

    private void RealLifeSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || refreshing) return;
        if (sender == RealLifePicker)
        {
            RefreshSecondCareerOptions();
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
        RefreshSecondCareerOptions();
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

    private void RefreshSecondCareerOptions()
    {
        var previousId = (SecondRealLifePicker.SelectedItem as LifePathModule)?.Id;
        var firstCareer = SelectedRealLife;
        var options = LifePathCatalog.RealLifeModules
            .Where(module => firstCareer is null ||
                firstCareer.Repeatable ||
                module.Id != firstCareer.Id)
            .ToArray();
        SecondRealLifePicker.ItemsSource = options;
        SecondRealLifePicker.SelectedItem = options
            .FirstOrDefault(module => module.Id == previousId);
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

    private void UpdateEducationSummary()
    {
        var school = SelectedSchool;
        SchoolDescription.Text = school?.Description ??
            "No formal education selected.";
        var fields = new[]
            {
                ("Basic field", SelectedBasicField),
                ("Advanced field", SelectedAdvancedField),
                ("Third field", SelectedSpecialistField)
            }
            .Where(entry => entry.Item2 is not null)
            .Select(entry => $"{entry.Item1}: {entry.Item2!.Name}")
            .ToArray();
        EducationFieldsSummary.Text = string.Join(Environment.NewLine, fields);
        EducationModuleCost.Text = school is null
            ? "0"
            : new[]
                {
                    school,
                    SelectedBasicField,
                    SelectedAdvancedField,
                    SelectedSpecialistField
                }
                .Where(module => module is not null)
                .Sum(module => module!.ModuleCost)
                .ToString();
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
                        var defaults = CreateDefaultFlexibleAllocations(
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

                        for (var i = 0; i < 6; i++)
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
                            picker.SelectedItem = defaults[i].Name;
                            var amount = new TextBox
                            {
                                Text = defaults[i].Xp.ToString(),
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
                            allocatorPanel.Children.Add(row);
                        }
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
                            ChoiceStep(selectedModule), remaining, status);
                        choiceControls[Key(selectedModule, choice)] = input;
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

    private void UpdateFlexibleChoiceDisplays()
    {
        foreach (var input in choiceControls.Values.Where(
                     input => input.Choice is not null))
        {
            UpdateFlexibleChoiceStatus(input);
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

        if (remaining > 0)
        {
            var target = options.FirstOrDefault(option =>
                LifePathEngine.ClassifyFlexibleTarget(option) !=
                EffectTarget.Attribute);
            target ??= options.FirstOrDefault();
            if (target is not null)
            {
                allocations.Add(new ChoiceAllocation(target, remaining));
            }
        }

        allocations = allocations
            .Where(allocation => allocation.Xp > 0)
            .GroupBy(allocation => allocation.Name, StringComparer.Ordinal)
            .Select(group => new ChoiceAllocation(
                group.Key, group.Sum(allocation => allocation.Xp)))
            .ToList();
        while (allocations.Count < 6)
        {
            allocations.Add(new ChoiceAllocation("", 0));
        }
        return allocations.Take(6).ToArray();
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
        RefreshThirdField(SelectedSchool);
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
            var character = BuildCharacter();
            var summary = CharacterRules.Calculate(character);
            PreviewAttributes.ItemsSource = character.Attributes;
            PreviewSkills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            PreviewTraits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            Stage0Attributes.ItemsSource = character.Attributes;
            Stage0Skills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            Stage0Traits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ReviewAttributes.ItemsSource = character.Attributes;
            ReviewSkills.ItemsSource = character.Skills.OrderBy(item => item.Name);
            ReviewTraits.ItemsSource = character.Traits.OrderBy(item => item.Name);
            ModuleCost.Text =
                LifePathEngine.CalculateModuleCost(character, SelectedModules()).ToString();
            SpentXp.Text = summary.SpentXp.ToString();
            FreeXp.Text = summary.FreeXp.ToString();
            RunningFreeXp.Text = summary.FreeXp.ToString();
            UpdateReview(character);
        }
        catch (InvalidOperationException)
        {
            PreviewAttributes.ItemsSource = null;
            PreviewSkills.ItemsSource = null;
            PreviewTraits.ItemsSource = null;
            Stage0Attributes.ItemsSource = null;
            Stage0Skills.ItemsSource = null;
            Stage0Traits.ItemsSource = null;
            ReviewAttributes.ItemsSource = null;
            ReviewSkills.ItemsSource = null;
            ReviewTraits.ItemsSource = null;
            ReviewIssues.ItemsSource = null;
            ReviewCharacterSummary.Text = "";
            ReviewLifePath.Text = "";
            ReviewRuleStatus.Text = "Complete the earlier stages to review this character.";
            RunningFreeXp.Text = "";
        }
    }

    private void UpdateReview(Character character)
    {
        var issues = PrerequisiteRules.Evaluate(character);
        var blockingCount = issues.Count(issue => issue.Category == "Affiliation");
        ReviewIssues.ItemsSource = issues;
        ReviewRuleStatus.Text = blockingCount > 0
            ? $"{blockingCount} blocking conflict(s) must be corrected."
            : issues.Count > 0
                ? $"{issues.Count} prerequisite warning(s) remain."
                : "Ready to create. No unmet prerequisites found.";
        ReviewRuleStatus.Foreground = blockingCount > 0
            ? System.Windows.Media.Brushes.Firebrick
            : issues.Count > 0
                ? System.Windows.Media.Brushes.DarkGoldenrod
                : System.Windows.Media.Brushes.DarkGreen;
        CreateButton.IsEnabled = blockingCount == 0;

        ReviewCharacterSummary.Text =
            $"{character.Name}{Environment.NewLine}" +
            $"{character.Sex}, born {character.BirthYear}, game year {character.GameYear}, age {character.Age}{Environment.NewLine}" +
            $"Height / Weight: {character.Height} cm / {character.Weight} kg{Environment.NewLine}" +
            $"Hair / Eyes: {ValueOrDash(character.HairColor)} / {ValueOrDash(character.EyeColor)}";

        var affiliation = character.BirthAffiliation.Length > 0
            ? $"{character.Affiliation} (born {character.BirthAffiliation})"
            : character.Affiliation;
        var education = character.School.Length == 0
            ? "None"
            : string.Join(" / ", new[]
                {
                    character.School,
                    character.BasicSchool,
                    character.AdvancedSchool,
                    character.SpecialSchool
                }.Where(value => value.Length > 0));
        ReviewLifePath.Text =
            $"Affiliation: {affiliation}{Environment.NewLine}" +
            $"Sub-affiliation: {ValueOrDash(character.SubAffiliation.Length > 0 ? character.SubAffiliation : character.BirthSubAffiliation)}{Environment.NewLine}" +
            $"Early childhood: {character.EarlyChildhood}{Environment.NewLine}" +
            $"Late childhood: {character.LateChildhood}{Environment.NewLine}" +
            $"Education: {education}{Environment.NewLine}" +
            $"Careers: {(character.RealLifeHistory.Count == 0 ? "None" : string.Join(" -> ", character.RealLifeHistory))}";
    }

    private static string ValueOrDash(string value) =>
        string.IsNullOrWhiteSpace(value) ? "-" : value;

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
        character.Sex = SexPicker.SelectedItem as string ?? "Male";
        character.BirthYear = TryReadPositiveNumber(BirthYearInput, out var birthYear)
            ? birthYear : 3024;
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
