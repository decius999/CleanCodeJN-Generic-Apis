using CleanCodeJN.GenericApis.Abstractions.Contracts;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Commands;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Behaviors;

public class PostValidationBehavior<TPostEntity, TPostDto>(IValidator<TPostDto> validator) : IPipelineBehavior<PostRequest<TPostEntity, TPostDto>, BaseResponse<TPostEntity>>
    where TPostEntity : class
    where TPostDto : class, IDto
{
    public async Task<BaseResponse<TPostEntity>> Handle(PostRequest<TPostEntity, TPostDto> request, RequestHandlerDelegate<BaseResponse<TPostEntity>> next, CancellationToken cancellationToken)
    {
        if (validator is not null)
        {
            var result = validator.Validate(request.Dto);
            if (!result.IsValid)
            {
                return await BaseResponse<TPostEntity>.Create(false, message: result.ToString());
            }
        }

        return await next();
    }
}
