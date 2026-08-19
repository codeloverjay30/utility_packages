using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreUtilityServices.Utilities
{
    public class SqliteDatabaseService<TContext> : IDatabaseService where TContext : DbContext
    {
        private readonly TContext _db;
        public SqliteDatabaseService(TContext db)
        {
            _db = db;
        }
        public async Task EnsureDatabaseCreatedAsync(
            IServiceProvider serviceProvider
        )
        {
            await this._db.Database.EnsureCreatedAsync();
        }
    }
}
