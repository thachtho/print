using Newtonsoft.Json;
using System.Management;

namespace printer_2.Services
{
    public class TimedHostedService : IHostedService, IDisposable
    {
        private readonly IUsbService _usbService;

        public TimedHostedService(IUsbService usbService)
        {
            _usbService = usbService;
        }

        private Timer? _timer = null;

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer( _ =>
                _usbService.CheckUsbAdd(), null, TimeSpan.Zero,
                TimeSpan.FromSeconds(2)
            );

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {

            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
