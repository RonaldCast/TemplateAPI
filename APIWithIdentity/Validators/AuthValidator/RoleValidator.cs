using System.Data;
using APIWithIdentity.DTOs;
using FluentValidation;

namespace APIWithIdentity.Validators
{
    public class RoleValidator : AbstractValidator<CreateRole>
    {
        public RoleValidator()
        {
            RuleFor(x => x.RoleName).NotEmpty()
                .WithMessage("RoleName must not be empty");
        }
    }
}