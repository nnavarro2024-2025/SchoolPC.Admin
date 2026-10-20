using SchoolPC.Admin.Models;
using SchoolPC.Admin.Services;
using SchoolPC.Admin.Data;
using SchoolPC.Admin.UI;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

namespace SchoolPC.Admin.Views
{
    public partial class PcLogbookDetailView : UserControl
    {
        private string _clientGuid;
        private PcInstance? _pc;
        private readonly ISessionService? _sessionService;
        private readonly IPcInstanceRepository? _pcRepository;
        private DataGridView _sessionsGridView = null!;
        private DateTimePicker _fromDatePicker = null!;
        private DateTimePicker _toDatePicker = null!;
        private Label _statusLabel = null!;
        private Label _errorLabel = null!;
        private Label _emptyStateLabel = null!;
        private Label _dateHeaderLabel = null!;
        private Button _prevDayButton = null!;
        private Button _nextDayButton = null!;
        private Button _toggleModeButton = null!;
        private DateTime _currentSingleDay;
        private bool _isRangeMode = false;

        /// <summary>Constructor for PcInstance mode (full mode - primary)</summary>
        public PcLogbookDetailView(PcInstance pc, ISessionService sessionService, IPcInstanceRepository pcRepository)
        {
            Debug.WriteLine($"[PcLogbookDetailView] Constructor called with PC GUID: {pc.ClientGuid}");
            _pc = pc;
            _clientGuid = pc.ClientGuid;
            _sessionService = sessionService;
            _pcRepository = pcRepository;
            _currentSingleDay = DateTime.Today;
            InitializeComponent();
            ConfigureUI();
            Debug.WriteLine($"[PcLogbookDetailView] Constructor completed");
        }

        private void ConfigureUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(15);

            // Use TableLayoutPanel for proper layout
            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

            // Pre-populate RowStyles collection to avoid IndexOutOfRangeException
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Row 0: Back Button (fixed 40px)
            var backButton = new Button
            {
                Text = "← Back to List",
                AutoSize = false,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Height = 40,
                Dock = DockStyle.Fill
            };
            backButton.Click += (s, e) => GoBackToList();
            tableLayout.Controls.Add(backButton, 0, 0);

