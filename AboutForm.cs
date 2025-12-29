using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.IO;

namespace WinUninstallDoctor
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeUI();
        }
    private void InitializeUI()
    {
        Text = "About WinUninstallDoctor";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(520, 420);

        // Main container
        var panel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20)
        };

        // Logo
        var picLogo = new PictureBox
        {
            Image = ByteArrayToImage(Properties.Resources.logo),
            SizeMode = PictureBoxSizeMode.Zoom,
            Size = new Size(140, 140),
            Location = new Point((ClientSize.Width - 140) / 2, 10),
        };

        // App name
        var lblTitle = new Label
        {
            Text = "WinUninstallDoctor",
            Font = new Font("Segoe UI", 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point((ClientSize.Width - 260) / 2, 160)
        };

        // Version
        var lblVersion = new Label
        {
            Text = $"Version {GetVersion()}",
            Font = new Font("Segoe UI", 10),
            ForeColor = Color.Gray,
            AutoSize = true,
            Location = new Point((ClientSize.Width - 120) / 2, 200)
        };

        // Description
        var lblDescription = new Label
        {
            Text =
                "WinUninstallDoctor diagnoses and fixes broken uninstall entries\n" +
                "left behind by manually removed or corrupted applications.",
            Font = new Font("Segoe UI", 10),
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(460, 60),
            Location = new Point(20, 235)
        };

        // Separator
        var separator = new Label
        {
            BorderStyle = BorderStyle.Fixed3D,
            Height = 2,
            Width = 460,
            Location = new Point(20, 305)
        };

        // Contact / footer
        var lblFooter = new Label
        {
            Text = "Contact: support@mabslabs.com\n© 2025 WinUninstallDoctor",
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            TextAlign = ContentAlignment.MiddleCenter,
            Size = new Size(460, 50),
            Location = new Point(20, 320)
        };

        // Close button
        var btnClose = new Button
        {
            Text = "Close",
            Width = 90,
            Height = 30,
            Location = new Point((ClientSize.Width - 90) / 2, 375)
        };
        btnClose.Click += (_, _) => Close();

        panel.Controls.AddRange(new Control[]
        {
            picLogo,
            lblTitle,
            lblVersion,
            lblDescription,
            separator,
            lblFooter,
            btnClose
        });

        Controls.Add(panel);
    }

    public static Image ByteArrayToImage(byte[] byteArray)
    {
        using (var ms = new MemoryStream(byteArray))
        {
            return Image.FromStream(ms);
        }
    }

        private static string GetVersion()
        {
            return Assembly
                .GetExecutingAssembly()
                .GetName()
                .Version?
                .ToString() ?? "1.0.0";
        }
    }
}
