﻿using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.Repository.EntityFramework.Contracts;
using FluentValidation;
using MediatR;

namespace CleanCodeJN.GenericApis.Commands;

public class PutCommand<TEntity, TDto, TKey>(IMapper mapper, IRepository<TEntity, TKey> repository, IEnumerable<IValidator<TDto>> validators) : IRequestHandler<PutRequest<TEntity, TDto>, BaseResponse<TEntity>>
    where TEntity : class, IEntity<TKey>
{
    public async Task<BaseResponse<TEntity>> Handle(PutRequest<TEntity, TDto> request, CancellationToken cancellationToken)
    {
        var result = await ValidationExtensions.Validate<TEntity, TDto>(validators, request.Dto);

        if (!result.Succeeded)
        {
            return result;
        }

        var entity = await repository.Update(mapper.Map<TEntity>(request.Dto), cancellationToken);

        return await BaseResponse<TEntity>.Create(entity is not null, entity);
    }
}
