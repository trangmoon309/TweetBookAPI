using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPosts();
        Task<Post> GetPostById(Guid postId);
        Task<bool> UpdatePost(Post updatedPost);
        Task<bool> DeletePost(Guid postId);
        Task<bool> AddPost(Post createdPost);
        Task<bool> UserOwnsPostAsync(Guid postId, string userId);
        Task<List<Tag>> GetAllTagsAsync();
        Task<Tag> DeleteTagAsync(string tagName);
        Task<bool> CreateTagAsync(Tag createdTag);
    }
}
