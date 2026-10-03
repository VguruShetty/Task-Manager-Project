using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TaskManager.Application.DTOs.Projects;

namespace TaskManager.Application.DTOs.Validators.Projects
{
    public class CreateProjectDtoValidator : AbstractValidator<CreateProjectDto>
    {
        public CreateProjectDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MinimumLength(3).WithMessage("Project name must be at least 3 characters long.")
                .MaximumLength(100).WithMessage("Project name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When( x => !string.IsNullOrEmpty(x.Description));
        }
    }
}
