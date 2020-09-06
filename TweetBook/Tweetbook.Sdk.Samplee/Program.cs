using Refit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;

namespace Tweetbook.Sdk.Samplee
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cachedToken = string.Empty;
            //Console.WriteLine("Hello world!");
            var identityApi = RestService.For<IIdentityAPI>("https://localhost:44333");
            var tweetBookApi = RestService.For<ITweetbookAPI>("https://localhost:44333", new RefitSettings { 
                AuthorizationHeaderValueGetter = () => Task.FromResult(cachedToken)
            });

            var registerResponse = await identityApi.RegisterAsync(new UserRegistrationRequest
            {
                Email = "hpt2.dut",
                Password = "TrangBK309!"
            });

            var loginResponse = await identityApi.LoginAsync(new UserLoginRequest
            {
                Email = "hpt2.dut",
                Password = "TrangBK309!"
            });

             cachedToken = loginResponse.Content.Token;

            var allPosts = await tweetBookApi.GetAllASync();

            var createdPost = await tweetBookApi.CreateAsync(new CreatePostRequest
            {
                Name = "This is created by the SDK",
                Tags = new List<AddPostTagRequest>() { new AddPostTagRequest { Name = "#SDK"} }
            });

            var retrievedPost = await tweetBookApi.GetAsync(createdPost.Content.Id);

            var updatedPost = await tweetBookApi.UpdateAsync(createdPost.Content.Id, new UpdatePostRequest
            {
                Name = "This is updated Post by SDK."
            });

            var deletedPost = await tweetBookApi.DeleteAsync(createdPost.Content.Id);

        }
    }
}
