﻿using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PostRequest<TEntity, TDto> : IRequest<BaseResponse<TEntity>>
    where TEntity : class
{
    public TDto Dto { get; init; }

    public bool SkipValidation { get; init; }
}
