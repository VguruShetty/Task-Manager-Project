using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TaskManager.Application.DTOs.Tags;

namespace TaskManager.Application.DTOs.Validators.Tags
{
    public class CreateTagDtoValidator : AbstractValidator<CreateTagDto>
    {
        public CreateTagDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tag name is required.")
                .MaximumLength(50).WithMessage("Tag name cannot exceed 50 characters.");

            RuleFor(x => x.ColorHex)
                .NotEmpty().WithMessage("Color hex is required.")
                .Matches(@"^#(?:[0-9a-fA-F]{3}){1,2}$").WithMessage("Invalid hex color code (e.g. #FF5733 or #FFF).");
        }
    }
}
