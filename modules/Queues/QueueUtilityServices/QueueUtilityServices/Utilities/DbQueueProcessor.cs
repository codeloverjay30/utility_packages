using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueUtilityServices.Utilities
{
    /// <summary>
    /// 萬用的背景消費者
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbQueueProcessor<TContext> : BackgroundService where TContext : DbContext
    {
        private readonly DbWorkQueue<TContext> _queue;
        private readonly IServiceProvider _serviceProvider;

        public DbQueueProcessor(DbWorkQueue<TContext> queue , IServiceProvider serviceProvider)
        {
            _queue = queue;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                var action = await _queue.DequeueAsync(stoppingToken);
                using var scope = _serviceProvider.CreateScope();
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<TContext>>();

                using var context = await factory.CreateDbContextAsync(stoppingToken);
                await action(context);
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}
