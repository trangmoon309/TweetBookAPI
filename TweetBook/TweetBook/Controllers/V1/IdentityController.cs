using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetbook.Contracts.V1.Request;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Request;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{
    public class IdentityController : Controller
    {
        private readonly IIdentityService identityService;

        public IdentityController(IIdentityService identityService)
        {
            this.identityService = identityService;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody]UserRegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new AuthFailedResponse(){
                   Errors = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage))
                });
            }

            var authResponse = await identityService.RegisterAsync(request.Email, request.Password);
            if (!authResponse.Success)
            {
                return BadRequest(new AuthFailedResponse() { Errors = authResponse.Errors });
            }
            return Ok(new AuthSuccessResponse() 
            { 
                Token = authResponse.Token, 
                RefreshToken = authResponse.RefreshToken.JwtId 
            });
        }

        [HttpPost(ApiRoutes.Identity.FacebookAuth)]
        public async Task<IActionResult> FacebookAuth([FromBody] UserFacebookAuthRequest request)
        {
            var authResponse = await identityService.LoginWithFacebookAsync(request.AccessToken);
            if (!authResponse.Success) return BadRequest(new AuthFailedResponse() { Errors = authResponse.Errors });

            return Ok(new AuthSuccessResponse() 
            { 
                Token = authResponse.Token, 
                RefreshToken = authResponse.RefreshToken.JwtId 
            });

        }

        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] UserLoginRequest request)
        {
            var authResponse = await identityService.LoginAsync(request.Email, request.Password);
            if (!authResponse.Success) return BadRequest(new AuthFailedResponse() { Errors = authResponse.Errors });

            return Ok(new AuthSuccessResponse() { Token = authResponse.Token, RefreshToken = authResponse.RefreshToken.JwtId });

        }

        [HttpPost(ApiRoutes.Identity.Refresh)]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
        {
            var authResponse = await identityService.RefreshTokenAsync(request.Token, request.RefreshToken);
            if (!authResponse.Success) return BadRequest(new AuthFailedResponse() { Errors = authResponse.Errors });

            return Ok(new AuthSuccessResponse() { Token = authResponse.Token, RefreshToken = authResponse.RefreshToken.JwtId });
        }



    }
}
