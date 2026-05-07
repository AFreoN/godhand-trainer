using GodHandTrainer.Cheats;
using GodHandTrainer.UI;

namespace GodHandTrainer;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var trainer = new TrainerService();
        Application.Run(new MainForm(trainer));
    }
}
