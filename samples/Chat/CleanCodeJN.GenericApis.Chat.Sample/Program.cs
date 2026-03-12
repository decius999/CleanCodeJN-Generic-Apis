using CleanCodeJN.GenericApis.Chat.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CleanCodeJN.GenericApis.Chat.Sample.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();

builder.Services.AddCleanCodeJNWithAiChat(options =>
{
    options.BackendUrl = "https://localhost:7132";
    options.ShowToolCalls = true;
    options.Title = "CleanCodeJN AI Assistant";
    // options.BearerToken = "your-token"; // set if backend requires auth
});

await builder.Build().RunAsync();
