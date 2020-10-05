using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Data;
using TweetBook.HealthChecks;

namespace TweetBook.Installers
{
    public class HealthChecksInstaller : IInstaller
    {
        public void InstallService(IConfiguration configuration, IServiceCollection services)
        {
            services.AddHealthChecks()
                //Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
                .AddDbContextCheck<DataContext>()
                .AddCheck<RedisHealthCheck>("Redis");
            //thêm middleware ở statup nưa
        }
    }
}
