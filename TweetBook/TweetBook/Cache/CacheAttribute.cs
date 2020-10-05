using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Services;

namespace TweetBook.Cache
{
    //IAsyncActionFilte, nội dung dc ghi lại ở trong folder Filter
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CachedAttribute : Attribute, IAsyncActionFilter
    {
        private readonly int _timeToLiveSeconds;
        //private readonly IResponseCacheService _cacheService;


        public CachedAttribute(int timeToLiveSeconds)
        {
            _timeToLiveSeconds = timeToLiveSeconds;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before:(before go to the controller) request tới middleware, và stuff mà bạn có thể làm trước khi nó bước sang middle tiếp theo
            //check if request is cached
            var cacheSettings = context.HttpContext.RequestServices.GetRequiredService<RedisCacheSettings>();

            //nếu cache không được kích hoạt thì vào action và k làm gì cả
            if(!cacheSettings.Enable)
            {
                await next();
                return;
            }


            var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>();

            //tạo cacheKey từ request
            var cacheKey = GenerateCacheKeyFromRequest(context.HttpContext.Request);

            //lấy cacheResponse(cacheValue từ cacheKey)
            var cachedReponse = await cacheService.GetCachedReponseAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedReponse))
            {
                var contentResult = new Microsoft.AspNetCore.Mvc.ContentResult
                {
                    Content = cachedReponse,
                    ContentType = "application/json",
                    StatusCode = 200
                };
                context.Result = contentResult;
                return;
            }

            //nếu key này chưa đc cache thì mình sẽ thực hiện action trước sau đó lấy response để tạo ra 1 cache
            var executedContext = await next();
            if(executedContext.Result is OkObjectResult okObjectResult)
            {
                await cacheService.CacheResponseAsync(cacheKey, okObjectResult.Value, TimeSpan.FromSeconds(_timeToLiveSeconds));
            };
            
            //after:(after go to the controller) response tới 
            //get the calue
            //cache the respose
        }

        private string GenerateCacheKeyFromRequest(HttpRequest request)
        {
            var keyBuilder = new StringBuilder();
            keyBuilder.Append($"{request.Path}");
            foreach(var (key,value) in request.Query.OrderBy(x => x.Key))
            {
                keyBuilder.Append($"{key}-{value}");
            }
            return keyBuilder.ToString();
        }
    }
}
