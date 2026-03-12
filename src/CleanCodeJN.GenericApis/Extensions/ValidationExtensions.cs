using CleanCodeJN.GenericApis.Abstractions.Responses;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Extensions;

/// <summary>
/// Validation Extensions
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Validate the DTO
    /// </summary>
    /// <typeparam name="TEntity">The Entity.</typeparam>
    /// <typeparam name="TDto">The DTO.</typeparam>
    /// <param name="validators">List of validators.</param>
    /// <param name="dto">The specific DT object to validate.</param>
    /// <param name="skipValidation">Should validation be skipped.</param>
    /// <returns>Validation Results in BaseResponse.</returns>
    public static async Task<BaseResponse<TEntity>> Validate<TEntity, TDto>(IEnumerable<IValidator<TDto>> validators, TDto dto, bool skipValidation)
        where TEntity : class
    {
        List<string> errorMessages = [];

        if (!skipValidation)
        {
            foreach (var validator in validators ?? [])
            {
                var result = validator.Validate(dto);
                if (!result.IsValid)
                {
                    errorMessages.Add(result.ToString(" "));
                }
            }
        }

        return errorMessages.Any()
            ? await BaseResponse<TEntity>.Create(false, message: string.Join(" ", errorMessages))
            : await BaseResponse<TEntity>.Create(true);
    }
}
