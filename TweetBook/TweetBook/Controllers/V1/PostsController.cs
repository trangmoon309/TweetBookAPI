using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : Controller
    {
        private IPostService service { get; set; }
        public PostsController(IPostService service)
        {
            this.service = service;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<ActionResult<IEnumerable<Post>>> GetAll()
        {
            return  Ok(await service.GetPosts());
        }

        [HttpGet(ApiRoutes.Posts.GetOne)]
        public async Task<ActionResult<Post>> GetOne([FromRoute]Guid id)
        {
            Post post = await service.GetPostById(id);    

            if (post == null) return NotFound();
            else return Ok(post);
        }

        //FromBody nói API rằng có 1 post request đc gửi đến và API sẽ map body trong request với các object nào đó.
        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            var post = new Post { Id = Guid.NewGuid(), Name = postRequest.Name, UserId = HttpContext.GetUserId() };

            //if (Guid.Empty != postRequest.Id) post.Id = Guid.NewGuid();
            await service.AddPost(post);

            //Scheme: http
            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUrl = baseUrl + "/" + ApiRoutes.Posts.GetOne.Replace("{id}", post.Id.ToString());

            var response = new PostResponse { Id = post.Id };

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

            Post updatedPost = new Post { Id = id, Name = newPost.Name, UserId = userId };

            var result =  await service.UpdatePost(updatedPost);
            if (result) return Ok(newPost);
            return NotFound();
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
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
