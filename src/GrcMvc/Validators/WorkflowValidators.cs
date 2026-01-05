using FluentValidation;
using GrcMvc.Models.DTOs;

namespace GrcMvc.Validators
{
    public class CreateWorkflowDtoValidator : AbstractValidator<CreateWorkflowDto>
    {
        public CreateWorkflowDtoValidator()
        {
            RuleFor(x => x.WorkflowNumber)
                .NotEmpty().WithMessage("Workflow number is required")
                .MaximumLength(50).WithMessage("Workflow number cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Workflow name is required")
                .MaximumLength(200).WithMessage("Workflow name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
                .Must(BeValidCategory).WithMessage("Category must be one of: Approval, Review, Assessment, Audit, Incident");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Workflow type is required")
                .MaximumLength(100).WithMessage("Workflow type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Type must be one of: Sequential, Parallel, Conditional, Hybrid");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Active, Inactive, Suspended, Completed");

            RuleFor(x => x.Priority)
                .Must(BeValidPriority).WithMessage("Priority must be one of: Critical, High, Medium, Low");

            RuleFor(x => x.AssignedTo)
                .NotEmpty().WithMessage("Assigned to is required")
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.InitiatedBy)
                .MaximumLength(100).WithMessage("Initiated by cannot exceed 100 characters");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue)
                .WithMessage("Due date cannot be in the past");

            RuleFor(x => x.Steps)
                .NotEmpty().WithMessage("Steps are required")
                .MaximumLength(5000).WithMessage("Steps cannot exceed 5000 characters");

            RuleFor(x => x.Conditions)
                .MaximumLength(2000).WithMessage("Conditions cannot exceed 2000 characters");

            RuleFor(x => x.Notifications)
                .MaximumLength(1000).WithMessage("Notifications cannot exceed 1000 characters");
        }

        private bool BeValidCategory(string category)
        {
            return category.Equals("Approval", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Review", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Assessment", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Audit", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Incident", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Sequential", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Parallel", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Conditional", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Hybrid", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Suspended", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidPriority(string priority)
        {
            return priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("High", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("Medium", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("Low", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class UpdateWorkflowDtoValidator : AbstractValidator<UpdateWorkflowDto>
    {
        public UpdateWorkflowDtoValidator()
        {
            RuleFor(x => x.WorkflowNumber)
                .NotEmpty().WithMessage("Workflow number is required")
                .MaximumLength(50).WithMessage("Workflow number cannot exceed 50 characters");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Workflow name is required")
                .MaximumLength(200).WithMessage("Workflow name cannot exceed 200 characters");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters");

            RuleFor(x => x.Category)
                .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
                .Must(BeValidCategory).WithMessage("Category must be one of: Approval, Review, Assessment, Audit, Incident");

            RuleFor(x => x.Type)
                .MaximumLength(100).WithMessage("Workflow type cannot exceed 100 characters")
                .Must(BeValidType).WithMessage("Type must be one of: Sequential, Parallel, Conditional, Hybrid");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Active, Inactive, Suspended, Completed");

            RuleFor(x => x.Priority)
                .Must(BeValidPriority).WithMessage("Priority must be one of: Critical, High, Medium, Low");

            RuleFor(x => x.AssignedTo)
                .MaximumLength(100).WithMessage("Assigned to cannot exceed 100 characters");

            RuleFor(x => x.InitiatedBy)
                .MaximumLength(100).WithMessage("Initiated by cannot exceed 100 characters");

            RuleFor(x => x.DueDate)
                .GreaterThanOrEqualTo(DateTime.Today).When(x => x.DueDate.HasValue)
                .WithMessage("Due date cannot be in the past");

            RuleFor(x => x.Steps)
                .MaximumLength(5000).WithMessage("Steps cannot exceed 5000 characters");

            RuleFor(x => x.Conditions)
                .MaximumLength(2000).WithMessage("Conditions cannot exceed 2000 characters");

            RuleFor(x => x.Notifications)
                .MaximumLength(1000).WithMessage("Notifications cannot exceed 1000 characters");
        }

        private bool BeValidCategory(string category)
        {
            return category.Equals("Approval", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Review", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Assessment", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Audit", StringComparison.OrdinalIgnoreCase) ||
                   category.Equals("Incident", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidType(string type)
        {
            return type.Equals("Sequential", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Parallel", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Conditional", StringComparison.OrdinalIgnoreCase) ||
                   type.Equals("Hybrid", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Active", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Suspended", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase);
        }

        private bool BeValidPriority(string priority)
        {
            return priority.Equals("Critical", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("High", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("Medium", StringComparison.OrdinalIgnoreCase) ||
                   priority.Equals("Low", StringComparison.OrdinalIgnoreCase);
        }
    }

    public class CreateWorkflowExecutionDtoValidator : AbstractValidator<CreateWorkflowExecutionDto>
    {
        public CreateWorkflowExecutionDtoValidator()
        {
            RuleFor(x => x.ExecutionNumber)
                .NotEmpty().WithMessage("Execution number is required")
                .MaximumLength(50).WithMessage("Execution number cannot exceed 50 characters");

            RuleFor(x => x.WorkflowName)
                .NotEmpty().WithMessage("Workflow name is required")
                .MaximumLength(200).WithMessage("Workflow name cannot exceed 200 characters");

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Status must be one of: Started, InProgress, Completed, Failed, Cancelled");

            RuleFor(x => x.InitiatedBy)
                .NotEmpty().WithMessage("Initiated by is required")
                .MaximumLength(100).WithMessage("Initiated by cannot exceed 100 characters");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("Start time is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Start time cannot be in the future");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime).When(x => x.EndTime.HasValue)
                .WithMessage("End time must be after start time");

            RuleFor(x => x.Duration)
                .GreaterThan(0).When(x => x.Duration.HasValue)
                .WithMessage("Duration must be greater than 0");
        }

        private bool BeValidStatus(string status)
        {
            return status.Equals("Started", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("InProgress", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Failed", StringComparison.OrdinalIgnoreCase) ||
                   status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);
        }
    }
}
