using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Request;

namespace TweetBook.SwaggerExamples.Request
{
    //Việc tạo các example giúp chúng ta chèn vào phần description trong swagger mà k cần phải gõ 
    /// <summary>
    ///  <remarks></remarks>
    /// </summary>
    public class CreateTagRequestExample : IExamplesProvider<CreateTagRequest>
    {
        public CreateTagRequest GetExamples()
        {
            return new CreateTagRequest
            {
                TagName = "new tag"
            };
        }

    }
}
