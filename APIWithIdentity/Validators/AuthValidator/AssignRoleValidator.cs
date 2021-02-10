using System;
using System.Data;
using APIWithIdentity.DTOs;
using FluentValidation;

namespace APIWithIdentity.Validators
{
    public class AssignRoleValidator : AbstractValidator<AssignRole>
    {
        public AssignRoleValidator()
        {
            RuleFor(x => x.UserId).Custom((id, context) =>
            {
                if (string.IsNullOrEmpty(id.Trim()))
                    context.AddFailure("the userId must not be empty");
                if (!Guid.TryParse(id, out _ ))
                    context.AddFailure("Type property is invalid");
            });

            RuleFor(x => x.Roles).Must(roles => roles.Length > 0)
                .WithMessage("Roles must not be empty");
        }
    }
}