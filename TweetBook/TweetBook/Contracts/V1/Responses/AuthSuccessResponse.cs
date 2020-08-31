using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Contracts.V1.Responses
{
    public class AuthSuccessResponse
    {
        public string Token { get; set; }
        public RefreshToken RefreshToken { get; set; }
    }
}
