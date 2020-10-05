using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Extensions
{
    public static class GeneralExtensions
    {
        //HTTPContext: Encapsulates all HTTP-specific information about an individual HTTP request.
        public static string GetUserId(this HttpContext httpContext)
        {
            if(httpContext.User == null)
            {
                return string.Empty;
            }

            //lấy userID từ token
            return httpContext.User.Claims.Single(x => x.Type == "id").Value;
        }
    }
}
