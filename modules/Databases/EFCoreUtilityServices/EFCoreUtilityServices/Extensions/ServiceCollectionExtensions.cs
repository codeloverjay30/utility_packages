using EFCoreUtilityServices.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCoreUtilityServices.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMyExternalServices<TContext>(
            this IServiceCollection services
        )
        where TContext : DbContext
        {
            services.AddScoped<IDatabaseService , SqliteDatabaseService<TContext>>();
            return services;

        }
    }
}
