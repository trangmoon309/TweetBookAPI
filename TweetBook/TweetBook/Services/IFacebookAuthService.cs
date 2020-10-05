using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.External;

namespace TweetBook.Services
{
    public interface IFacebookAuthService
    {
        //Kiểm tra access token(hay là thoog tin người dùng trong access token) có valid với ứng dụng của chugns ta hay không?
        //https://graph.facebook.com/debug_token?input_token=&access_token=
        Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken);

        //https://graph.facebook.com/me?fields=first_name,last_name,picture,email&access_token=
        Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken);

    }
}
