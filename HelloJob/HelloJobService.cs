using Cronos;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HelloJob
{
    public class HelloJobService : BackgroundService
    {
        private readonly CronExpression _cron;
        private readonly TimeZoneInfo _timeZoneInfo;
        private readonly IServiceProvider _serviceProvider;

        public HelloJobService(IServiceProvider serviceProvider)
        {
            _cron = CronExpression.Parse("55 11 * * *"); 
            _timeZoneInfo = TimeZoneInfo.Local;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var next = _cron.GetNextOccurrence(DateTimeOffset.Now, _timeZoneInfo);
                if (next.HasValue)
                {
                    var delay = next.Value - DateTimeOffset.Now;
                    if (delay.TotalMilliseconds > 0)
                        await Task.Delay(delay, stoppingToken);

                    var mesaj = "Merhaba - " + DateTime.Now.ToString("HH:mm:ss");

                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<TextHub>>();
                        await hubContext.Clients.All.SendAsync("YeniMesaj", mesaj);
                    }

                    await Task.Delay(1000, stoppingToken);
                }
                else
                {
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }

}
