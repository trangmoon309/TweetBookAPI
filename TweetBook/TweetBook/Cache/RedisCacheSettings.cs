using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TweetBook.Cache
{
    public class RedisCacheSettings
    {
        public bool Enable { get; set; }
        public string ConnectionString { get; set; }
    }
}
