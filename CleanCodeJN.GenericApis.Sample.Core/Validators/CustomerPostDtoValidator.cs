using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Sample.Validators.Core;

public class CustomerPostDtoValidator : AbstractValidator<CustomerPostDto>
{
    public CustomerPostDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10);
    }
}
