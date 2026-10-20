using SchoolPC.Admin.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Forms;

namespace SchoolPC.Admin.UI
{
    public partial class LoginForm : Form
    {
        private readonly IAdminAccountService _adminService;
        private readonly IServiceProvider _serviceProvider;

        public LoginForm(IAdminAccountService adminService, IServiceProvider serviceProvider)
        {
            _adminService = adminService;
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        private void LoginButton_Click(object? sender, EventArgs e)
        {
            var username = UsernameTextBox.Text.Trim();
            var password = PasswordTextBox.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                StatusLabel.Text = "Please enter username and password.";
                return;
            }

            // Disable button during login
            if (sender is Button btn)
                btn.Enabled = false;

            StatusLabel.Text = "Logging in...";

            LoginAsync(username, password);
        }

        private async void LoginAsync(string username, string password)
        {
            var loginButton = LoginButton;

            try
            {
                System.Diagnostics.Debug.WriteLine($"[LOGIN] Attempting login for user: {username}");

                var admin = await _adminService.AuthenticateAsync(username, password);

                if (admin == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[LOGIN] Authentication failed for user: {username}");
                    StatusLabel.Text = "Invalid username or password.";
                    if (loginButton != null)
                        loginButton.Enabled = true;
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"[LOGIN] Authentication successful for user: {username}");

                // Hide login form and show main shell
                StatusLabel.Text = $"Welcome, {admin.FullName}!";

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

                StatusLabel.Text = $"Error: {ex.Message}";

                if (loginButton != null)
                    loginButton.Enabled = true;
            }
        }
    }
}
