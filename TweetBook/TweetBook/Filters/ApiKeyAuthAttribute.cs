using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Binder;
using TweetBook.Options;
using Microsoft.Extensions.Options;

namespace TweetBook.Filters
{
    //Attribute = tên folder - Attribute
    //IAsyncActionFilter: kế thừa lớp Filter để trở thành 1 filter pipeline
    //**đăng kí DI ở AddMVC

    //Đc gọi đến ở SecretController, và để sử dụng các method trong secret thì phải điên APIKey 
    // = giá trị đc config trong appsetting thì mới được.

    //We can user attribute for a class or a method
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
    {
        private const string ApiKeyHeaderName = "ApiKey"; //tên key ở mục config

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before: là trước khi it goes on our controller as a middleware, the call
            // will come here, then it goes to the controller and when coming back it will go on after

            //Lấy value từ key tương ứng trong phần HEADER của Http Request, gán cho potentialApiKey
            //C2: potentialApiKey = context.HttpContext.Request.Query["ApiKey"];
            if(!context.HttpContext.Request.Headers.TryGetValue(ApiKeyHeaderName, out var potentialApiKey))
            {
                context.Result = new UnauthorizedResult();
                //khi return thì sẽ k được thực thi next() => k thể tới đc controller
                return;
            }

            //var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            //Lấy value của key "ApiKey" trong config
            //var apiKey = configuration.GetValue<string>("ApiKey");
            var apiKey = "MySecretKey";
            if (!apiKey.Equals(potentialApiKey)) //If value lấy từ config = value lấy ở http request header
            {
                context.Result = new UnauthorizedResult();
                return;
            }
            //Nếu 2 value = nhau tức t có thể sử dụng API 
            await next(); //this action will execute delegate

            //after
        }
    }
}


/*
 API Key let other services can integrate with your API
Ví dụ: Một github API có 1 apikey dành cho dev có thể lấy về, và sau đó dev
sẽ dùng các apikey đó để có thể đưa lên môi request, nhờ đó mà github biết được
và cho bạn sử dụng các api đó mà k cần phải nhập username/password nào cả.
 
- Và apikey có thể đưa vào ở nhiề nơi: header request, query, document ,...
với ví dụ này thì nick dùng header request

- Cách để lấy value từ appsetting theo công thức làm:
https://stackoverflow.com/questions/46940710/getting-value-from-appsettings-json-in-net-core
 */
