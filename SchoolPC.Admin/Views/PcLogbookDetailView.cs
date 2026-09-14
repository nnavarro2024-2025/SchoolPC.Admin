using SchoolPC.Admin.Models;
using SchoolPC.Admin.Services;
using System.Windows.Forms;

namespace SchoolPC.Admin.Views
{
    public class PcLogbookDetailView : UserControl
    {
        private readonly PcInstance _pc;
        private readonly ISessionService _sessionService;
        private DataGridView _sessionsGridView = null!;
        private DateTimePicker _fromDatePicker = null!;
        private DateTimePicker _toDatePicker = null!;

        public PcLogbookDetailView(PcInstance pc, ISessionService sessionService)
        {
            _pc = pc;
            _sessionService = sessionService;
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
                Text = $"PC Logbook - {_pc.DisplayName ?? _pc.CurrentPcName}",
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
                Height = 60,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };

            // From Date
            var fromLabel = new Label { Text = "From:", Location = new Point(10, 10), Width = 40, Height = 30 };
            _fromDatePicker = new DateTimePicker
            {
                Location = new Point(55, 10),
                Width = 120,
                Height = 30,
                Value = DateTime.Today
            };
            filterPanel.Controls.AddRange(new Control[] { fromLabel, _fromDatePicker });

            // To Date
            var toLabel = new Label { Text = "To:", Location = new Point(180, 10), Width = 30, Height = 30 };
            _toDatePicker = new DateTimePicker
            {
                Location = new Point(215, 10),
                Width = 120,
                Height = 30,
                Value = DateTime.Today
            };
            filterPanel.Controls.AddRange(new Control[] { toLabel, _toDatePicker });

            // Filter Button
            var filterButton = new Button
            {
                Text = "Apply Filter",
                Location = new Point(345, 10),
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            filterButton.Click += (s, e) => LoadSessions();
            filterPanel.Controls.Add(filterButton);

            // Export Button
            var exportButton = new Button
            {
                Text = "Export to CSV",
                Location = new Point(455, 10),
                Width = 120,
                Height = 30,
                BackColor = Color.FromArgb(0, 150, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            exportButton.Click += ExportToCsv_Click;
            filterPanel.Controls.Add(exportButton);

            Controls.Add(filterPanel);

            // Sessions DataGridView
            _sessionsGridView = new DataGridView
            {
                Name = "SessionsGridView",
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
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentName", HeaderText = "Student Name", Width = 200 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentId", HeaderText = "Student ID", Width = 120 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Email", HeaderText = "Email Address", Width = 200 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn", HeaderText = "Time In", Width = 150 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Time Out", Width = 150 });

            Controls.Add(_sessionsGridView);

            LoadSessions();
        }

        private async void LoadSessions()
        {
            try
            {
                _sessionsGridView.Rows.Clear();

                var sessions = await _sessionService.GetSessionsForPcAsync(_pc.ClientGuid, _fromDatePicker.Value, _toDatePicker.Value);

                foreach (var session in sessions)
                {
                    var email = $"{session.StudentId}@uic.edu.ph";
                    var timeIn = session.LoginTime.ToString("g");
                    var timeOut = session.LogoutTime?.ToString("g") ?? "Still Logged In";

                    _sessionsGridView.Rows.Add(session.StudentName, session.StudentId, email, timeIn, timeOut);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading sessions: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ExportToCsv_Click(object? sender, EventArgs e)
        {
            try
            {
                var sessions = await _sessionService.GetSessionsForPcAsync(_pc.ClientGuid, _fromDatePicker.Value, _toDatePicker.Value);
                var csv = await _sessionService.ExportSessionsToCsvAsync(sessions, _pc);

                var saveDialog = new SaveFileDialog
                {
                    FileName = $"PC_{_pc.DisplayName ?? _pc.CurrentPcName}_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveDialog.FileName, csv);
                    MessageBox.Show("Sessions exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
