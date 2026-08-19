using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProgressBarUtilityServices
{
    public static class ProgressBarUtility
    {
        public static IServiceCollection AddProgressTracking(this IServiceCollection services)
        {
            services.AddSingleton<IProgressFactory , ProgressFactory>();
            return services;
        }
    }
}
