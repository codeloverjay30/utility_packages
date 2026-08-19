using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QueueUtilityServices.Services;
using QueueUtilityServices.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QueueUtilityServices.Extensions
{
    public static class UniversalInfrastructureExtensions
    {
        public static IServiceCollection AddUniversalInfrastructure<TContext>(
            this IServiceCollection services , Assembly assembly) where TContext : DbContext
        {
            // 1. 註冊 Queue 與 背景處理器
            services.AddSingleton<DbWorkQueue<TContext>>();
            services.AddHostedService<DbQueueProcessor<TContext>>();

            // 2. 註冊 MediatR
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
            // 註冊我們自定義的萬用 Handler
            services.AddTransient<IRequestHandler<DbQueueCommand<TContext>> , DbQueueHandler<TContext>>();

            // 3. 自動掃描所有實作 IDbService<TContext> 的類別並註冊
            var types = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract &&
                       t.GetInterfaces().Any(i => i.IsGenericType &&
                       i.GetGenericTypeDefinition() == typeof(IDbService<>) &&
                       i.GenericTypeArguments [ 0 ] == typeof(TContext)));

            foreach(var type in types)
            {
                services.AddScoped(type);
            }
            return services;
        }
    }
}
