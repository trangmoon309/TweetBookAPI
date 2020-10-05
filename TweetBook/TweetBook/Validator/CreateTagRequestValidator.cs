using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Request;

namespace TweetBook.Validator
{
    //tạo validator cho CreateTagRequest
    //chạy tự động
    //fluent validation
    public class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
    {
        public CreateTagRequestValidator()
        {
            RuleFor(x => x.TagName)
                .NotEmpty()
                .WithMessage("Tag name is required");
            RuleFor(x => x.TagName)
                .Must(RequiredLength).WithMessage("Tag name is required to longer than 10 characters!");
        }

        private bool RequiredLength(string tagName)
        {
            return tagName.Length > 10;
        }


    }
}
