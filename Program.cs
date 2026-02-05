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
using System.Windows.Forms;
using WinUninstallDoctor;

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

        Application.Run(new WinUninstallDoctor.MainForm());
    }
}
