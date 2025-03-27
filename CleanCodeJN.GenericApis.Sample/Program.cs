using System.Text.Json.Serialization;
using CleanCodeJN.GenericApis.Extensions;
using CleanCodeJN.GenericApis.Sample.DataAccess;
using CleanCodeJN.GenericApis.Sample.Extensions;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers()
    .AddNewtonsoftJson(); // this is needed for "http patch" only. If you do not need to use patch, you can remove this line

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.AddCleanCodeJN<MyDbContext>(options =>
{
    options.ApplicationAssemblies =
    [
        typeof(CleanCodeJN.GenericApis.Sample.Business.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly,
        typeof(CleanCodeJN.GenericApis.Sample.Domain.AssemblyRegistration).Assembly
    ];
    options.ValidatorAssembly = typeof(CleanCodeJN.GenericApis.Sample.Core.AssemblyRegistration).Assembly;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCleanCodeJNWithMinimalApis();
app.MapControllers();

// For seeding of in-memory db only
app.EnsureDatabaseCreated();

app.Run();

