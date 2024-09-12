using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.DataAccess;
using CleanCodeJN.GenericApis.Sample.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.RegisterRepositoriesCommandsWithAutomaticMapping<MyDbContext>(applicationAssemblies:
[
    typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
    typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
    typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly
],
validatorAssembly: typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.RegisterApis();
app.MapControllers();

// For seeding of in-memory db only
app.EnsureDatabaseCreated();

app.Run();

