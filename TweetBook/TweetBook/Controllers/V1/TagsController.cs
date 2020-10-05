using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;
using TweetBook.Extensions;
using TweetBook.Services;

namespace TweetBook.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Produces("application/json")]
    public class TagsController : Controller
    {
        private readonly IPostService _postService;

        public TagsController(IPostService postService)
        {
            _postService = postService;
        }

        /// <summary>
        /// Returns all the tags in systems
        /// </summary>
        /// <returns></returns>

        [HttpGet(ApiRoutes.Tags.GetAll)]
        //[Authorize(Policy = "TagViewer")]
        [Authorize(Policy = "CustomPolicy")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetAllTagsAsync());
        }

        [HttpDelete(ApiRoutes.Tags.Delete)]
        //[Authorize(Roles = "Admin")]
        [Authorize(Policy = "CustomPolicy")]
        public async Task<IActionResult> Delete([FromRoute] string tagName)
        {
            var deleted = await _postService.DeleteTagAsync(tagName);
            if(deleted != null)
            {
                return Ok("Xóa thành công!");
            }
            return NotFound();
        }


        /// <summary>
        /// Create a tag in the system
        /// </summary>
        /// <remarks>
        ///     Sample **request**:     
        ///         POST api/v1/tags
        ///         {
        ///             "name": "some name"
        ///         }
        /// </remarks>
        /// <response code="200">Creates a tag in the system</response>
        /// <response code="400">Unable to create a tag in the system due to validation error</response>
        [HttpPost(ApiRoutes.Tags.Create)]
        //exampleResponse
        [ProducesResponseType(typeof(TagResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        public async Task<IActionResult> Create([FromBody] CreateTagRequest createTagRequest)
        {
            //if (!ModelState.IsValid)  được thay thể bằng ValidationFilter
            //{
            //    return BadRequest(new { error = "Not validated" });
            //}
            //class này có validator là: ValadationFilter(Model.IsValid) và CreateTagRequestValidator
            var newTag = new Tag
            {
                TagId = Guid.NewGuid().ToString(),
                Name = createTagRequest.TagName,
                CreatedOn = DateTime.UtcNow,
                CreatorID = HttpContext.GetUserId(),
                PostId = Guid.Parse("6F9A8C75-293C-4E7B-A3D3-760A664F4877"),
            };

            var created = await _postService.CreateTagAsync(newTag);
            if (created) return Ok(newTag);
            return BadRequest(new ErrorResponse { Errors = new List<ErrorModel> { 
                new ErrorModel { Message = "Unable to create tag"} 
            } 
            });
        }
    }
}
