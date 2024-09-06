using CleanCodeJN.GenericApis.Abstractions.Responses;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ValidationExtensions
{
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

        if (errorMessages.Any())
        {
            return await BaseResponse<TEntity>.Create(false, message: string.Join(" ", errorMessages));
        }

        return await BaseResponse<TEntity>.Create(true);
    }
}
