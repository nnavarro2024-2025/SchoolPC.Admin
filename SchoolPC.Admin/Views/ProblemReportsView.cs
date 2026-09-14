using SchoolPC.Admin.Services;
using System.Windows.Forms;

namespace SchoolPC.Admin.Views
{
    public class ProblemReportsView : UserControl
    {
        private readonly string _currentAdminUsername;
        private readonly IProblemReportService _reportService;
        private DataGridView _reportsGridView = null!;
        private ComboBox _statusFilterCombo = null!;

        public ProblemReportsView(string adminUsername)
        {
            _currentAdminUsername = adminUsername;
            // TODO: Inject service properly via DI
            _reportService = null!; // Placeholder
            InitializeComponent();
            ConfigureUI();
        }

        private void InitializeComponent()
        {
            // Will be called by constructor
        }

        private void ConfigureUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(15);

            // Title
            var titleLabel = new Label
            {
                Text = "Problem Reports",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.FromArgb(0, 102, 204)
            };
            Controls.Add(titleLabel);

            // Filter Panel
            var filterPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            var statusLabel = new Label { Text = "Status:", Location = new Point(10, 12), Width = 50, Height = 25 };
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
            Controls.Add(filterPanel);

            // Reports DataGridView
            _reportsGridView = new DataGridView
            {
                Name = "ReportsGridView",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeight = 30
            };

            // Columns
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "PC", HeaderText = "PC Name", Width = 150 });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Student", HeaderText = "Student Name", Width = 200 });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Message", HeaderText = "Problem Description", Width = 300 });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Timestamp", HeaderText = "Reported At", Width = 150 });
            _reportsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 100 });
            _reportsGridView.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "Action", Text = "Mark Resolved", Width = 120, UseColumnTextForButtonValue = true });

            _reportsGridView.CellClick += ReportsGridView_CellClick;
            Controls.Add(_reportsGridView);

            LoadReports();
        }

        private async void LoadReports()
        {
            try
            {
                _reportsGridView.Rows.Clear();

                // Placeholder data
                _reportsGridView.Rows.Add("LAB-PC-001", "NIKKO ANGELO P. NAVARRO", "Monitor flickering", DateTime.Now.ToString("g"), "Open", "Mark Resolved");
                _reportsGridView.Rows.Add("LAB-PC-002", "KYLE CLANCY F. TAN", "Keyboard not working", DateTime.Now.AddHours(-2).ToString("g"), "Open", "Mark Resolved");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading reports: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReportsGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == _reportsGridView.Columns["Action"].Index && e.RowIndex >= 0)
            {
                var selectedRow = _reportsGridView.Rows[e.RowIndex];
                var status = selectedRow.Cells["Status"].Value?.ToString();

                if (status == "Resolved")
                {
                    MessageBox.Show("This report is already resolved.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                DialogResult result = MessageBox.Show("Mark this report as resolved?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // TODO: Call service to mark resolved
                    selectedRow.Cells["Status"].Value = "Resolved";
                    MessageBox.Show("Report marked as resolved.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
