using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using CleanCodeJN.GenericApis.Extensions;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

public class PostValidationBehavior<TPostEntity, TPostDto>(IValidator<TPostDto> validator) : IPipelineBehavior<PostRequest<TPostEntity, TPostDto>, BaseResponse<TPostEntity>>
    where TPostEntity : class
    where TPostDto : class, IDto
{
    public async Task<BaseResponse<TPostEntity>> Handle(PostRequest<TPostEntity, TPostDto> request, RequestHandlerDelegate<BaseResponse<TPostEntity>> next, CancellationToken cancellationToken)
    {
        var result = await BehaviorExtensions.Validate<TPostEntity, TPostDto>(validator, request.Dto);

        if (!result.ResultState.Succeeded())
        {
            return result;
        }

        return await next();
    }
}
