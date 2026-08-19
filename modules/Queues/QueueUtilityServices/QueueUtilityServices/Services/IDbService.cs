using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace QueueUtilityServices.Services
{
    /// <summary>
    /// 服務標記介面
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IDbService<TContext> where TContext : DbContext
    {
    }
}
