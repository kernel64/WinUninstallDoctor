/*
 * WinUninstallDoctor - Windows uninstaller cleanup and diagnostic tool
 * Copyright (C) 2026 Mohamed Aymen
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

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
