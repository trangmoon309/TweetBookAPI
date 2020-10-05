using Cosmonaut;
using Cosmonaut.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Services
{
    //Storing and managin entities with Azure Cosmos DB
    // Bước 1: kế thừa service interface
    // BƯớc 2: Cài package Cosmonaut và Cosmonaut DI
    // Bước 3: Tạo 1 cosmos DB, và lấy primarykey
    // Bước 4: Config cosmossettings rồi sau đó cấu hình cho start up
    // Bước 5: Tạo CosmostDto trong Domain

    public class CosmosPostService : IPostService
    {
        private readonly ICosmosStore<CosmosPostDto> cosmosStore;

        public CosmosPostService(ICosmosStore<CosmosPostDto> cosmosStore)
        {
            this.cosmosStore = cosmosStore;
        }

        public async Task<bool> AddPost(Post createdPost)
        {
            var cosmosPost = new CosmosPostDto()
            {
                //Id = createdPost.Id.ToString(),
                Id = Guid.NewGuid().ToString(),
                Name = createdPost.Name
            };
            var response = await cosmosStore.AddAsync(cosmosPost);
            return response.IsSuccess;
        }

        public Task<bool> CreateTagAsync(Tag createdTag)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeletePost(Guid postId)
        {
            var response = await cosmosStore.RemoveByIdAsync(postId.ToString());
            return response.IsSuccess;
        }

        public Task<Tag> DeleteTagAsync(string tagName)
        {
            throw new NotImplementedException();
        }

        public Task<List<Tag>> GetAllTagsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<Post> GetPostById(Guid postId)
        {
            var post = await cosmosStore.Query().FirstOrDefaultAsync(p => p.Id == postId.ToString());
            if (post == null) return null;
            return new Post { Id = Guid.Parse(post.Id), Name = post.Name };
        }

        public async Task<List<Post>> GetPosts()
        {
            var posts = await cosmosStore.Query().ToListAsync();
            return posts.Select(x => new Post { Id = Guid.Parse(x.Id), Name = x.Name }).ToList();
        }

        public Task<List<Post>> GetPosts(PaginationFilter paginationFilter)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> UpdatePost(Post updatedPost)
        {
            var post = await cosmosStore.Query().FirstOrDefaultAsync(p => p.Id == updatedPost.Id.ToString());
            post.Name = updatedPost.Name;
            var response = await cosmosStore.UpdateAsync(post);
            return response.IsSuccess;
        }

        public Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
