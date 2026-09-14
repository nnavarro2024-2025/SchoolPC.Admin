using SchoolPC.Admin.Models;
using SchoolPC.Admin.Services;
using System.Windows.Forms;

namespace SchoolPC.Admin.Views
{
    public class PcLogbookListView : UserControl
    {
        private DataGridView _pcGridView = null!;
        private IPcInstanceService _pcService;

        public PcLogbookListView(IPcInstanceService pcService)
        {
            _pcService = pcService;
            InitializeComponent();
            ConfigureUI();
        }

        private void InitializeComponent()
        {
            // Will be called by constructor
        }

        private void ConfigureUI()
        {
            // Get service from app DI container (simplified - you can enhance with DI)
            // For now, using simpler initialization
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(15);

            // Title
            var titleLabel = new Label
            {
                Text = "PC Logbook - All PCs",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.FromArgb(0, 102, 204)
            };
            Controls.Add(titleLabel);

            // DataGridView for PCs
            _pcGridView = new DataGridView
            {
                Name = "PcGridView",
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToOrderColumns = true,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                ColumnHeadersHeight = 35,
                ColumnHeadersVisible = true
            };

            // Style column headers
            _pcGridView.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(5)
            };

            // Columns with headers
            _pcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "DisplayName", HeaderText = "PC Display Name", Width = 200 });
            _pcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "CurrentPcName", HeaderText = "Machine Name", Width = 150 });
            _pcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "IpAddress", HeaderText = "IP Address", Width = 120 });
            _pcGridView.Columns.Add(new DataGridViewTextBoxColumn { Name = "LastSeen", HeaderText = "Last Seen", Width = 150 });

            _pcGridView.CellDoubleClick += PcGridView_CellDoubleClick;
            Controls.Add(_pcGridView);

            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                _pcGridView.Rows.Clear();

                // Call real service to get all active PCs from database
                var pcs = await _pcService.GetAllActivePcsAsync();

                foreach (var pc in pcs)
                {
                    // Display name fallback: if display_name is NULL, show current_pc_name as safety net
                    var displayName = string.IsNullOrEmpty(pc.DisplayName) ? pc.CurrentPcName : pc.DisplayName;

                    _pcGridView.Rows.Add(
                        displayName,
                        pc.CurrentPcName,
                        pc.IpAddress ?? "N/A",
                        pc.LastSeen.ToString("g")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading PCs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PcGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var selectedRow = _pcGridView.Rows[e.RowIndex];
                var displayName = selectedRow.Cells["DisplayName"].Value?.ToString();
                MessageBox.Show($"Opening detail view for: {displayName}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // TODO: Navigate to detail view
            }
        }
    }
}
