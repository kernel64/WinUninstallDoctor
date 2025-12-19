using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class ProgramItemControl : UserControl
{
    private PictureBox picIcon;
    private Label lblName;
    private Label lblVersion;
    private Label lblPublisher;
    private CheckBox chkSelect;
    private Button btnDelete;
    private Button btnFixPath;

    private ProgramEntry _entry;

    public event Action SelectionChanged;

    public bool IsChecked => chkSelect.Visible && chkSelect.Checked;

    public ProgramItemControl()
    {
        InitializeUI();
    }

    private void InitializeUI()
    {
        Height = 64;
        Width = 1000;
        Margin = new Padding(2);
        BackColor = Color.White;

        chkSelect = new CheckBox
        {
            Location = new Point(10, 22),
            Enabled = false
        };
        chkSelect.CheckedChanged += (_, _) => SelectionChanged?.Invoke();

        picIcon = new PictureBox
        {
            Size = new Size(48, 48),
            Location = new Point(40, 8),
            SizeMode = PictureBoxSizeMode.CenterImage
        };

        lblName = new Label
        {
            Location = new Point(95, 6),
            AutoSize = false,
            Width = 420,
            Height = 25,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
        };

        lblVersion = new Label
        {
            Location = new Point(95, 32),
            Width = 120,
            ForeColor = Color.DimGray
        };

        lblPublisher = new Label
        {
            Location = new Point(180, 32),
            Width = 260,
            ForeColor = Color.DimGray
        };

        btnFixPath = new Button
        {
            Text = "Fix path",
            Width = 80,
            Height = 33,
            Location = new Point(810, 14),
            Visible = false
        };
        btnFixPath.Click += BtnFixPath_Click;

        btnDelete = new Button
        {
            Text = "Delete",
            Width = 80,
            Height = 33,
            Location = new Point(900, 14),
            Visible = false
        };
        btnDelete.Click += BtnDelete_Click;

        Controls.AddRange(new Control[]
        {
            picIcon,
            lblName,
            lblVersion,
            lblPublisher,
            chkSelect,
            btnFixPath,
            btnDelete
        });
    }

    // ===============================
    // Public API
    // ===============================

    public void Bind(ProgramEntry entry)
    {
        _entry = entry;

        lblName.Text = entry.DisplayName;
        lblVersion.Text = entry.DisplayVersion ?? "";
        lblPublisher.Text = entry.Publisher ?? "";

        picIcon.Image = LoadIcon(entry.DisplayIcon);

        bool broken = !entry.UninstallExists;

        chkSelect.Enabled = broken;
        btnDelete.Visible = broken;
        btnFixPath.Visible = broken;

        if (broken)
        {
            BackColor = Color.MistyRose;
        }
        else
        {
            BackColor = Color.White;
        }
    }

    public void DeleteEntry()
    {
        try
        {
            Registry.LocalMachine.DeleteSubKeyTree(_entry.RegistryKeyPath, false);
            Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to delete registry entry.\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ===============================
    // Button actions
    // ===============================

    private void BtnDelete_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show(
            $"Remove '{_entry.DisplayName}' from installed programs?\n\n" +
            "This will NOT uninstall the application.",
            "Confirm deletion",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        DeleteEntry();
        SelectionChanged?.Invoke();
    }

    private void BtnFixPath_Click(object sender, EventArgs e)
    {
        string oldExeName = Path.GetFileName(ExtractExecutablePath(_entry.UninstallString));

        OpenFileDialog ofd = new()
        {
            Filter = "Executable (*.exe)|*.exe",
            Title = "Locate uninstall executable",
            FileName = oldExeName
        };

        if (ofd.ShowDialog() != DialogResult.OK)
            return;

        try
        {
            using RegistryKey key =
                Registry.LocalMachine.OpenSubKey(_entry.RegistryKeyPath, true);

            key.SetValue("UninstallString", $"\"{ofd.FileName}\"");

            MessageBox.Show(
                "Uninstall path corrected successfully.",
                "Success",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            // Mark as fixed
            chkSelect.Visible = false;
            btnDelete.Visible = false;
            btnFixPath.Visible = false;
            BackColor = Color.White;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to update uninstall path.\n\n{ex.Message}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    // ===============================
    // Helpers
    // ===============================

    private Image LoadIcon(string displayIcon)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(displayIcon))
            {
                string path = displayIcon;

                int comma = path.IndexOf(",");
                if (comma > 0)
                    path = path.Substring(0, comma);

                path = path.Trim('"');

                if (File.Exists(path))
                {
                    Icon icon = Icon.ExtractAssociatedIcon(path);
                    return icon.ToBitmap();
                }
            }
        }
        catch
        {
            // Ignore
        }

        return SystemIcons.Application.ToBitmap();
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

        string[] parts = uninstall.Split(' ');
        if (parts.Length > 0 && parts[0].EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            return parts[0];

        return null;
    }
}
