﻿using System.Linq.Expressions;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class GetByIdRequest<TEntity, TKey> : GetBaseRequest, IRequest<BaseResponse<TEntity>>
      where TEntity : class
{
    public required TKey Id { get; init; }

    public List<Expression<Func<TEntity, object>>> Includes { get; set; } = [];

    public Expression<Func<TEntity, bool>> Where { get; set; } = x => true;
}
