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
    public class ProgramEntry
    {
        public string DisplayName { get; set; }
        public string DisplayVersion { get; set; }
        public string Publisher { get; set; }
        public string UninstallString { get; set; }
        public string DisplayIcon { get; set; }
        public string RegistryKeyPath { get; set; }
        public bool UninstallExists { get; set; }
        public bool HasProblem { get; set; }
        public bool IsSystemComponent { get; set; }
        public bool IsSelectable { get; set; }
    }
}
