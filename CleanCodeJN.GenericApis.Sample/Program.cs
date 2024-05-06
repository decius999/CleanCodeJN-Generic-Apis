using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.Context;
using CleanCodeJN.GenericApis.Sample.Dtos;
using CleanCodeJN.GenericApis.Sample.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterRepositoriesCommandsWithAutomapper<MyDbContext>(cfg =>
{
    cfg.CreateMap<Customer, CustomerPutDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerPostDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerGetDto>().ReverseMap();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterApis();

app.Run();

