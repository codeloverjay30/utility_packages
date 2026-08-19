using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreUtilityServices.Utilities
{
    public interface IDatabaseService
    {
        Task EnsureDatabaseCreatedAsync(IServiceProvider serviceProvider);
    }
}
