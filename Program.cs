using Microsoft.Extensions.Hosting.Internal;
using printer_2.Services;
using printer_2.Socket;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        ConfigureServices(builder.Services);
        // Build the WebApplication instance
        var app = builder.Build();

        // Configure the HTTP request pipeline
        app.UseAuthorization();
        app.MapControllers();

        app.UseCors(options =>
        {
            options.WithOrigins([
                "http://localhost:5173",
                "http://127.0.0.1:5500",
                "http://localhost:81",
                "http://192.168.1.107"
            ])
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
        });
        app.MapHub<SocketHub>("/socket");
        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSingleton<IUsbService, UsbService>();
        services.AddSingleton<ISocketService, SocketService>();
        services.AddScoped<IPrinterService, PrinterService>();
        services.AddSingleton<IVantayService, VantayService>();

        services.AddHostedService<TimedHostedService>();
        services.AddHostedService<LifetimeService>();
        services.AddSignalR();
    }
}

