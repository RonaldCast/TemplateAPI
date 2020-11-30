using APIWithIdentity.DTOs.MusicDTOs;
using FluentValidation;

namespace APIWithIdentity.Validators.MusicValidator
{
    public class SaveMusicValidator : AbstractValidator<SaveMusic>
    {
        public SaveMusicValidator()
        {
            RuleFor(m => m.Name)
                .NotEmpty()
                .MaximumLength(50);
            
            RuleFor(m => m.ArtistId)
                .NotEmpty()
                .WithMessage("'Artist Id' must not be 0.");
        }
    }
}