using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Responses;

namespace TweetBook.Filters
{
    //khi có hàm này thì t k cần Model.IsValid ở CreateTag nữa
    //cần phải đăng kí MvcInstaller
    public class ValidationFilter : IAsyncActionFilter
    {
        //Run code immediately before and after an action method is called.
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before: Do something before the action executes.
            //Vì theo sơ đồ hoạt động, cơ chế model binding được thực thi trước Action Filter
            //Nên ta mới có thể dùng modelstate như dưới được.
            if (!context.ModelState.IsValid)
            {
                var errorsInModelState = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage)).ToArray();

                var errorResponse = new ErrorResponse();
                foreach(var item in errorsInModelState)
                {
                    //mỗi error có nhiều error-messeage: value=>errors=>error-messeage
                    //item: error int errors
                    //subError: error-messages in errors
                    foreach(var subError in item.Value)
                    {
                        var errorModel = new ErrorModel
                        {
                            FieldName = item.Key,
                            Message = subError
                        };

                        errorResponse.Errors.Add(errorModel);
                    }
                }

                //save qua context chứ k return
                context.Result = new BadRequestObjectResult(errorResponse);
                return;
            }
            await next(); //calls the action method.

            //after controller: Do something after the action executes.
        }
    }
}
