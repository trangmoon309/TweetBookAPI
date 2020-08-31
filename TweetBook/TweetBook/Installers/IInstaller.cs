using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook
{
    public interface IInstaller
    {
        void InstallService(IConfiguration configuration, IServiceCollection services);
    }
}
