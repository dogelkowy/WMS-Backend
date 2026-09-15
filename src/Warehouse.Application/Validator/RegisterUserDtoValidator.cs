using Warehouse.Application.DTO;
using FluentValidation;

public class RegisterUserDtoValidator : AbstractValidator<RegisterUserDto>
{
    public RegisterUserDtoValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required.") 
            .MinimumLength(3)
            .WithMessage("Username must contain at least 3 characters.")
            .MaximumLength(50)
            .WithMessage("Username cannnot contain more than 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(5)
            .WithMessage("Password must contain at least 8 characters.");
    }
}
