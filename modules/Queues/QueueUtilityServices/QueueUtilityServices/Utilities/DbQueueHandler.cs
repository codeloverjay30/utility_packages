using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueUtilityServices.Utilities
{
    /// <summary>
    /// 這是一個「萬用 Handler」，它會攔截所有的 DbQueueCommand<T> 並轉發到 Queue
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbQueueHandler<TContext> : IRequestHandler<DbQueueCommand<TContext>> where TContext : DbContext
    {
        private readonly DbWorkQueue<TContext> _queue;
        public DbQueueHandler(DbWorkQueue<TContext> queue) => _queue = queue;

        public async Task Handle(DbQueueCommand<TContext> request , CancellationToken ct)
        {
            await _queue.EnqueueAsync(request.Action);
        }
    }
}
