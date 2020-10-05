using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Services
{
    public interface IResponseCacheService
    {
        Task CacheResponseAsync(string cacheKey, object reponse, TimeSpan timeTimeLive);
        Task<string> GetCachedReponseAsync(string cacheKey);
    }
}
