using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Data;
using TweetBook.Services;

namespace TweetBook.Installers
{
    public class DataInstaller : IInstaller
    {
        public void InstallService(IConfiguration configuration, IServiceCollection services)
        {
            services.AddDbContext<DataContext>(options =>options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    //quản lý truy cập với roles
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<DataContext>();


            services.AddScoped<IPostService, PostService>();
            //services.AddSingleton<IPostService, CosmosPostService>();
        }
    }
}
