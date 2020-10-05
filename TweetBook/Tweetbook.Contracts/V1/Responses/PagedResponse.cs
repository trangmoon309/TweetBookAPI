using System;
using System.Collections.Generic;
using System.Text;

namespace Tweetbook.Contracts.V1.Responses
{
    public class PagedResponse<T>
    {
        //ctor không đối số để SDK có thể dùng nó 
        public PagedResponse() { }
        public PagedResponse(IEnumerable<T> data)
        {
            Data = data;
        }

        public IEnumerable<T> Data { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public string NextPage { get; set; }
        public string PreviousPage { get; set; }

    }
}