            // Row 1: Title and PC Info (fixed 100px)
            var infoPanelRow = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 245, 250),
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };

            var titleLabel = new Label
            {
                Text = "PC Detail",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 102, 204),
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Top
            };
            infoPanelRow.Controls.Add(titleLabel);

            var nameLabel = new Label
            {
                Text = $"Display Name: {_pc.DisplayName ?? _pc.CurrentPcName}",
                AutoSize = false,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top
            };
            infoPanelRow.Controls.Add(nameLabel);

            var guidLabel = new Label
            {
                Text = $"Client GUID: {_clientGuid.Substring(0, 8)}...",
                AutoSize = false,
                Height = 25,
                Font = new Font("Segoe UI", 10),
                Dock = DockStyle.Top
            };
            infoPanelRow.Controls.Add(guidLabel);

            tableLayout.Controls.Add(infoPanelRow, 0, 1);

            // Row 2: Filter Bar (fixed 60px)
            var filterPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10),
                Height = 60
            };

            var filterFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoScroll = false,
                Padding = Padding.Empty,
                Margin = Padding.Empty
            };

            // Single-day mode: prev button, date header, next button
            _prevDayButton = new Button
            {
                Text = "<",
                Width = 40,
                Height = 30,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 5, 0)
            };
            _prevDayButton.Click += (s, e) => MoveToPreviousDay();
            filterFlow.Controls.Add(_prevDayButton);

            _dateHeaderLabel = new Label
            {
                Text = FormatSingleDayHeader(_currentSingleDay),
                AutoSize = false,
                Width = 300,
                Height = 30,
                Font = new Font("Segoe UI", 10),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 5, 0)
            };
            filterFlow.Controls.Add(_dateHeaderLabel);

            _nextDayButton = new Button
            {
                Text = ">",
                Width = 40,
                Height = 30,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };
            _nextDayButton.Click += (s, e) => MoveToNextDay();
            filterFlow.Controls.Add(_nextDayButton);

            // Toggle mode button
            _toggleModeButton = new Button
            {
                Text = "📅 Range Mode",
                Width = 120,
                Height = 30,
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 10, 0)
            };
            _toggleModeButton.Click += (s, e) => ToggleRangeMode();
            filterFlow.Controls.Add(_toggleModeButton);

            // Range mode controls (initially hidden)
            var fromLabel = new Label
            {
                Text = "From:",
                Width = 45,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 5, 0),
                Visible = false
            };
            filterFlow.Controls.Add(fromLabel);

            _fromDatePicker = new DateTimePicker
            {
                Width = 120,
                Height = 30,
                Value = DateTime.Today,
                Visible = false,
                Margin = new Padding(0, 0, 10, 0)
            };
            filterFlow.Controls.Add(_fromDatePicker);

            var toLabel = new Label
            {
                Text = "To:",
                Width = 30,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 5, 0),
                Visible = false
            };
            filterFlow.Controls.Add(toLabel);

            _toDatePicker = new DateTimePicker
            {
                Width = 120,
                Height = 30,
                Value = DateTime.Today,
                Visible = false,
                Margin = new Padding(0, 0, 10, 0)
            };
            filterFlow.Controls.Add(_toDatePicker);

            var applyButton = new Button
            {
                Text = "Apply",
                Width = 80,
                Height = 30,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Margin = new Padding(0, 0, 10, 0)
            };
            applyButton.Click += async (s, e) => await LoadSessions();
            filterFlow.Controls.Add(applyButton);

            var exportButton = new Button
            {
                Text = "📥 Export CSV",
                Width = 110,
                Height = 30,
                BackColor = Color.FromArgb(50, 150, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Margin = new Padding(0, 0, 0, 0)
            };
            exportButton.Click += async (s, e) => await ExportToCSV();
            filterFlow.Controls.Add(exportButton);

            // Store references to hide/show in ToggleRangeMode
            fromLabel.Tag = "fromLabel";
            toLabel.Tag = "toLabel";
            applyButton.Tag = "applyButton";

            filterPanel.Controls.Add(filterFlow);
            tableLayout.Controls.Add(filterPanel, 0, 2);

            // Row 3: Status/Error/Empty-State Line (fixed 50px)
            var statusPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(10),
                Height = 50
            };

            _statusLabel = new Label
            {
                Text = "Ready",
                AutoSize = false,
                Width = 400,
                Height = 20,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(0, 102, 204),
                Dock = DockStyle.Top
            };
            statusPanel.Controls.Add(_statusLabel);

            _errorLabel = new Label
            {
                Text = "",
                AutoSize = false,
                Width = 400,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Red,
                Dock = DockStyle.Top,
                Visible = false
            };
            statusPanel.Controls.Add(_errorLabel);

            _emptyStateLabel = new Label
            {
                Text = "No sessions in this range.",
                AutoSize = false,
                Width = 400,
                Height = 20,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                ForeColor = Color.Gray,
                Dock = DockStyle.Top,
                Visible = false
            };
            statusPanel.Controls.Add(_emptyStateLabel);

            tableLayout.Controls.Add(statusPanel, 0, 3);

            // Row 4: Grid (Fill remaining space)
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
                ColumnHeadersHeight = 36,
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(0, 102, 204),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                EnableHeadersVisualStyles = false
            };

            // Columns: Name, Email, Student ID, Time In, Time Out, Remarks
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentName", HeaderText = "Name", FillWeight = 15 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentEmail", HeaderText = "Email Address", FillWeight = 25 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentId", HeaderText = "Student ID / Username", FillWeight = 15 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeIn", HeaderText = "Time In", FillWeight = 15 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "TimeOut", HeaderText = "Time Out", FillWeight = 15 });
            _sessionsGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remarks", HeaderText = "Remarks", FillWeight = 15 });

            tableLayout.Controls.Add(_sessionsGridView, 0, 4);

            Controls.Add(tableLayout);

            // Load initial data in single-day mode
            _ = LoadSessions();
        }

        private string FormatSingleDayHeader(DateTime day)
        {
            return day.ToString("dddd MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
        }

        private void MoveToPreviousDay()
        {
            _currentSingleDay = _currentSingleDay.AddDays(-1);
            _dateHeaderLabel.Text = FormatSingleDayHeader(_currentSingleDay);
            _ = LoadSessions();
        }

        private void MoveToNextDay()
        {
            _currentSingleDay = _currentSingleDay.AddDays(1);
            _dateHeaderLabel.Text = FormatSingleDayHeader(_currentSingleDay);
            _ = LoadSessions();
        }

        private void ToggleRangeMode()
        {
            _isRangeMode = !_isRangeMode;
            _toggleModeButton.Text = _isRangeMode ? "📅 Single Day" : "📅 Range Mode";

            // Hide/show controls based on mode
            _prevDayButton.Visible = !_isRangeMode;
            _nextDayButton.Visible = !_isRangeMode;
            _dateHeaderLabel.Visible = !_isRangeMode;

            foreach (Control ctrl in _dateHeaderLabel.Parent.Controls)
            {
                if (ctrl.Tag?.ToString() == "fromLabel" || ctrl.Tag?.ToString() == "toLabel" || ctrl.Tag?.ToString() == "applyButton")
                {
                    ctrl.Visible = _isRangeMode;
                }
                else if (ctrl is DateTimePicker dtp && (dtp == _fromDatePicker || dtp == _toDatePicker))
                {
                    dtp.Visible = _isRangeMode;
                }
                else if (ctrl is Button btn && btn.Text == "Apply")
                {
                    btn.Visible = _isRangeMode;
                }
            }

            _ = LoadSessions();
        }

        private async Task LoadSessions()
        {
            Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] Starting - SessionService null? {_sessionService == null}, PC null? {_pc == null}, GridView null? {_sessionsGridView == null}");

            if (_sessionService == null || _pc == null || _sessionsGridView == null)
            {
                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] EARLY EXIT - Service or PC or GridView is null");
                return;
            }

            try
            {
                _sessionsGridView.Rows.Clear();
                _errorLabel.Visible = false;
                _emptyStateLabel.Visible = false;

                DateTime fromDate, toDate;
                if (_isRangeMode)
                {
                    fromDate = _fromDatePicker.Value.Date;
                    toDate = _toDatePicker.Value.Date;
                }
                else
                {
                    // Single-day mode: from 00:00 of _currentSingleDay to 23:59:59
                    fromDate = _currentSingleDay.Date;
                    toDate = _currentSingleDay.Date;
                }

                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] Fetching sessions for PC GUID: {_pc.ClientGuid}, From: {fromDate:yyyy-MM-dd}, To: {toDate:yyyy-MM-dd}");

                var sessions = await _sessionService.GetSessionsForPcAsync(_pc.ClientGuid, fromDate, toDate);

                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] Received {sessions.Count} sessions from service");

                // Update status label
                if (_isRangeMode)
                {
                    var fromStr = fromDate.ToString("MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
                    var toStr = toDate.ToString("MMMM d, yyyy", CultureInfo.GetCultureInfo("en-US"));
                    _statusLabel.Text = $"Loaded {sessions.Count} session(s) from {fromStr} to {toStr}";
                }
                else
                {
                    _statusLabel.Text = $"Loaded {sessions.Count} session(s) for {FormatSingleDayHeader(_currentSingleDay)}";
                }

                if (sessions.Count == 0)
                {
                    Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] No sessions found");
                    _emptyStateLabel.Visible = true;
                    return;
                }

                // Add rows to grid with Remarks logic
                foreach (var session in sessions)
                {
                    var timeInStr = session.LoginTime.ToString("dddd MM/dd/yyyy h:mm tt", CultureInfo.GetCultureInfo("en-US"));
                    var timeOutStr = "";
                    var remarks = "";

                    // Determine Time Out and Remarks based on end_reason and recovered_at
                    if (session.LogoutTime.HasValue)
                    {
                        timeOutStr = session.LogoutTime.Value.ToString("dddd MM/dd/yyyy h:mm tt", CultureInfo.GetCultureInfo("en-US"));

                        if (session.EndReason == "logout")
                        {
                            remarks = "";
                        }
                        else if (session.EndReason == "windows shutdown")
                        {
                            remarks = "Windows shutdown";
                        }
                        else if (session.EndReason == "PC turned off unexpectedly")
                        {
                            if (session.RecoveredAt.HasValue)
                            {
                                var recoveredStr = session.RecoveredAt.Value.ToString("dddd MM/dd/yyyy h:mm tt", CultureInfo.GetCultureInfo("en-US"));
                                remarks = $"PC turned off, back on at {recoveredStr}";
                            }
                            else
                            {
                                remarks = "PC turned off unexpectedly";
                            }
                        }
                    }
                    else
                    {
                        // logout_time is NULL - check if PC is online (within 2 minutes)
                        if (_pc.IsOnline)
                        {
                            timeOutStr = "In use";
                        }
                        else
                        {
                            timeOutStr = "PC offline";
                            remarks = "Not yet recovered";
                        }
                    }

                    _sessionsGridView.Rows.Add(
                        session.StudentName,
                        session.StudentEmail ?? "",
                        session.StudentId,
                        timeInStr,
                        timeOutStr,
                        remarks
                    );

                    Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] Added session row: {session.StudentName} at {timeInStr}");
                }

                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] Completed - Total rows in grid: {_sessionsGridView.Rows.Count}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] EXCEPTION: {ex.GetType().Name}: {ex.Message}");
                Debug.WriteLine($"[PcLogbookDetailView.LoadSessions] StackTrace: {ex.StackTrace}");
                _errorLabel.Text = $"Error: {ex.Message}";
                _errorLabel.Visible = true;
            }
        }

        private async Task ExportToCSV()
        {
            try
            {
                if (_sessionsGridView.Rows.Count == 0)
                {
                    MessageBox.Show("No sessions to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                // Collect all visible data
                var rows = new System.Collections.Generic.List<string>();
                rows.Add("Name,Email Address,Student ID / Username,Time In,Time Out,Remarks");

                foreach (DataGridViewRow row in _sessionsGridView.Rows)
                {
                    var name = row.Cells["StudentName"].Value?.ToString() ?? "";
                    var email = row.Cells["StudentEmail"].Value?.ToString() ?? "";
                    var studentId = row.Cells["StudentId"].Value?.ToString() ?? "";
                    var timeIn = row.Cells["TimeIn"].Value?.ToString() ?? "";
                    var timeOut = row.Cells["TimeOut"].Value?.ToString() ?? "";
                    var remarks = row.Cells["Remarks"].Value?.ToString() ?? "";

                    // Quote fields for CSV
                    rows.Add($"\"{name}\",\"{email}\",\"{studentId}\",\"{timeIn}\",\"{timeOut}\",\"{remarks}\"");
                }

                // Save dialog
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"sessions_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv",
                    Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                    DefaultExt = "csv"
                };

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Write with UTF-8 BOM for Excel
                    await System.IO.File.WriteAllLinesAsync(
                        saveFileDialog.FileName,
                        rows,
                        System.Text.Encoding.UTF8
                    );
                    MessageBox.Show("Export completed successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void GoBackToList()
        {
            var parent = Parent;
            while (parent != null && parent is not Form)
            {
                parent = parent.Parent;
            }

            if (parent is MainShellForm mainShell)
            {
                mainShell.ShowPcLogbook();
            }
        }

        private void PcLogbookDetailView_Load(object sender, EventArgs e)
        {

        }
    }
}
