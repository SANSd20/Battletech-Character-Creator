using System.Windows;
using System.Windows.Threading;

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
                    wizard.Close();
                    Shutdown(0);
                });
            wizard.Show();
            return;
        }

        new MainWindow().Show();
    }
}
