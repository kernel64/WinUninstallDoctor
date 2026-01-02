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
