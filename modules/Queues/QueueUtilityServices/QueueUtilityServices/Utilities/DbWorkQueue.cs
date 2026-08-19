using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;

namespace QueueUtilityServices.Utilities
{
    /// <summary>
    /// 執行緒安全的隊列
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbWorkQueue<TContext> where TContext : DbContext
    {
        private readonly Channel<Func<TContext , Task>> _queue = Channel.CreateUnbounded<Func<TContext , Task>>();
        public async ValueTask EnqueueAsync(Func<TContext , Task> workItem) => await _queue.Writer.WriteAsync(workItem);
        public async ValueTask<Func<TContext , Task>> DequeueAsync(CancellationToken ct) => await _queue.Reader.ReadAsync(ct);
    }
}
