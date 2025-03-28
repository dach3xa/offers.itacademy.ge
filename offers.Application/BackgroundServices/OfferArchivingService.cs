using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using offers.Application.Services.Offers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace offers.Application.BackgroundServices
{
    public class OfferArchivingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5); 

        public OfferArchivingService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateAsyncScope())
                {
                    var service = scope.ServiceProvider.GetRequiredService<IOfferService>();

                    await service.ArchiveOffersAsync(stoppingToken);
                }
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
