using System;
using System.Windows.Forms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        if (!SingleInstanceHelper.EnsureSingleInstance())
        {
            MessageBox.Show("WinUninstallDoctor is already running.");
            return;
        }

        if (!SecurityHelper.IsRunningAsAdmin())
        {
            UacHelper.RestartAsAdmin();
            return;
        }

        Application.Run(new MainForm());
    }
}
