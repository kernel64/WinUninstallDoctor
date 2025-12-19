using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

public class MainForm : Form
{
    private FlowLayoutPanel flpPrograms;
    private Button btnScan;
    private Button btnDeleteSelected;
    private ProgressBar progressBarScan;
    private Label lblStatus;

    private readonly List<ProgramEntry> programs = new();

    public MainForm()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Text = "WinUninstallDoctor";
        Width = 1100;
        Height = 750;
        StartPosition = FormStartPosition.CenterScreen;

        btnScan = new Button
        {
            Text = "Scan programs",
            Width = 140,
            Height = 32,
            Location = new Point(15, 15)
        };
        btnScan.Click += async (_, _) => await StartScanAsync();

        btnDeleteSelected = new Button
        {
            Text = "Delete selected",
            Width = 150,
            Height = 32,
            Location = new Point(165, 15),
            Enabled = false
        };
        btnDeleteSelected.Click += BtnDeleteSelected_Click;

        progressBarScan = new ProgressBar
        {
            Location = new Point(330, 20),
            Width = 350,
            Height = 22
        };

        lblStatus = new Label
        {
            Text = "Idle",
            Location = new Point(700, 22),
            AutoSize = true
        };

        flpPrograms = new FlowLayoutPanel
        {
            Location = new Point(15, 60),
            Width = ClientSize.Width - 30,
            Height = ClientSize.Height - 80,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
        };

        Controls.AddRange(new Control[]
        {
            btnScan,
            btnDeleteSelected,
            progressBarScan,
            lblStatus,
            flpPrograms
        });
    }

    private async Task StartScanAsync()
    {
        btnScan.Enabled = false;
        btnDeleteSelected.Enabled = false;
        flpPrograms.Controls.Clear();
        programs.Clear();

        lblStatus.Text = "Scanning registry...";
        progressBarScan.Value = 0;

        await Task.Run(ScanPrograms);

        lblStatus.Text = $"Scan finished ({programs.Count} programs)";
        btnScan.Enabled = true;
        btnDeleteSelected.Enabled = true;
    }

    private void ScanPrograms()
    {
        string[] paths =
        {
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
            @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
        };

        int total = CountSubKeys(paths);
        int current = 0;

        foreach (string path in paths)
        {
            using RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(path);
            if (baseKey == null) continue;

            foreach (string subKeyName in baseKey.GetSubKeyNames())
            {
                using RegistryKey appKey = baseKey.OpenSubKey(subKeyName);
                if (appKey == null) continue;

                string name = appKey.GetValue("DisplayName") as string;
                if (string.IsNullOrWhiteSpace(name))
                {
                    --total;
                    continue;
                }

                ProgramEntry entry = new()
                {
                    DisplayName = name,
                    DisplayVersion = appKey.GetValue("DisplayVersion") as string,
                    Publisher = appKey.GetValue("Publisher") as string,
                    UninstallString = appKey.GetValue("UninstallString") as string,
                    DisplayIcon = appKey.GetValue("DisplayIcon") as string,
                    RegistryKeyPath = path + "\\" + subKeyName
                };

                entry.UninstallExists = CheckUninstall(entry.UninstallString);
                programs.Add(entry);

                Invoke(() =>
                {
                    AddProgramControl(entry);
                    // update progress bar
                    progressBarScan.Value = Math.Min(100, (++current * 100) / total);
                });
            }
        }
    }

    private void AddProgramControl(ProgramEntry entry)
    {
        ProgramItemControl ctrl = new();
        ctrl.Bind(entry);
        ctrl.SelectionChanged += UpdateDeleteButtonState;
        flpPrograms.Controls.Add(ctrl);
    }

    private void UpdateDeleteButtonState()
    {
        foreach (ProgramItemControl ctrl in flpPrograms.Controls)
        {
            if (ctrl.IsChecked)
            {
                btnDeleteSelected.Enabled = true;
                return;
            }
        }
        btnDeleteSelected.Enabled = false;
    }

    private void BtnDeleteSelected_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(
            "Delete all selected broken entries from installed programs?",
            "Confirmation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        foreach (ProgramItemControl ctrl in flpPrograms.Controls)
        {
            if (ctrl.IsChecked)
                ctrl.DeleteEntry();
        }

        btnDeleteSelected.Enabled = false;
    }

    private bool CheckUninstall(string uninstallString)
    {
        if (string.IsNullOrWhiteSpace(uninstallString))
            return false;

        string exePath = ExtractExecutablePath(uninstallString);
        return exePath != null && File.Exists(exePath);
    }

    private string ExtractExecutablePath(string uninstall)
    {
        uninstall = uninstall.Trim();

        if (uninstall.StartsWith("\""))
        {
            int end = uninstall.IndexOf("\"", 1);
            if (end > 1)
                return uninstall.Substring(1, end - 1);
        }

        string[] parts = uninstall.Split(' ');
        if (parts.Length > 0 && parts[0].EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            return parts[0];

        return null;
    }

    private int CountSubKeys(string[] paths)
    {
        int count = 0;
        foreach (string path in paths)
        {
            using RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
            if (key != null)
                count += key.GetSubKeyNames().Length;
        }
        return count;
    }
}
