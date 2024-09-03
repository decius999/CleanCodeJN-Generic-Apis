using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

public class PutValidationBehavior<TPutEntity, TPutDto>(IValidator<TPutDto> validator) : IPipelineBehavior<PutRequest<TPutEntity, TPutDto>, BaseResponse<TPutEntity>>
    where TPutEntity : class
    where TPutDto : class, IDto
{
    public async Task<BaseResponse<TPutEntity>> Handle(PutRequest<TPutEntity, TPutDto> request, RequestHandlerDelegate<BaseResponse<TPutEntity>> next, CancellationToken cancellationToken)
    {
        if (validator is not null)
        {
            var result = validator.Validate(request.Dto);
            if (!result.IsValid)
            {
                return await BaseResponse<TPutEntity>.Create(false, message: result.ToString());
            }
        }

        return await next();
    }
}
