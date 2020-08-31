using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1.Request
{
    public class UserRegistrationRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
}
