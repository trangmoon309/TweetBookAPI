using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TweetBook.Contracts;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;
using TweetBook.Extensions;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{
    // AuthenticationScheme: Specifies protocols for authentication.
    //JwtBearerDefault: Default values used by bearer authentication.
    // Mục đích là không định nghĩa việc xác thực và mặc định chúng. 
    // Bằng cách: nói cho Authentication biết AuthenticateScheme và DefaultChallengeScheme  cần được sử dụng JwtBearer
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Poster,Admin")]
    public class PostsController : Controller
    {
        private IPostService service { get; set; }
        private readonly IMapper mapper;
        public PostsController(IPostService service, IMapper mapper)
        {
            this.service = service;
            this.mapper = mapper;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<ActionResult<IEnumerable<Post>>> GetAll()
        {
            var posts = await service.GetPosts();
            //List<PostResponse> response = posts.Select(e => new PostResponse
            //{
            //    Id = e.Id,
            //    UserId = e.UserId,
            //    Name = e.Name,
            //    Tags = e.Tags.Select(x => new TagResponse { Name = x.Name})
            //}).ToList();

            var response = mapper.Map<List<PostResponse>>(posts);
            return  Ok(response);
        }

        [HttpGet(ApiRoutes.Posts.GetOne)]
        public async Task<ActionResult<Post>> GetOne([FromRoute]Guid id)
        {
            Post post = await service.GetPostById(id);

            if (post == null) return NotFound();
            //else return Ok(new PostResponse 
            //{ 
            //    Id = post.Id, 
            //    Name = post.Name, 
            //    UserId = post.UserId, 
            //    Tags = post.Tags.Select(e => new TagResponse { Name = e.Name }).ToList() 
            //}); 

            else return Ok(mapper.Map<PostResponse>(post));
        }

        //FromBody nói API rằng có 1 post request đc gửi đến và API sẽ map body trong request với các object nào đó.
        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            List<Tag> tags = new List<Tag>();
            foreach(var item in postRequest.Tags)
            {
                tags.Add(new Tag
                {
                    TagId = Guid.NewGuid().ToString(),
                    Name = item.Name,
                    CreatorID = HttpContext.GetUserId(),
                    CreatedBy = item.CreatedBy,
                    CreatedOn = DateTime.UtcNow
                });
            }
            var post = new Post { Id = Guid.NewGuid(), Name = postRequest.Name, UserId = HttpContext.GetUserId(), Tags = tags};

            //if (Guid.Empty != postRequest.Id) post.Id = Guid.NewGuid();
            await service.AddPost(post);

            //Scheme: http
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUrl = baseUrl + "/" + ApiRoutes.Posts.GetOne.Replace("{id}", post.Id.ToString());

            var response = mapper.Map<PostResponse>(post);

            return Created(locationUrl, response);
            //return CreatedAtAction(nameof(GetOne), new { id = post.Id }, post);
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute]Guid id, [FromBody]UpdatePostRequest newPost)
        {
            string userId = HttpContext.GetUserId();
            var userOwnsPost = await service.UserOwnsPostAsync(id, userId);

            if (!userOwnsPost)
            {
                return BadRequest(error: new { error = "You don't own this post." });
            }

            Post updatedPost = new Post { Id = id, Name = newPost.Name, UserId = userId, 
                Tags = newPost.Tags.Select(x => new Tag { Name = x.Name, CreatedBy = x.CreatedBy }).ToList() };

            var result =  await service.UpdatePost(updatedPost);
            if (result) return Ok(newPost);
            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        [Authorize(Roles = "Admin")]
        [Authorize(Roles = "Poster")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {

            var userOwnsPost = await service.UserOwnsPostAsync(id, HttpContext.GetUserId());

            if (!userOwnsPost)
            {
                return BadRequest(error: new { error = "You don't own this post." });
            }

            if (id == null) return BadRequest();
            var result = await service.DeletePost(id);
            if (result) return Ok("Deleted!");
            return NotFound();
        }
    }
}
