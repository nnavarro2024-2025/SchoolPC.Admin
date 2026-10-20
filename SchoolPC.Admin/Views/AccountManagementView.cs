using SchoolPC.Admin.Services;
using System.Windows.Forms;

namespace SchoolPC.Admin.Views
{
    public partial class AccountManagementView : UserControl
    {
        private readonly IStudentAccountService _studentService;
        private readonly IAdminAccountService _adminService;
        private TabControl _tabControl = null!;

        public AccountManagementView()
        {
            _studentService = null!;
            _adminService = null!;
            InitializeComponent();
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Padding = new Padding(15);

            // Title
            var titleLabel = new Label
            {
                Text = "Account Management",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 40,
                ForeColor = Color.FromArgb(0, 102, 204)
            };
            Controls.Add(titleLabel);

            // Tab Control
            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Padding = new Point(5, 5)
            };

            // TAB 1: Add Student
            var addStudentTab = new TabPage { Text = "Add Student Account" };
            addStudentTab.Controls.Add(CreateAddStudentPanel());
            _tabControl.TabPages.Add(addStudentTab);

            // TAB 2: Add Admin
            var addAdminTab = new TabPage { Text = "Add Admin Account" };
            addAdminTab.Controls.Add(CreateAddAdminPanel());
            _tabControl.TabPages.Add(addAdminTab);

            // TAB 3: Ban/Unban Students
            var banStudentTab = new TabPage { Text = "Ban/Unban Students" };
            banStudentTab.Controls.Add(CreateBanStudentPanel());
            _tabControl.TabPages.Add(banStudentTab);

            Controls.Add(_tabControl);
        }

        private Panel CreateAddStudentPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            var fields = new[]
            {
                ("First Name:", "FirstNameTb", 80),
                ("Middle Name:", "MiddleNameTb", 140),
                ("Last Name:", "LastNameTb", 200),
                ("Email (UIC):", "EmailTb", 260),
                ("Password:", "PasswordTb", 320)
            };

            foreach (var (label, name, yPos) in fields)
            {
                panel.Controls.Add(new Label { Text = label, Location = new Point(20, yPos), Width = 120, Height = 25, Font = new Font("Segoe UI", 9) });
                var tb = new TextBox
                {
                    Name = name,
                    Location = new Point(150, yPos),
                    Width = 300,
                    Height = 30,
                    PasswordChar = name == "PasswordTb" ? '*' : '\0'
                };
                panel.Controls.Add(tb);
            }

            // Notes field
            panel.Controls.Add(new Label { Text = "Notes:", Location = new Point(20, 380), Width = 120, Height = 25 });
            var notesTb = new TextBox
            {
                Name = "NotesTb",
                Location = new Point(150, 380),
                Width = 300,
                Height = 60,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            panel.Controls.Add(notesTb);

            // Add Button
            var addBtn = new Button
            {
                Text = "Add Student",
                Location = new Point(150, 460),
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addBtn.Click += AddStudentBtn_Click;
            panel.Controls.Add(addBtn);

            return panel;
        }

        private Panel CreateAddAdminPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            var fields = new[]
            {
                ("Username:", "AdminUsernameTb", 80),
                ("Password:", "AdminPasswordTb", 140),
                ("Full Name:", "AdminFullNameTb", 200),
                ("Role:", "AdminRoleTb", 260)
            };

            foreach (var (label, name, yPos) in fields)
            {
                panel.Controls.Add(new Label { Text = label, Location = new Point(20, yPos), Width = 120, Height = 25 });
                var tb = name == "AdminRoleTb" ? new ComboBox
                {
                    Name = name,
                    Location = new Point(150, yPos),
                    Width = 300,
                    Height = 30,
                    DropDownStyle = ComboBoxStyle.DropDownList
                } as Control :
                new TextBox
                {
                    Name = name,
                    Location = new Point(150, yPos),
                    Width = 300,
                    Height = 30,
                    PasswordChar = name == "AdminPasswordTb" ? '*' : '\0'
                };

                if (tb is ComboBox roleCombo)
                {
                    roleCombo.Items.AddRange(new[] { "admin", "super_admin", "monitor" });
                    roleCombo.SelectedIndex = 0;
                }

                panel.Controls.Add(tb);
            }

            // Add Button
            var addBtn = new Button
            {
                Text = "Add Admin",
                Location = new Point(150, 330),
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            addBtn.Click += AddAdminBtn_Click;
            panel.Controls.Add(addBtn);

            return panel;
        }

        private Panel CreateBanStudentPanel()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20), AutoScroll = true };

            // Banned students list
            var bannedLabel = new Label { Text = "Currently Banned Students:", Location = new Point(20, 20), Width = 300, Height = 25, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            panel.Controls.Add(bannedLabel);

            var bannedGrid = new DataGridView
            {
                Name = "BannedGrid",
                Location = new Point(20, 50),
                Width = 450,
                Height = 200,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true
            };
            bannedGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "StudentId", HeaderText = "Student ID", Width = 100 });
            bannedGrid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Reason", HeaderText = "Ban Reason", Width = 200 });
            bannedGrid.Columns.Add(new DataGridViewButtonColumn { Name = "Action", HeaderText = "Action", Text = "Unban", UseColumnTextForButtonValue = true });
            bannedGrid.CellClick += BannedGrid_CellClick;
            panel.Controls.Add(bannedGrid);

            // Ban new student section
            panel.Controls.Add(new Label { Text = "Ban New Student:", Location = new Point(20, 270), Width = 200, Height = 25, Font = new Font("Segoe UI", 10, FontStyle.Bold) });
            panel.Controls.Add(new Label { Text = "Student ID/Name:", Location = new Point(20, 310), Width = 120, Height = 25 });

            var searchTb = new TextBox
            {
                Name = "SearchTb",
                Location = new Point(150, 310),
                Width = 300,
                Height = 30
            };
            panel.Controls.Add(searchTb);

            panel.Controls.Add(new Label { Text = "Reason:", Location = new Point(20, 360), Width = 120, Height = 25 });
            var reasonTb = new TextBox
            {
                Name = "ReasonTb",
                Location = new Point(150, 360),
                Width = 300,
                Height = 60,
                Multiline = true
            };
            panel.Controls.Add(reasonTb);

            var banBtn = new Button
            {
                Text = "Ban Student",
                Location = new Point(150, 440),
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(200, 50, 50),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            banBtn.Click += BanStudentBtn_Click;
            panel.Controls.Add(banBtn);

            return panel;
        }

        private void AddStudentBtn_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Add Student feature will be implemented with full DI integration.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void AddAdminBtn_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Add Admin feature will be implemented with full DI integration.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BanStudentBtn_Click(object? sender, EventArgs e)
        {
            MessageBox.Show("Ban Student feature will be implemented with full DI integration.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BannedGrid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                MessageBox.Show("Unban feature will be implemented with full DI integration.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
