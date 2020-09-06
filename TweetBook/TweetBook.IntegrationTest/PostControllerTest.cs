using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Request;
using TweetBook.Domain;
using Xunit;

namespace TweetBook.IntegrationTest
{
    public class PostControllerTest : IntegrationTest
    {
        [Fact]
        public async Task GetAll_WithoutAnyPosts_ReturnsEmptyResponse()
        {
            //Arrange: do some set up
             await AuthenticateAsync(); //client will be automatically authentication

            //Act: let's test client
            var response = await httpClient.GetAsync(ApiRoutes.Posts.GetAll);

            //Assert:  cài package FluentAssert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsAsync<List<Post>>()).Should().BeEmpty();
        }

        [Fact]
        public async Task Get_ReturnsPost_WhenPostExistsInTheDatabase()
        {
            //Arrange
            await AuthenticateAsync();
            var createdPost = await CreatePostAsync(new CreatePostRequest { Name = "Test create post." });
            //Act
            var respose = await httpClient.GetAsync(ApiRoutes.Posts.GetOne.Replace("{id}", createdPost.Id.ToString()));

            //Assert
            respose.StatusCode.Should().Be(HttpStatusCode.OK);
            var returndPost = (await respose.Content.ReadAsAsync<Post>());
             returndPost.Id.Should().Be(createdPost.Id);
            returndPost.Name.Should().Be("Test create post.");

        }
    }
}
