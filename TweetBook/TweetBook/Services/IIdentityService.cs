using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterAsync(string Email, string Password);
        Task<AuthenticationResult> LoginAsync(string Email, string Password);
        Task<AuthenticationResult> RefreshTokenAsync(string Token, string RefreshToken);
        Task<AuthenticationResult> LoginWithFacebookAsync(string accessToken); //acessToken sẽ giúp ta call the API
    }
}
