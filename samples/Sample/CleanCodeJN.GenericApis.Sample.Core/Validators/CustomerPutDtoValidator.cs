using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Sample.Core.Validators;

/// <summary>
/// Validates a <see cref="CustomerPutDto"/> before an existing customer is updated.
/// </summary>
public class CustomerPutDtoValidator : AbstractValidator<CustomerPutDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CustomerPutDtoValidator"/> and configures the validation rules.
    /// </summary>
    public CustomerPutDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0);

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
    }
}
