using System.Windows;

namespace BattletechCharacterCreator.App;

public partial class StartWindow : Window
{
    public StartWindow()
    {
        InitializeComponent();
    }

    private void Wizard_Click(object sender, RoutedEventArgs e)
    {
        var wizard = new CharacterWizardWindow { Owner = this };
        if (wizard.ShowDialog() == true && wizard.CreatedCharacter is not null)
        {
            OpenEditor(new MainWindow(wizard.CreatedCharacter));
        }
    }

    private void Editor_Click(object sender, RoutedEventArgs e) =>
        OpenEditor(new MainWindow());

    private void OpenEditor(MainWindow editor)
    {
        editor.Show();
        Close();
    }
}
