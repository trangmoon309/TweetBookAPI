using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using TweetBook.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Swashbuckle.AspNetCore.Swagger;
using TweetBook.Options;
using TweetBook.Installers;
using TweetBook.Services;
using AutoMapper;
using Tweetbook.Contracts.HealthChecks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace TweetBook
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.InstallServiceAssembly(Configuration);
            //typeof(Startup) tell the mapper to find the assembly that this type is in and automatically resolve
            //any mapping profile and register them in the eye without us have to any work
            services.AddAutoMapper(typeof(Startup));

            //bind 1 sectin cho 1 CLASS(KHÔNG implement interface), thì ta dùng Configure
            var ApiKeySetting = Configuration.GetSection("ApiKey");
            services.Configure<ApiKeyOptions>(ApiKeySetting);

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";

                    var response = new HealthCheckResponse
                    {
                        Status = report.Status.ToString(),
                        Checks = report.Entries.Select(x => new HealthCheckss
                        {
                            Component = x.Key,
                            Status = x.Value.Status.ToString(),
                            Description = x.Value.Description
                        }),
                        Duration = report.TotalDuration
                    };

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
                }
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseResponseCaching();

            app.UseAuthentication();

            var swaggerOptions = new SwaggerOption();
            Configuration.GetSection(nameof(SwaggerOptions)).Bind(swaggerOptions);
            //nếu k cần lấy ngay giá trị đc bind, có thể dùng
            app.UseSwagger(options =>
            {
                options.RouteTemplate = swaggerOptions.JsonRoute;
            });
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(swaggerOptions.UIEndPoint, swaggerOptions.Description);
            });


            app.UseMvc();

        }
    }
}
