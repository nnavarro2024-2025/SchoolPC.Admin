using SchoolPC.Admin.Models;
using SchoolPC.Admin.Services;
using SchoolPC.Admin.UI;
using System.Windows.Forms;
using System.Diagnostics;

namespace SchoolPC.Admin.Views
{
    public partial class PcLogbookListView : UserControl
    {
        private IPcInstanceService _pcService;
        private List<PcInstance> _allPcs = new();

        public PcLogbookListView(IPcInstanceService pcService)
        {
            _pcService = pcService;
            InitializeComponent();
            ConfigureColumns();
            Debug.WriteLine("[PcLogbookListView] Initialized");
            LoadData();
        }

        private void ConfigureColumns()
        {
            // Style column headers
            PcGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            };

            // Add columns
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "Display Name" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentPcName", HeaderText = "Machine Name" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "IpAddress", HeaderText = "IP Address" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastSeen", HeaderText = "Last Seen" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Online", HeaderText = "Online" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "ClientGuid", HeaderText = "Client ID" });
            PcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status" });
        }

        private async void ShowInactiveCheckbox_CheckedChanged(object? sender, EventArgs e)
        {
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                // Clear error label
                ErrorLabel.Visible = false;
                ErrorLabel.Height = 0;

                Debug.WriteLine("[LoadData] Starting data load...");
                PcGridView.Rows.Clear();

                // Load all PCs (active + inactive) if checkbox is checked, otherwise only active
                bool showInactive = ShowInactiveCheckbox.Checked;
                Debug.WriteLine($"[LoadData] Show Inactive = {showInactive}");

                _allPcs = showInactive
                    ? await _pcService.GetAllPcsAsync()
                    : await _pcService.GetAllActivePcsAsync();

                Debug.WriteLine($"[LoadData] Retrieved {_allPcs.Count} PCs from database");

                if (_allPcs.Count == 0)
                {
                    Debug.WriteLine("[LoadData] WARNING: No PCs returned from database!");
                }

                foreach (var pc in _allPcs)
                {
                    Debug.WriteLine($"[LoadData] Processing PC: {pc.ClientGuid} | Active: {pc.IsActive} | Display: {pc.DisplayName}");

                    // Display name fallback: if display_name is NULL, show current_pc_name
                    var displayName = string.IsNullOrEmpty(pc.DisplayName) ? pc.CurrentPcName : pc.DisplayName;

                    // Format last seen as relative time (e.g., "2 mins ago")
                    var timeAgo = FormatTimeAgo(pc.LastSeen);

                    // Shorten client GUID: show first 8 + last 4 chars
                    var shortGuid = ShortenGuid(pc.ClientGuid);

                    // Status: show "Inactive" if not active
                    var status = pc.IsActive ? "" : "Inactive";

                    var rowIndex = PcGridView.Rows.Add(
                        displayName,
                        pc.CurrentPcName,
                        pc.IpAddress ?? "N/A",
                        timeAgo,
                        pc.IsOnline ? "Yes" : "No",
                        shortGuid,
                        status
                    );

                    Debug.WriteLine($"[LoadData] Added row {rowIndex} to grid");

                    // Store full ClientGuid in Tag for row lookup on click
                    PcGridView.Rows[rowIndex].Tag = pc.ClientGuid;

                    // Style inactive PCs: gray text
                    if (!pc.IsActive)
                    {
                        foreach (DataGridViewCell cell in PcGridView.Rows[rowIndex].Cells)
                        {
                            cell.Style.ForeColor = Color.Gray;
                            cell.Style.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                        }
                    }
                }

                Debug.WriteLine($"[LoadData] Completed. Grid now has {PcGridView.Rows.Count} rows");

                // Force column width recalculation - this is necessary for the columns to become visible
                PcGridView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                Debug.WriteLine("[LoadData] Columns auto-resized");
            }
            catch (Exception ex)
            {
                // Expose error visibly in red label at top and in Debug output
                var errorMsg = $"ERROR loading PCs: {ex.Message}\n{ex.StackTrace}";
                ErrorLabel.Text = $"ERROR: {ex.Message}";
                ErrorLabel.Height = 60;
                ErrorLabel.Visible = true;

                Debug.WriteLine("========== PcLogbookListView LoadData Exception ==========");
                Debug.WriteLine(errorMsg);
                Debug.WriteLine("=========================================================");
            }
        }

        private string FormatTimeAgo(DateTime lastSeen)
        {
            var diff = DateTime.Now - lastSeen;

            if (diff.TotalSeconds < 60)
                return $"{(int)diff.TotalSeconds}s ago";
            if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes}m ago";
            if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours}h ago";

            return $"{(int)diff.TotalDays}d ago";
        }

        private string ShortenGuid(string guid)
        {
            if (guid.Length >= 12)
                return $"{guid.Substring(0, 8)}...{guid.Substring(guid.Length - 4)}";
            return guid;
        }

        private void PcGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = PcGridView.Rows[e.RowIndex];
                var clientGuid = selectedRow.Tag as string;

                if (!string.IsNullOrEmpty(clientGuid))
                {
                    // Single-click: navigate to detail view
                    ShowDetailView(clientGuid);
                }
            }
        }

        private void PcGridView_DoubleClick(object? sender, EventArgs e)
        {
            // Double-click also navigates (same behavior)
            if (PcGridView.SelectedRows.Count > 0)
            {
                var selectedRow = PcGridView.SelectedRows[0];
                var clientGuid = selectedRow.Tag as string;

                if (!string.IsNullOrEmpty(clientGuid))
                {
                    ShowDetailView(clientGuid);
                }
            }
        }

        private void ShowDetailView(string clientGuid)
        {
            // Find parent shell form and navigate
            var parent = Parent;
            while (parent != null && parent is not Form)
            {
                parent = parent.Parent;
            }

            if (parent is MainShellForm mainShell)
            {
                mainShell.ShowPcLogbookDetail(clientGuid);
            }
        }
    }
}
