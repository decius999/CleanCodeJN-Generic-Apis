using CleanCodeJN.GenericApis.Abstractions.Responses;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Extensions;

public static class ValidationExtensions
{
    public static async Task<BaseResponse<TEntity>> Validate<TEntity, TDto>(IEnumerable<IValidator<TDto>> validators, TDto dto)
        where TEntity : class
    {
        var validator = validators.FirstOrDefault();
        if (validator is not null)
        {
            var result = validator.Validate(dto);
            if (!result.IsValid)
            {
                return await BaseResponse<TEntity>.Create(false, message: result.ToString(" "));
            }
        }

        return await BaseResponse<TEntity>.Create(true);
    }
}
