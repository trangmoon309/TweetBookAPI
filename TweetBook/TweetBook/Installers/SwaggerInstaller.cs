using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TweetBook.Installers
{
    public class SwaggerInstaller : IInstaller
    {
        public void InstallService(IConfiguration configuration, IServiceCollection services)
        {
            //Add Swagger tool
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TweetBook API", Version = "v1" });

                //Swagger-Example: Swagger extension
                options.ExampleFilters();

                //Swagger also needs to know about jwt config authentication
                //Thêm Authorization vào swaggerUI
                // this method lets you enable the authentication schemes. (One can use multiple security schemes too if needed.
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                //This method lets you control the given authentication scheme applied either Global level or Operation level.
                options.AddSecurityRequirement(
                    new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme{
                                Name = "Name of Security Scheme",
                                Type = SecuritySchemeType.ApiKey,
                                In = ParameterLocation.Header,
                                Reference = new OpenApiReference
                                {   
                                    //như t học ở MVC, muốn thực hiện một công việc cần biết id thì route thường có dạng
                                    // controller/action?Id=...&token=
                                    // hoặc controller/action#Id
                                    Id = "Bearer",
                                    Type = ReferenceType.SecurityScheme
                                }}, 
                            new List<string>()
                        }
                    });

                // Extended Swagger documentation
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                options.IncludeXmlComments(xmlPath);

            });

            //Swagger_Examples
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

        }
    }
}
/*
    1. AddSecurityDefinition: Method này giúp chúng ta định nghĩa cách bảo vệ API bằng cách định nghĩa 1 hoặc nhiều security schema
    2. AddSecurityRequirement: This method lets you control the given authentication scheme applied either Global level or Operation level.
 
 */
