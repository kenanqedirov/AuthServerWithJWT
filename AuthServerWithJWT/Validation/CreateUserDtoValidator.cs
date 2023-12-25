using CoreLayer.Dtos;
using FluentValidation;

namespace AuthServerWithJWT.Validation
{
    public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
    {
        public CreateUserDtoValidator()
        {
            RuleFor(a => a.Email).NotEmpty().WithMessage("Email is required").EmailAddress().WithMessage("Email is wrong");
            RuleFor(a => a.Username).NotEmpty().WithMessage("Username is required");
            RuleFor(a => a.Password).NotEmpty().WithMessage("Password is required");
        }
    }
}
