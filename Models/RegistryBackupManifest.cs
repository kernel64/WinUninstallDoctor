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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinUninstallDoctor.Models
{
    public class RegistryBackupManifest
    {
        public string BackupId { get; set; }
        public string TargetApp { get; set; }
        public DateTime CreatedAt { get; set; }
        public string WindowsVersion { get; set; }
        public int FormatVersion { get; set; } = 1;
        public List<string> RegistryKeys { get; set; } = new();
        public string RegistryFile { get; set; }
    }
}
