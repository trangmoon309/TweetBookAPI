using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1.Request.Queries;

namespace TweetBook.Services
{
    //Cài đặt URi cho các trang trong việc phản hồi phân trang
    public interface IUriService
    {
        Uri GetPostUri(string postId);
        Uri GetAllPostsUri(PaginationQuery pagination = null);
    }
}
