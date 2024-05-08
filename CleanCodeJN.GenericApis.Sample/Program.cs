using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.Context;
using CleanCodeJN.GenericApis.Sample.Dtos;
using CleanCodeJN.GenericApis.Sample.Models;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.RegisterRepositoriesCommandsWithAutomapper<MyDbContext>(cfg =>
{
    cfg.CreateMap<Customer, CustomerPutDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerPostDto>().ReverseMap();
    cfg.CreateMap<Customer, CustomerGetDto>().ReverseMap();

    cfg.CreateMap<Invoice, InvoicePutDto>().ReverseMap();
    cfg.CreateMap<Invoice, InvoicePostDto>().ReverseMap();
    cfg.CreateMap<Invoice, InvoiceGetDto>().ReverseMap();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterApis();
app.MapControllers();

app.Run();

