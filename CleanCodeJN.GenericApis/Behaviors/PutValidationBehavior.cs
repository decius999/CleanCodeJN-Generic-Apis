using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Extensions;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

public class PutValidationBehavior<TPutEntity, TPutDto>(IValidator<TPutDto> validator) : IPipelineBehavior<PutRequest<TPutEntity, TPutDto>, BaseResponse<TPutEntity>>
    where TPutEntity : class
    where TPutDto : class, IDto
{
    public async Task<BaseResponse<TPutEntity>> Handle(PutRequest<TPutEntity, TPutDto> request, RequestHandlerDelegate<BaseResponse<TPutEntity>> next, CancellationToken cancellationToken)
    {
        var result = await BehaviorExtensions.Validate<TPutEntity, TPutDto>(validator, request.Dto);

        if (!result.ResultState.Succeeded())
        {
            return result;
        }

        return await next();
    }
}
