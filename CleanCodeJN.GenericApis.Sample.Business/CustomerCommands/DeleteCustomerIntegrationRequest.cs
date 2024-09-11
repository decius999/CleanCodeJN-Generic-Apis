﻿using CleanCodeJN.GenericApis.Abstractions.Responses;
using CleanCodeJN.GenericApis.Sample.Domain;
using MediatR;

namespace CleanCodeJN.GenericApis.Sample.Core.Business.CustomerCommands;

public class DeleteCustomerIntegrationRequest : IRequest<BaseResponse<Customer>>
{
    public required int Id { get; init; }
}
