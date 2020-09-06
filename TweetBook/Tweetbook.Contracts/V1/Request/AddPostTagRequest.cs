using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1.Request
{
    public class AddPostTagRequest
    {
        public string Name { get; set; }
        public string CreatedBy { get; set; }

    }
}
