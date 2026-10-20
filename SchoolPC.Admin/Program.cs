using Microsoft.Extensions.DependencyInjection;
using SchoolPC.Admin.Data;
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
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            // Setup Dependency Injection
            var services = new ServiceCollection();
            ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();

            // Get LoginForm from DI and run it
            var loginForm = serviceProvider.GetRequiredService<LoginForm>();
            Application.Run(loginForm);
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

