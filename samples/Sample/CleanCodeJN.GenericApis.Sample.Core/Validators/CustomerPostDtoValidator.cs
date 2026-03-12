using CleanCodeJN.GenericApis.Sample.Core.Dtos;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Sample.Core.Validators;

/// <summary>
/// Validates a <see cref="CustomerPostDto"/> before a new customer is created.
/// </summary>
public class CustomerPostDtoValidator : AbstractValidator<CustomerPostDto>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CustomerPostDtoValidator"/> and configures the validation rules.
    /// </summary>
    public CustomerPostDtoValidator() => RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);
}
