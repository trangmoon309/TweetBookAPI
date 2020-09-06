using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Domain
{
    public class Tag
    {
        public string TagId { get; set; }
        public string Name { get; set; }
        public string CreatorID { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }

        public Guid PostId { get; set; }

        //[ForeignKey(nameof(PostId))]
        //public Post Post { get; set; }
    }
}
