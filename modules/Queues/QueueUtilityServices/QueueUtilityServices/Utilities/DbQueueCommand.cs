using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueUtilityServices.Utilities
{
    /// <summary>
    /// 通用的背景任務指令 (MediatR)
    /// 使用者可以透過這個 Command 把任何動作丟進 Queue
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class DbQueueCommand<TContext> : IRequest where TContext : DbContext
    {
        public Func<TContext , Task> Action { get; set; } = default!;
    }
}
