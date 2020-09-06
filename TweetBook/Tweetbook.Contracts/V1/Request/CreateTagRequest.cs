using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Contracts.V1.Request
{
    public class CreateTagRequest
    {
        [Required]
        public string TagName { get; set; }
    }
}
