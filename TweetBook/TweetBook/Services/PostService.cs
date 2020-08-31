using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Data;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext dataContext;

        public PostService(DataContext dataContext)
        {
            this.dataContext = dataContext;
        }

        public async Task<List<Post>> GetPosts()
        {
            return await dataContext.Posts.ToListAsync();
        }
        public async Task<Post> GetPostById(Guid postId)
        {
            return await dataContext.Posts.FirstOrDefaultAsync(e => e.Id == postId);
        }

        public async Task<bool> UpdatePost(Post updatedPost)
        {
            var post = await GetPostById(updatedPost.Id);
            if (post == null) return false;
            post.Name = updatedPost.Name;
            var result = await dataContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeletePost(Guid postId)
        {
            var deletedPost = await GetPostById(postId);
            if (deletedPost == null) return false;
             dataContext.Remove(deletedPost);
            var result = await dataContext.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> AddPost(Post createdPost)
        {
           await dataContext.AddAsync(createdPost);
           var result = await dataContext.SaveChangesAsync();
           return result > 0;
        }

        public async Task<bool> UserOwnsPostAsync(Guid postId, string userId)
        {
            var post = await dataContext.Posts.SingleOrDefaultAsync(x => x.Id == postId);
            if (post == null) return false;
            if (post.UserId.Equals(userId)) return true;
            return false;
        }
    }
}
