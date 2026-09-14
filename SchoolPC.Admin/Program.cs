using Microsoft.Extensions.DependencyInjection;
using SchoolPC.Admin.Data;
using SchoolPC.Admin.Hosting;
using SchoolPC.Admin.Services;
using SchoolPC.Admin.UI;

namespace SchoolPC.Admin
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static async Task Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Setup Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Start SignalR Hub in background
            var signalRHost = new SignalRHostBuilder(serviceProvider);
            var hubStopToken = new CancellationTokenSource();
            _ = signalRHost.StartAsync("http://localhost:5000", hubStopToken.Token);

            try
            {
                // Get LoginForm from DI and run it
                var loginForm = serviceProvider.GetRequiredService<LoginForm>();
                Application.Run(loginForm);
            }
            finally
            {
                // Stop SignalR Hub when app closes
                hubStopToken.Cancel();
                await signalRHost.StopAsync();
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            // Connection Factory (MySQL connection string)
            const string connectionString = "Server=localhost;Database=schoolpc_db;User=root;Password=;";
            services.AddSingleton(new DatabaseConnectionFactory(connectionString));

            // Repositories
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IProblemReportRepository, ProblemReportRepository>();
            services.AddScoped<IPcInstanceRepository, PcInstanceRepository>();
            services.AddScoped<IStudentAccountRepository, StudentAccountRepository>();
            services.AddScoped<IAdminAccountRepository, AdminAccountRepository>();

            // Services
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ISessionService, SessionService>();
            services.AddScoped<IProblemReportService, ProblemReportService>();
            services.AddScoped<IPcInstanceService, PcInstanceService>();
            services.AddScoped<IStudentAccountService, StudentAccountService>();
            services.AddScoped<IAdminAccountService, AdminAccountService>();

            // UI Forms - register with factory that includes IServiceProvider
            services.AddScoped<LoginForm>();
            services.AddScoped<MainShellForm>();
        }
    }
}
