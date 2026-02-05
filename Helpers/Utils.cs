/*
 * WinUninstallDoctor - Windows uninstaller cleanup and diagnostic tool
 * Copyright (C) 2026 Mohamed Aymen
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 */

using System.Drawing;
using System.IO;
using System.Reflection;

namespace WinUninstallDoctor
{

    public static class Utils
    {
        public static Image ByteArrayToImage(byte[] byteArray)
        {
            using (var ms = new MemoryStream(byteArray))
            {
                return Image.FromStream(ms);
            }
        }

        public static Icon LoadIconFromResource(byte[] iconBytes)
        {
            using (var ms = new MemoryStream(iconBytes))
            using (var icon = new Icon(ms))
            {
                return (Icon)icon.Clone();
            }
        }

        public static string GetVersion()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";
        }
    }

}
