using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Services
{
    public class ResponseCacheService : IResponseCacheService
    {
        private readonly IDistributedCache distributedCache;

        public ResponseCacheService(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }
        
        //Lưu trữ(tạo) 1 response
        public async Task CacheResponseAsync(string cacheKey, object reponse, TimeSpan timeTimeLive)
        {
            if (reponse == null) return;
            var serializeResponse = JsonConvert.SerializeObject(reponse);
            await distributedCache.SetStringAsync(cacheKey, serializeResponse, new DistributedCacheEntryOptions
            {
                //time live của cached data
                AbsoluteExpirationRelativeToNow = timeTimeLive
            });
        }

        public async Task<string> GetCachedReponseAsync(string cacheKey)
        {
            var cacheResponse = await distributedCache.GetStringAsync(cacheKey);
            return string.IsNullOrEmpty(cacheResponse) ? null : cacheResponse;
        }
    }
}
//Nói cho đơn giản, caching là cách ta hi sinh memory/disk để giảm CPU time, hoặc network time nhằm tăng tốc độ hoặc giảm 
//tải hệ thống.

//Dùng distributed cache để lưu trữ dữ liệu khi ứng dụng được host trên cloud hoặc server farm. The cache được chia sẻ giữa các
// server xử lí yêu cầu.A client can submit a request that's handled by any server in the group if cached data for the client 
//is available. ASP.NET Core works with SQL Server, Redis, and NCache distributed caches.
// A distributed cache is  Thông tin trong cache không được lưu trong bộ nhớ của một web server riêng biệt và dữ liệu 
// được cache là có sẵn trong tất cả các server của ứng dụng.
/*
 - Khi dữ liệu được cached thì dữ liệu sẽ được: 
    + Nhất quán giữa các request đến muilti servers : Cached data được liên lạc trên tất cả các web servers. Người dùng 
        không thấy sự khác biệt kết quả cho dù web server nào xử lý request của họ.
    + Cached data vẫn tồn tại khi server restart và deployments. Một web server riêng biệt có thể gỡ bỏ hoặc thêm mới vào 
        mà không ảnh hưởng đến cache.
    + Không dùng bộ nhớ cục bộ - Doesn't use LOCAL MEMORY
    + Kho dữ liệu (database) có ít request được làm (so với multiple in-memory caches hoặc không cache).
 - Giống như bất kì loại cache nào, distributed cahe có thể cải thiện một cách đáng kể khả năng phản hồi của ứng dụng, 
từ đó data có thể được nhận từ cache sẽ nhanh hơn nhiều so với relational database (hoặc web service)
- Bất kể Redis hay SQL Server distributed caches được chọn, ứng dụng tương tác với cache sử dụng một common 
IDistributedCache interface.
 */
//https://viblo.asia/p/in-memory-caching-trong-aspnet-core-aWj53XmoK6m
/*  IDistributedCache Interface
 - Cho phép các items được thêm, nhận, gỡ bỏ khỏi distributed cache
 - Method:  
    + Get, GetAsync: tham số là 1 key kiểu string, nhận về 1 item đã được cache như một mảng byte[] nếu tìm thấy trong cache
    + Set, SetAsync: thêm 1 item trong cache dựa vào key của nó. 
    + Remove, RemoveAsync: gỡ 1 item cache dựa trên key của nó.

 - Redis là một database open-source in-memory, cái mà thường xuyên được sử dụng như một distributed cache
 - Bạn sẽ cấu hình Redis implementation trong phương thức ConfigureServices và truy cập nó trong ứng dụng của bạn bởi việc
yêu cầu một instance của IDistributedCache
 -  services.AddDistributedRedisCache(options =>
    {
        options.Configuration = "localhost";
        options.InstanceName = "SampleInstance";
    });

 - SqlServerCache implementtation cho phép distributed cache để sử dụng một database SQL Server như một nơi lưu trữ của nó.
 - Redis cache là một giải pháp caching cái mà mang lại cho bạn năng suất cao và độ trễ thấp so với SQL Cache.
 */
