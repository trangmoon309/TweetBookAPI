using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1.Responses
{
    public class PostResponse
    {
        public Guid Id { get; set; }
        //public List<Tag> Tags { get; set; }

        //use automapping
        public string Name { get; set; }
        public string UserId { get; set; }
        public IEnumerable<TagResponse> Tags { get; set; }
    }
}
