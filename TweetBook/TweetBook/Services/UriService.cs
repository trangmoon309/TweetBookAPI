using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1.Request.Queries;
using TweetBook.Contracts.V1;

namespace TweetBook.Services
{
    //generate URI from our API for different purpose
    public class UriService : IUriService
    {
        private readonly string baseUri;

        public UriService(string baseUri)
        {
            this.baseUri = baseUri;
        }

        public Uri GetPostUri(string postId)
        {
            return new Uri(baseUri + ApiRoutes.Posts.GetOne.Replace("{id}", postId));
        }

        //trả về uri để query post theo điều kiện
        public Uri GetAllPostsUri(PaginationQuery pagination = null)
        {
            var uri = new Uri(baseUri);
            if(pagination == null)
            {
                return uri;
            }
            var modifiedUri = QueryHelpers.AddQueryString(baseUri, "pageNumber", pagination.PageNumber.ToString());
            modifiedUri = QueryHelpers.AddQueryString(modifiedUri, "pageSize", pagination.PageSize.ToString());
            //https://localhost:44333/?pageNumber=2&pageSize=3
            return new Uri(modifiedUri);
        }
    }
}
