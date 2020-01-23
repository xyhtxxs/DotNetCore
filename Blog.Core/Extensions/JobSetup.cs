using Blog.Core.Tasks;
using Blog.Core.Tasks.QuartzNet;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using System;

namespace Blog.Core.Extensions
{
    /// <summary>
    /// Cors 启动服务
    /// </summary>
    public static class JobSetup
    {
        public static void AddJobSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddHostedService<Job1TimedService>();
            services.AddHostedService<Job2TimedService>();

            //services.AddSingleton<IJobFactory, JobFactory>();
            //services.AddTransient<Job1Quartz>();//Job使用瞬时依赖注入

        }
    }
}
