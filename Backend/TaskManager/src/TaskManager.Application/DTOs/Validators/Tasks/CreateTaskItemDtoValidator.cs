using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using TaskManager.Application.DTOs.Tasks;

namespace TaskManager.Application.DTOs.Validators.Tasks
{
    public class CreateTaskItemDtoValidator : AbstractValidator<CreateTaskItemDto>
    {
        public CreateTaskItemDtoValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("A valid ProjectId is required.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Invalid task priority value.");

            RuleFor(x => x.DueDateUtc)
                .GreaterThan(DateTime.UtcNow).WithMessage("Due date must be in the future.")
                .When(x => x.DueDateUtc.HasValue);
        }
    }
}
