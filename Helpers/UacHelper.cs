using System;
using System.Diagnostics;
using System.Windows.Forms;

public static class UacHelper
{
    public static void RestartAsAdmin()
    {
        try
        {
            ProcessStartInfo psi = new()
            {
                FileName = Application.ExecutablePath,
                UseShellExecute = true,
                Verb = "runas"
            };
            Process.Start(psi);
        }
        catch
        {
            MessageBox.Show("Administrator privileges are required.");
        }

        Environment.Exit(0);
    }
}
