/*
 * WinUninstallDoctor - Windows uninstaller cleanup and diagnostic tool
 * Copyright (C) 2026 Mohamed Aymen
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Threading;

public static class SingleInstanceHelper
{
    private static Mutex mutex;

    public static bool EnsureSingleInstance()
    {
        mutex = new Mutex(true, "WinUninstallDoctor_Mutex", out bool created);
        return created;
    }
}
