using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WinUninstallDoctor;

public class MainForm : Form
{
    private FlowLayoutPanel flpPrograms;
    private Button btnScan;
    private Button btnDeleteSelected;
    private ProgressBar progressBarScan;
    private Label lblStatus;

    private Label lblFilterCounter;
    private TextBox txtSearch;
    private CheckBox chkOnlyBroken;
    private CheckBox chkHideSystem;
    private CheckBox chkOnlySelectable;

    private readonly List<ProgramEntry> allPrograms = new();
    private List<ProgramEntry> filteredPrograms = new();

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

        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimumSize = new Size(Width, Height);
        MaximumSize = new Size(Width, Height);

        var stream = new MemoryStream(WinUninstallDoctor.Properties.Resources.icon);
        
        Icon = new Icon(stream);

        btnScan = new Button
        {
            Text = "Scan programs",
            Width = 140,
            Height = 32,
            Location = new Point(15, 40)
        };
        btnScan.Click += async (_, _) => await StartScanAsync();

        btnDeleteSelected = new Button
        {
            Text = "Delete selected",
            Width = 150,
            Height = 32,
            Location = new Point(165, 40),
            Enabled = false
        };
        btnDeleteSelected.Click += BtnDeleteSelected_Click;

        progressBarScan = new ProgressBar
        {
            Location = new Point(330, 45),
            Width = 350,
            Height = 22
        };

        lblStatus = new Label
        {
            Text = "Idle",
            Location = new Point(700, 47),
            AutoSize = true
        };

        flpPrograms = new FlowLayoutPanel
        {
            Location = new Point(15, 132),
            Width = ClientSize.Width - 30,
            Height = ClientSize.Height - 80,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
        };


        txtSearch = new TextBox
        {
            PlaceholderText = "Search programs...",
            Width = 300,
            Enabled = false,
            Location = new Point(15, 87)
        };

        chkOnlyBroken = new CheckBox
        {
            Text = "Only broken",
            Width = 150,
            Height = 30,
            Enabled = false,
            Location = new Point(330, 90)
        };

        chkHideSystem = new CheckBox
        {
            Text = "Hide system components",
            Width = 240,
            Height = 30,
            Enabled = false,
            Location = new Point(500, 90),
        };

        lblFilterCounter = new Label
        {
            Text = "",
            Location = new Point(740, 90),
            AutoSize = true
        };


        MenuStrip menu = new MenuStrip();

        ToolStripMenuItem helpMenu = new ToolStripMenuItem("Help");
        ToolStripMenuItem aboutItem = new ToolStripMenuItem("About WinUninstallDoctor");

        aboutItem.Click += (_, _) =>
        {
            using var about = new AboutForm();
            about.ShowDialog(this);
        };

        helpMenu.DropDownItems.Add(aboutItem);
        menu.Items.Add(helpMenu);

        MainMenuStrip = menu;

        Controls.AddRange(new Control[]
        {
            menu,
            btnScan,
            btnDeleteSelected,
            progressBarScan,
            lblStatus,
            flpPrograms,
            txtSearch,
            chkOnlyBroken,
            chkHideSystem,
            lblFilterCounter
        });

        txtSearch.TextChanged += (_, _) => ApplyFilters();
        chkOnlyBroken.CheckedChanged += (_, _) => ApplyFilters();
        chkHideSystem.CheckedChanged += (_, _) => ApplyFilters();


    }

    void AdjustFlowLayoutHeight()
    {
        int totalHeight = 0;

        foreach (Control c in flpPrograms.Controls)
            totalHeight += c.Height + c.Margin.Vertical;

        // Add small padding
        totalHeight += 5;

        flpPrograms.Height = Math.Min(
            totalHeight,
            ClientSize.Height - flpPrograms.Top - 10
        );
    }


    private async Task StartScanAsync()
    {
        btnScan.Enabled = false;
        btnDeleteSelected.Enabled = false;
        flpPrograms.Controls.Clear();
        allPrograms.Clear();
        filteredPrograms.Clear();

        lblStatus.Text = "Scanning registry...";
        progressBarScan.Value = 0;

        await Task.Run(ScanPrograms);

        AdjustFlowLayoutHeight();

        lblStatus.Text = $"Scan finished ({allPrograms.Count} programs)";
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
                    RegistryKeyPath = path + "\\" + subKeyName,
                    IsSystemComponent = (appKey.GetValue("SystemComponent") as int? ?? 0) != 0,
                    IsSelectable = (appKey.GetValue("NoRemove") as int? ?? 0) == 0,
                    HasProblem = string.IsNullOrWhiteSpace(appKey.GetValue("UninstallString") as string)
                        || !CheckUninstall(appKey.GetValue("UninstallString") as string)
                };

                entry.UninstallExists = CheckUninstall(entry.UninstallString);
                allPrograms.Add(entry);

                Invoke(() =>
                {
                    AddProgramControl(entry);
                    // update progress bar
                    progressBarScan.Value = Math.Min(100, (++current * 100) / total);
                });
            }

            if (allPrograms.Count > 0)
            {
                txtSearch.Enabled = true;
                chkHideSystem.Enabled = true;
                chkOnlyBroken.Enabled = true;
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

    private void ApplyFilters()
    {
        if (allPrograms == null) return;

        string search = txtSearch.Text.Trim().ToLowerInvariant();
        filteredPrograms.Clear();

        filteredPrograms = allPrograms.Where(p =>
        {
            // Recherche texte
            bool matchSearch =
                string.IsNullOrEmpty(search) ||
                (p.DisplayName?.ToLowerInvariant().Contains(search) ?? false) ||
                (p.Publisher?.ToLowerInvariant().Contains(search) ?? false) ||
                (p.DisplayVersion?.ToLowerInvariant().Contains(search) ?? false);

            // Programmes système
            if (chkHideSystem.Checked && p.IsSystemComponent)
                return false;

            // Programmes cassés
            if (chkOnlyBroken.Checked && !p.HasProblem)
                return false;

            // Sélectionnables uniquement
            //if (chkOnlySelectable.Checked && !p.IsSelectable)
            //    return false;

            return matchSearch;
        })
        .ToList();

        lblFilterCounter.Text = $"(Showing {filteredPrograms.Count} of {allPrograms.Count} programs)";
        RefreshProgramList();
    }

    private void InitializeComponent()
    {

    }

    private void RefreshProgramList()
    {
        flpPrograms.SuspendLayout();
        flpPrograms.Controls.Clear();

        foreach (var program in filteredPrograms)
        {
            AddProgramControl(program);
        }

        flpPrograms.ResumeLayout();
    }

}
