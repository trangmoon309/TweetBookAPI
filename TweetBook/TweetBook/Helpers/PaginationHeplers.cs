using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1.Request.Queries;
using Tweetbook.Contracts.V1.Responses;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;
using TweetBook.Services;

namespace TweetBook.Helpers
{
    public class PaginationHeplers
    {
        public static PagedResponse<T> CreatePaginationResponse<T>(IUriService uriService, PaginationFilter pagination, List<T> response)
        {
            //lấy uri để query lấy post ở next page
            var nextPage = pagination.PageNumber >= 1 ? uriService
                .GetAllPostsUri(new PaginationQuery(pagination.PageNumber + 1, pagination.PageSize)).ToString() : null;

            //lấy uri để query lấy post ở previous page
            var previousPage = pagination.PageNumber - 1 >= 1 ? uriService
                .GetAllPostsUri(new PaginationQuery(pagination.PageNumber - 1, pagination.PageSize)).ToString() : null;

            return new PagedResponse<T>
            {
                Data = response,
                NextPage = response.Any() ? nextPage : null,
                PreviousPage = previousPage,
                PageNumber = pagination.PageNumber >= 1 ? pagination.PageNumber : (int?)null,
                PageSize = pagination.PageSize >= 1 ? pagination.PageSize : (int?)null
            };
        }
    }
}
