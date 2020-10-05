using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
using TweetBook.Authorization;
using TweetBook.Cache;
using TweetBook.Filters;
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
            //c2: configuration.GetSection("jwtSettings").Bind(jwtSettings);
            //Hai cách này dùng để bind 1 section ở appsetting cho 1 ĐỐI TƯỢNG (object)
            //Còn muốn bind 1 sectin cho 1 CLASS, thì ta dùng Configure
            services.AddSingleton(jwtSettings);

            //thiết lập các phiên bản tương thích
            services.AddMvc()
                //FluentValidation
                .AddFluentValidation(mvcConfig => mvcConfig.RegisterValidatorsFromAssemblyContaining<Startup>())
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            //TokenValidationParameters property phải được cài đặt cho JWTBearerOptions thì nó cho phép người gọi chỉ định các cài đặt nâng
            // cao hơn về cách mà mã JWT token được xác thực.
            //ValidateIssuerSignsKey (Key của issuer phải match với một giá trị mong đợi)
            //ValdiateIssuer(cho biết rằng token's signature cần được xác thực)
            var tokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,    
                ValidateIssuer = false, //k cho phep viec nhung nguoi khac nhau duoc cap chung 1 token
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)), //xac thuc signature
                ValidateAudience = false, //1 token không thể đc dùng cho nhiều site
                RequireExpirationTime = false, // không yêu cầu 1 token phải có exp time
                ValidateLifetime = true //xác thực lifetime
            };

            services.AddSingleton(tokenValidationParameters);


            //AUTHENTICATION
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; //Bearer
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                //add JwtBearerAuthentication
                //JwtBearer là một middleware tìm kiếm các tokens trong HTTP Authorization header của các request đc gửi tới
                // Nếu là valid token, request sẽ được authorized.  => jwtbeaer giống như 1 công cụ để xác thực = token
                .AddJwtBearer( x => {
                    x.SaveToken = true;
                    x.TokenValidationParameters = tokenValidationParameters; //xác thực token đc lấy từ HTTP Request Header.
                });


            //AUTHORIZATION
            services.AddAuthorization(options => {
                options.AddPolicy("TagViewer", builder => builder.RequireClaim("tags.view", "true"));
                options.AddPolicy("CustomPolicy", policy => policy.AddRequirements(new WorksForCompanyRequirement(".dut")));
            });

            services.AddSingleton<IAuthorizationHandler, WorksForCompanyHandler>();

            
            services.AddMvc(opt => {
                opt.EnableEndpointRouting = false;
                opt.Filters.Add<ValidationFilter>();
                //opt.Filters.Add<ApiKeyAuthAttribute>();
                //opt.Filters.Add<CachedAttribute>();
            });

            services.AddScoped<IIdentityService, IdentityService>();

            //Pagination
            services.AddSingleton<IUriService>(provider =>
            {
                //add DI IUriService-UriService, lấy baseUri để đưa cho hàm tạo của UriService
                //Khi ta gửi request đến server, request đó sẽ chứa các thuộc tính như: path/scheme/..
                //Ví dụ gửi request xem tất cả các post
                //scheme: https
                //path: api/v1/posts
                var accessor = provider.GetRequiredService<IHttpContextAccessor>();
                var request = accessor.HttpContext.Request; 
                var absoluteUri = string.Concat(request.Scheme, "://", request.Host.ToUriComponent(), "/");
                return new UriService(absoluteUri);
            });
        }
    }
}

