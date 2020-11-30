using APIWithIdentity.DTOs;
using FluentValidation;

namespace APIWithIdentity.Validators.ArtistValidator
{
    public class SaveArtistValidator : AbstractValidator<SaveArtist>
    {
        public SaveArtistValidator()
        {
            RuleFor(a => a.Name)
                .NotEmpty().MaximumLength(50);
        }
    }
}