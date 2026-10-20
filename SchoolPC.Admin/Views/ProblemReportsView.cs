using SchoolPC.Admin.Services;
using System.Diagnostics;
using System.Windows.Forms;

namespace SchoolPC.Admin.Views
{
    public partial class ProblemReportsView : UserControl
    {
        private readonly string _currentAdminUsername;
        private readonly IProblemReportService _reportService;
        private DataGridView _reportsGridView = null!;
        private ComboBox _statusFilterCombo = null!;

        public ProblemReportsView(string adminUsername, IProblemReportService reportService)
        {
            _currentAdminUsername = adminUsername;
            _reportService = reportService;
            InitializeComponent();
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            this.SuspendLayout();
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;
            this.AutoScaleMode = AutoScaleMode.Font;

            // Use TableLayoutPanel for reliable layout control
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                BackColor = Color.White,
                Padding = new Padding(15, 15, 15, 15),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                AutoSize = false
            };

            // Row 0: Title
            var titleLabel = new Label
            {
                Text = "Problem Reports",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = true,
                Height = 50
            };
            mainLayout.Controls.Add(titleLabel, 0, 0);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Row 1: Filter panel
            var filterPanel = new Panel
            {
                BackColor = Color.FromArgb(240, 240, 240),
                Height = 50,
                AutoSize = false
            };

            var statusLabel = new Label
            {
                Text = "Status:",
                Location = new Point(10, 12),
                Width = 50,
                Height = 25,
                Font = new Font("Segoe UI", 9)
            };
            _statusFilterCombo = new ComboBox
            {
                Location = new Point(65, 12),
                Width = 120,
                Height = 25,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _statusFilterCombo.Items.AddRange(new[] { "All", "Open", "Resolved" });
            _statusFilterCombo.SelectedIndex = 1;
            _statusFilterCombo.SelectedIndexChanged += (s, e) => LoadReports();

            filterPanel.Controls.AddRange(new Control[] { statusLabel, _statusFilterCombo });
            mainLayout.Controls.Add(filterPanel, 0, 1);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            // Row 2: Grid (Fill remaining space)
            _reportsGridView = new DataGridView
            {
                Name = "ReportsGridView",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeight = 30,
                EnableHeadersVisualStyles = false
            };

            // Style column headers
            _reportsGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            // Columns
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "PC", HeaderText = "PC Name" });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Student", HeaderText = "Student Name" });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Problem Description" });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "Reported At" });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status" });
            _reportsGridView.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "Action", Text = "Mark Resolved", UseColumnTextForButtonValue = true });

            _reportsGridView.CellClick += ReportsGridView_CellClick;

            mainLayout.Controls.Add(_reportsGridView, 0, 2);
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Auto-fill remaining space

            this.Controls.Add(mainLayout);

            this.ResumeLayout(true);

            Debug.WriteLine("[ConfigureUI] ProblemReportsView initialized");
            LoadReports();
        }

        private async void LoadReports()
        {
            try
            {
                _reportsGridView.Rows.Clear();

                // Get the selected status filter
                string? statusFilter = _statusFilterCombo.SelectedItem?.ToString();
                if (statusFilter == "All")
                {
                    statusFilter = null;
                }

                // Fetch reports from the database using the service
                var reports = await _reportService.GetReportsByStatusAsync(statusFilter);

                // Populate grid with real data
                foreach (var report in reports)
                {
                    var rowIndex = _reportsGridView.Rows.Add(
                        report.PcName,
                        report.StudentName,
                        report.Message,
                        report.CreatedAt.ToString("g"),
                        report.Status ?? "open",
                        "Mark Resolved"
                    );

                    // Store ReportId in the row's Tag for later retrieval
                    _reportsGridView.Rows[rowIndex].Tag = report.ReportId;
                }

                Debug.WriteLine($"[LoadReports] Loaded {reports.Count} reports with status filter: {statusFilter ?? "null"}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[LoadReports] Error: {ex.Message}");
            }
        }

        private async void ReportsGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == _reportsGridView.Columns["Action"].Index && e.RowIndex >= 0)
            {
                var selectedRow = _reportsGridView.Rows[e.RowIndex];
                var status = selectedRow.Cells["Status"].Value?.ToString();
                var reportId = selectedRow.Tag as int?;

                if (!reportId.HasValue)
                {
                    MessageBox.Show("Error: Could not identify report. Please refresh and try again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (status == "resolved" || status == "Resolved")
                {
                    MessageBox.Show("This report is already resolved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show("Mark this report as resolved?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    try
                    {
                        // Call the service to update the database
                        await _reportService.MarkReportResolvedAsync(reportId.Value, _currentAdminUsername);

                        // Update UI to reflect the change
                        selectedRow.Cells["Status"].Value = "resolved";
                        MessageBox.Show("Report marked as resolved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Refresh the grid to ensure data is current
                        LoadReports();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error marking report as resolved: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Debug.WriteLine($"[ReportsGridView_CellClick] Error: {ex.Message}");
                    }
                }
            }
        }
    }
}
