using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Options;
using TweetBook.Services;

namespace TweetBook.Installers
{
    public class MvcInstaller : IInstaller
    {
        public void InstallService(IConfiguration configuration, IServiceCollection services)
        {
            //Authentication with JWT
            var jwtSettings = new JwtSettings();
            configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);

            //thiết lập các phiên bản tương thích
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //TokenValidationParameters property phải được cài đặt cho JWTBearerOptions thì nó cho phép người gọi chỉ định các cài đặt nâng
            // cao hơn về cách mà mã JWT token được xác thực.
            //ValidateIssuerSignsKey (Key của issuer phải match với một giá trị mong đợi)
            //ValdiateIssuer(cho biết rằng token's signature cần được xác thực)
            var tokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = false,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateAudience = false,
                //expiration: hết hạn = false => token k bị hết hạn
                RequireExpirationTime = false,
                ValidateLifetime = true
            };

            services.AddSingleton(tokenValidationParameters);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                //add JwtBearerAuthentication
                //JwtBearer là một middleware tìm kiếm các tokens trong HTTP Authorization header của các request đc gửi tới
                // Nếu là valid token, request sẽ được authorized.  => jwtbeaer giống như 1 công cụ để xác thực = token
                .AddJwtBearer( x => {
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenValidationParameters;
                });

            
            services.AddMvc(opt => opt.EnableEndpointRouting = false);

            //Add Swagger tool
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "TweetBook API", Version = "v1" });

                //Swagger also needs to know about jwt config authentication
                //Thêm Authorization vào swaggerUI
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {new OpenApiSecurityScheme{Reference = new OpenApiReference
                    {
                        Id = "Bearer",
                        Type = ReferenceType.SecurityScheme
                    }}, new List<string>()}
                });

            });

            services.AddScoped<IIdentityService, IdentityService>();
        }
    }
}

/*
    1. AddSecurityDefinition: Method này giúp chúng ta định nghĩa cách bảo vệ API bằng cách định nghĩa 1 hoặc nhiều security schema
    2. AddSecurityRequirement: This method lets you control the given authentication scheme applied either Global level or Operation level.
 
 */
