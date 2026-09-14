using SchoolPC.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace SchoolPC.Admin.UI
{
    public partial class LoginForm : Form
    {
        private readonly IAdminAccountService _adminService;
        private readonly IServiceProvider _serviceProvider;
        private Panel _mainPanel = null!;
        private TextBox _usernameTextBox = null!;
        private TextBox _passwordTextBox = null!;
        private Label _statusLabel = null!;

        public LoginForm(IAdminAccountService adminService, IServiceProvider serviceProvider)
        {
            _adminService = adminService;
            _serviceProvider = serviceProvider;
            ConfigureUI();
        }

        private void ConfigureUI()
        {
            Text = "SchoolPC Admin - Login";
            Size = new Size(450, 400);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9);
            BackColor = Color.White;

            // Panel for form content
            _mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(20),
                AutoScroll = false,
                Name = "MainPanel"
            };

            // Title label
            var titleLabel = new Label
            {
                Text = "Admin Login",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, 20),
                ForeColor = Color.FromArgb(0, 102, 204)
            };
            _mainPanel.Controls.Add(titleLabel);

            // Credentials hint label
            var hintLabel = new Label
            {
                Text = "Demo: username=admin, password=password",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                AutoSize = true,
                Location = new Point(20, 50),
                ForeColor = Color.Gray
            };
            _mainPanel.Controls.Add(hintLabel);

            // Username label and textbox
            var usernameLabel = new Label
            {
                Text = "Username:",
                Location = new Point(20, 100),
                AutoSize = true
            };
            _mainPanel.Controls.Add(usernameLabel);

            _usernameTextBox = new TextBox
            {
                Name = "UsernameTextBox",
                Location = new Point(20, 125),
                Width = 380,
                Height = 32,
                Font = new Font("Segoe UI", 10),
                Text = "admin"
            };
            _mainPanel.Controls.Add(_usernameTextBox);

            // Password label and textbox
            var passwordLabel = new Label
            {
                Text = "Password:",
                Location = new Point(20, 165),
                AutoSize = true
            };
            _mainPanel.Controls.Add(passwordLabel);

            _passwordTextBox = new TextBox
            {
                Name = "PasswordTextBox",
                Location = new Point(20, 190),
                Width = 380,
                Height = 32,
                PasswordChar = '*',
                Font = new Font("Segoe UI", 10),
                Text = "password"
            };
            _mainPanel.Controls.Add(_passwordTextBox);

            // Status label
            _statusLabel = new Label
            {
                Name = "StatusLabel",
                Text = "",
                Location = new Point(20, 230),
                Width = 380,
                Height = 20,
                ForeColor = Color.Red,
                Font = new Font("Segoe UI", 9, FontStyle.Italic),
                AutoSize = false
            };
            _mainPanel.Controls.Add(_statusLabel);

            // Login button
            var loginButton = new Button
            {
                Text = "Login",
                Location = new Point(20, 260),
                Width = 380,
                Height = 40,
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            loginButton.Click += LoginButton_Click;
            _mainPanel.Controls.Add(loginButton);

            Controls.Add(_mainPanel);
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            var username = _usernameTextBox.Text.Trim();
            var password = _passwordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                _statusLabel.Text = "Please enter username and password.";
                return;
            }

            // Disable button during login
            if (sender is Button btn)
                btn.Enabled = false;

            _statusLabel.Text = "Logging in...";

            LoginAsync(username, password);
        }

        private async void LoginAsync(string username, string password)
        {
            var loginButton = FindLoginButton();

            try
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Attempting login for user: {username}");

                var admin = await _adminService.AuthenticateAsync(username, password);

                if (admin == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Authentication failed for user: {username}");
                    _statusLabel.Text = "Invalid username or password.";
                    if (loginButton != null)
                        loginButton.Enabled = true;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Authentication successful for user: {username}");

                // Hide login form and show main shell
                _statusLabel.Text = $"Welcome, {admin.FullName}!";

                // Close login form after brief delay to show message
                await Task.Delay(500);

                MainShellForm mainForm = new MainShellForm(admin, _serviceProvider);
                mainForm.Show();
                Hide();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Stack Trace: {ex.StackTrace}");

                _statusLabel.Text = $"Error: {ex.Message}";

                if (loginButton != null)
                    loginButton.Enabled = true;
            }
        }

        private Button? FindLoginButton()
        {
            foreach (Control ctrl in _mainPanel.Controls)
            {
                if (ctrl is Button btn && btn.Text == "Login")
                    return btn;
            }

            return null;
        }
    }
}
