using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;


namespace WinUninstallDoctor
{
    public class MainForm : Form
    {
        private FlowLayoutPanel flpPrograms;
        private Button btnScan;
        private Button btnDeleteSelected;
        private ProgressBar progressBarScan;
        private Label lblStatus;

        private TextBox txtSearch;
        private CheckBox chkOnlyBroken;
        private CheckBox chkHideSystem;

        private readonly List<ProgramEntry> allPrograms = new();
        private List<ProgramEntry> filteredPrograms = new();

        // Timer pour le debouncing de la recherche
        private System.Threading.Timer searchDebounceTimer;
        private const int DEBOUNCE_DELAY_MS = 300;

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
                Height = 535,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
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


            Button btnAbout = new Button
            {
                Text = "",
                Size = new Size(36, 36),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(ClientSize.Width - 50, 40),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                TextAlign = ContentAlignment.MiddleCenter,

            };
            btnAbout.Image = WinUninstallDoctor.AboutForm.ByteArrayToImage(WinUninstallDoctor.Properties.Resources.info);
            btnAbout.ImageAlign = ContentAlignment.MiddleCenter;
            btnAbout.Text = "";
            btnAbout.FlatAppearance.BorderSize = 0;
            btnAbout.BackColor = Color.Transparent;
            btnAbout.Cursor = Cursors.Hand;

            btnAbout.Click += (_, _) =>
            {
                using var about = new AboutForm();
                about.ShowDialog(this);
            };


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
            btnAbout
            });

            // DEBOUNCING pour la recherche texte
            txtSearch.TextChanged += TxtSearch_TextChanged;

            // Filtrage immédiat pour les checkboxes
            chkOnlyBroken.CheckedChanged += (_, _) => ApplyFilters();
            chkHideSystem.CheckedChanged += (_, _) => ApplyFilters();
        }

        // Gestion du debouncing pour la recherche
        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            // Annuler le timer précédent s'il existe
            searchDebounceTimer?.Dispose();

            // Indicateur visuel pendant la recherche
            if (!string.IsNullOrEmpty(txtSearch.Text))
            {
                lblStatus.Text = "Searching...";
                lblStatus.ForeColor = Color.DarkOrange;
            }

            // Créer un nouveau timer qui s'exécutera après le délai
            searchDebounceTimer = new System.Threading.Timer(
                callback: _ =>
                {
                    if (!IsDisposed)
                    {
                        Invoke(() =>
                        {
                            ApplyFilters();
                            lblStatus.Text = $"Found {filteredPrograms.Count} programs";
                            lblStatus.ForeColor = Color.Green;
                        });
                    }
                },
                state: null,
                dueTime: DEBOUNCE_DELAY_MS,
                period: Timeout.Infinite
            );
        }

        void AdjustFlowLayoutHeight()
        {
            int totalHeight = 0;

            foreach (Control c in flpPrograms.Controls)
                totalHeight += c.Height + c.Margin.Vertical;

            totalHeight += 5;

            flpPrograms.Height = Math.Min(
                totalHeight,
                ClientSize.Height - flpPrograms.Top - 10
            );
        }

        private async System.Threading.Tasks.Task StartScanAsync()
        {
            btnScan.Enabled = false;
            btnDeleteSelected.Enabled = false;
            flpPrograms.Controls.Clear();
            allPrograms.Clear();
            filteredPrograms.Clear();

            lblStatus.Text = "Scanning registry...";
            lblStatus.ForeColor = Color.Black;
            progressBarScan.Value = 0;

            await System.Threading.Tasks.Task.Run(ScanPrograms);

            //AdjustFlowLayoutHeight();

            lblStatus.Text = $"Scan finished ({allPrograms.Count} programs)";
            lblStatus.ForeColor = Color.Green;
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
            if (string.IsNullOrWhiteSpace(uninstall))
                return null;

            uninstall = uninstall.Trim();

            if (uninstall.StartsWith("\""))
            {
                int end = uninstall.IndexOf("\"", 1);
                if (end > 1)
                    return uninstall.Substring(1, end - 1);
            }

            var exeIndex = uninstall.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            if (exeIndex > 0)
                return uninstall.Substring(0, exeIndex + 4);

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

        // Fonction de filtrage optimisée
        private void ApplyFilters()
        {
            if (allPrograms == null || allPrograms.Count == 0)
                return;

            string search = txtSearch.Text.Trim().ToLowerInvariant();

            filteredPrograms = allPrograms.Where(p =>
            {
                // Filtre: Recherche texte
                bool matchSearch = string.IsNullOrEmpty(search) ||
                    (p.DisplayName?.ToLowerInvariant().Contains(search) ?? false) ||
                    (p.Publisher?.ToLowerInvariant().Contains(search) ?? false) ||
                    (p.DisplayVersion?.ToLowerInvariant().Contains(search) ?? false);

                if (!matchSearch)
                    return false;

                // Filtre: Masquer les composants système
                if (chkHideSystem.Checked && p.IsSystemComponent)
                    return false;

                // Filtre: Afficher uniquement les programmes cassés
                if (chkOnlyBroken.Checked && !p.HasProblem)
                    return false;

                return true;
            })
            .ToList();

            RefreshProgramList();
        }

        // Rafraîchissement optimisé de la liste
        private void RefreshProgramList()
        {
            flpPrograms.SuspendLayout();

            try
            {
                // Réutiliser les contrôles existants au lieu de tout recréer
                var existingControls = flpPrograms.Controls
                    .Cast<ProgramItemControl>()
                    .ToDictionary(c => c._entry, c => c);

                flpPrograms.Controls.Clear();

                foreach (var program in filteredPrograms)
                {
                    ProgramItemControl ctrl;

                    // Réutiliser le contrôle existant si disponible
                    if (existingControls.TryGetValue(program, out var existing))
                    {
                        ctrl = existing;
                    }
                    else
                    {
                        // Créer un nouveau contrôle seulement si nécessaire
                        ctrl = new ProgramItemControl();
                        ctrl.Bind(program);
                        ctrl.SelectionChanged += UpdateDeleteButtonState;
                    }

                    flpPrograms.Controls.Add(ctrl);
                }

                // Nettoyer les contrôles non réutilisés
                foreach (var unused in existingControls.Values.Where(c => !flpPrograms.Controls.Contains(c)))
                {
                    unused.Dispose();
                }
            }
            finally
            {
                flpPrograms.ResumeLayout();
                //AdjustFlowLayoutHeight();
            }

            UpdateDeleteButtonState();
        }

        // Nettoyer le timer à la fermeture
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                searchDebounceTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

}