﻿using AutoMapper;
using CleanCodeJN.GenericApis.Abstractions.Contracts;
using MediatR;

namespace CleanCodeJN.GenericApis.API;

public class GetById<TEntity, TGetDto>(IMediator commandBus, IMapper mapper) : GetByIdBase<TEntity, TGetDto>(commandBus, mapper)
    where TEntity : class
    where TGetDto : class, IDto
{
}
