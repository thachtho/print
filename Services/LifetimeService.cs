namespace printer_2.Services
{

    public class LifetimeService : IHostedService
    {
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IVantayService _vantayService;

        public LifetimeService(IHostApplicationLifetime appLifetime, IVantayService vantayService)
        {
            _appLifetime = appLifetime;
            _vantayService = vantayService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // No-op
            Console.WriteLine("StartAsync=>>>>>>>>>>>>>>>");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StopAsync=>>>>>>>>>>>>>");
            _vantayService.removeDataSensor();

            return Task.CompletedTask;
        }
    }
}
