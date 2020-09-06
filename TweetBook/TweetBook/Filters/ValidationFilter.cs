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
        //what will happen before we get into the controller
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            //before controller
            if (!context.ModelState.IsValid)
            {
                var errorsInModelState = context.ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(x => x.ErrorMessage)).ToArray();

                var errorResponse = new ErrorResponse();
                foreach(var item in errorsInModelState)
                {
                    //mỗi error có nhiều error-messeage
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
            await next(); //delegate call here

            //after controller
        }
    }
}
