using CleanCodeJN.GenericApis.Sample.Dtos;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Sample.Validators;

public class CustomerPutDtoValidator : AbstractValidator<CustomerPutDto>
{
    public CustomerPutDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(10);
    }
}
