using SchoolPC.Admin.Models;
using SchoolPC.Admin.Views;
using SchoolPC.Admin.Services;
using SchoolPC.Admin.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;
using System.Diagnostics;

namespace SchoolPC.Admin.UI
{
    public partial class MainShellForm : Form
    {
        private readonly AdminAccount _currentAdmin;
        private readonly IServiceProvider _serviceProvider;
        private Panel _contentPanel = null!;
        private Panel _sidebarPanel = null!;
        private UserControl? _currentView;

        public MainShellForm(AdminAccount admin, IServiceProvider serviceProvider)
        {
            _currentAdmin = admin;
            _serviceProvider = serviceProvider;
            ConfigureUI();
            Load += MainShellForm_Load;
        }

        private void MainShellForm_Load(object? sender, EventArgs e)
        {
            // Set SplitterDistance in Load event (after form handle is created)
            // Find the SplitContainer and set the splitter distance
            foreach (Control ctrl in Controls)
            {
                if (ctrl is SplitContainer sc)
                {
                    sc.SplitterDistance = 220;
                    break;
                }
            }
        }

        private void ConfigureUI()
        {
            Text = $"SchoolPC Admin - {_currentAdmin.FullName}";
            Size = new Size(1400, 800);
            Font = new Font("Segoe UI", 9);
            FormBorderStyle = FormBorderStyle.Sizable;
            StartPosition = FormStartPosition.CenterScreen;

            // Create main layout: sidebar + content
            var mainLayout = new SplitContainer
            {
                Dock = DockStyle.Fill,
                FixedPanel = FixedPanel.Panel1,
                BorderStyle = BorderStyle.Fixed3D,
                IsSplitterFixed = true,  // ← LOCK: Prevent manual splitter drag
                Panel1MinSize = 220
            };

            // SIDEBAR (Left Panel) - Use FlowLayoutPanel for proper control layout and spacing
            var sidebarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };
            _sidebarPanel = mainLayout.Panel1;
            _sidebarPanel.BackColor = Color.FromArgb(240, 240, 240);
            _sidebarPanel.Controls.Add(sidebarFlow);

            // Logo/Title in sidebar
            var logoLabel = new Label
            {
                Text = "SchoolPC",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = false,
                Width = 200,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 15)  // Bottom margin for spacing
            };
            sidebarFlow.Controls.Add(logoLabel);

            // Menu buttons - FlowLayoutPanel respects Margin for proper spacing
            var buttons = new (string text, string name, Action clickHandler)[]
            {
                ("PC Logbook", "PcLogbookButton", () => ShowPcLogbook()),
                ("Problem Reports", "ProblemReportsButton", () => ShowProblemReports()),
                ("Account Management", "AccountMgmtButton", () => ShowAccountManagement()),
                ("Logout", "LogoutButton", () => Logout())
            };

            foreach (var (text, name, clickHandler) in buttons)
            {
                var btn = new Button
                {
                    Text = text,
                    Name = name,
                    Width = 190,
                    Height = 45,
                    BackColor = text == "Logout" ? Color.FromArgb(200, 50, 50) : Color.FromArgb(0, 102, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 0, 0, 10)  // Space between buttons (FlowLayoutPanel respects this)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += (s, e) => clickHandler?.Invoke();
                sidebarFlow.Controls.Add(btn);
            }

            // CONTENT PANEL (Right Panel)
            _contentPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };

            mainLayout.Panel2.Controls.Add(_contentPanel);

            Controls.Add(mainLayout);

            // Show PC Logbook by default
            ShowPcLogbook();
        }

        public void ShowPcLogbook()
        {
            var pcService = _serviceProvider.GetRequiredService<IPcInstanceService>();
            SwapView(new PcLogbookListView(pcService));
        }

        public async void ShowPcLogbookDetail(string clientGuid)
        {
            try
            {
                var pcService = _serviceProvider.GetRequiredService<IPcInstanceService>();
                var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
                var pcRepository = _serviceProvider.GetRequiredService<IPcInstanceRepository>();

                // Fetch the PC instance from the database
                var pc = await pcService.GetPcByGuidAsync(clientGuid);

                if (pc == null)
                {
                    MessageBox.Show($"PC with ID {clientGuid} not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create detail view with full data and services
                SwapView(new PcLogbookDetailView(pc, sessionService, pcRepository));
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error loading PC details: {ex.Message}\n\nFull Exception:\n{ex}";
                Debug.WriteLine($"[MainShellForm.ShowPcLogbookDetail] EXCEPTION: {ex}");
                MessageBox.Show(errorMsg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowProblemReports()
        {
            var reportService = _serviceProvider.GetRequiredService<IProblemReportService>();
            SwapView(new ProblemReportsView(_currentAdmin.Username, reportService));
        }

        public void ShowAccountManagement()
        {
            SwapView(new AccountManagementView());
        }

        private void SwapView(UserControl newView)
        {
            _contentPanel.Controls.Clear();
            _currentView = newView;
            newView.Dock = DockStyle.Fill;
            _contentPanel.Controls.Add(newView);
        }

        private void Logout()
        {
            DialogResult result = MessageBox.Show("Are you sure you want to logout?", "Confirm Logout", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Close();
            }
        }
    }
}
