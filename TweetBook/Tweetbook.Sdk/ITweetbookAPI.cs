using Refit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;

namespace Tweetbook.Sdk
{
    [Headers("Authorization: Bearer")]
    public interface ITweetbookAPI
    {
        [Get("/api/v1/posts")]
        Task<ApiResponse<List<PostResponse>>> GetAllASync();

        [Get("/api/v1/posts/{id}")]
        Task<ApiResponse<PostResponse>> GetAsync(Guid id);

        [Post("/api/v1/posts")]
        Task<ApiResponse<PostResponse>> CreateAsync([Body] CreatePostRequest request);

        [Put("/api/v1/posts/{id}")]
        Task<ApiResponse<PostResponse>> UpdateAsync(Guid id, [Body] UpdatePostRequest request);

        [Delete("/api/v1/posts/{id}")]
        Task<ApiResponse<string>> DeleteAsync(Guid id);

    }
}
