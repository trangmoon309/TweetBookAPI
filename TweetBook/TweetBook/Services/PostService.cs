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
        public async Task<List<Post>> GetPosts(PaginationFilter paginationFilter = null)
        {
            if(paginationFilter == null)
            {
                return await dataContext.Posts.Include(x => x.Tags).ToListAsync(); //Include như quan hệ 1-n; 1post có nhiều tags
            }
            var skip = (paginationFilter.PageNumber - 1) * paginationFilter.PageSize;// ví dụ muốn xem trang 2 => skip = 1*pagesize, nghĩa là skip trang 1
            return await dataContext.Posts.Include(x => x.Tags).Skip(skip).Take(paginationFilter.PageSize).ToListAsync();
        }

        public async Task<List<Post>> GetPosts()
        {
            var posts =  await dataContext.Posts.ToListAsync();
            foreach(var item in posts)
            {
                IEnumerable<Tag> tags = dataContext.Tags.Select(e => e).Where(e => e.PostId == item.Id);
                item.Tags = tags.ToList();
            }
            return posts;
        }
        public async Task<Post> GetPostById(Guid postId)
        {
            var post =  await dataContext.Posts.FirstOrDefaultAsync(e => e.Id == postId);
            IEnumerable<Tag> tags = dataContext.Tags.Select(e => e).Where(e => e.PostId == post.Id);
            post.Tags = tags.ToList();
            return post;
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

        public async Task<List<Tag>> GetAllTagsAsync()
        {
            return await dataContext.Tags.ToListAsync();
        }

        public async Task<Tag> DeleteTagAsync(string tagName)
        {
            var deleted = await dataContext.Tags.FirstOrDefaultAsync(e => e.Name == tagName);
            dataContext.Tags.Remove(deleted);
            var result = await dataContext.SaveChangesAsync();
            if (result>0) return deleted;
            return null;
        }

        public async Task<bool> CreateTagAsync(Tag createdTag)
        {
            await dataContext.Tags.AddAsync(createdTag);
            var result = await dataContext.SaveChangesAsync();

            return result > 0;
        }
    }
}
