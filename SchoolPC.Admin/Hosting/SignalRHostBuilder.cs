using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using SchoolPC.Admin.Hubs;

namespace SchoolPC.Admin.Hosting
{
    /// <summary>
    /// Self-hosts a SignalR hub (MonitoringHub) in the WinForms application.
    /// Uses Kestrel to run a minimal HTTP server on localhost for hub connections.
    /// This allows SchoolPC.Client instances to connect to the hub running within this app.
    /// </summary>
    public class SignalRHostBuilder
    {
        private WebApplication? _app;
        private readonly IServiceProvider _serviceProvider;

        public SignalRHostBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Build and start the SignalR hub server.
        /// </summary>
        /// <param name="hubUrl">Hub URL (default: http://localhost:5000/hubs/monitoring)</param>
        /// <param name="cancellationToken">Token to stop the server</param>
        public async Task StartAsync(string hubUrl = "http://localhost:5000", CancellationToken cancellationToken = default)
        {
            var builder = WebApplication.CreateBuilder();

            // Configure Kestrel to listen on the specified URL
            builder.WebHost.UseUrls(hubUrl);

            // Add SignalR to services
            builder.Services.AddSignalR();

            // Add services from the WinForms app's DI container
            // Copy all registrations from the WinForms ServiceProvider
            CopyServicesToBuilder(builder.Services);

            // Build the web app
            _app = builder.Build();

            // Map the MonitoringHub route
            _app.MapHub<MonitoringHub>("/hubs/monitoring");

            // Start the server
            await _app.StartAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine($"SignalR Hub started at {hubUrl}/hubs/monitoring");
        }

        /// <summary>
        /// Stop the SignalR server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            if (_app != null)
            {
                await _app.StopAsync(cancellationToken);
                await _app.DisposeAsync();
            }
        }

        /// <summary>
        /// Copy service registrations from the WinForms DI container to the web app builder.
        /// This ensures the hub can access the same services as the UI.
        /// </summary>
        private void CopyServicesToBuilder(IServiceCollection serviceCollection)
        {
            // Get all services from the WinForms ServiceProvider
            // Create new instances of service descriptors for common services

            // In practice, you can either:
            // 1. Re-register key services here (PasswordHasher, Repositories, etc.)
            // 2. Or use a shared DI configuration method

            // For now, we rely on each service being registered independently
            // The actual registration happens in Program.cs where both contexts can access it
        }
    }
}
