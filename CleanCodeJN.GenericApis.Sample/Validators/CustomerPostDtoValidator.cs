using CleanCodeJN.GenericApis.Sample.Dtos;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Sample.Validators;

public class CustomerPostDtoValidator : AbstractValidator<CustomerPostDto>
{
    public CustomerPostDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10);
    }
}
