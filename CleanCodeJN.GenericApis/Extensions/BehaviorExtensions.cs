using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using FluentValidation;

namespace CleanCodeJN.GenericApis.Extensions;

public static class BehaviorExtensions
{
    public static async Task<BaseResponse<TEntity>> Validate<TEntity, TDto>(IValidator<TDto> validator, TDto dto)
        where TEntity : class
        where TDto : class, IDto
    {
        if (validator is not null)
        {
            var result = validator.Validate(dto);
            if (!result.IsValid)
            {
                return await BaseResponse<TEntity>.Create(false, message: result.ToString());
            }
        }

        return await BaseResponse<TEntity>.Create(true);
    }
}
