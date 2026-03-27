using CleanCodeJN.GenericApis.Chat.Extensions;
using CleanCodeJN.GenericApis.DataGrid.Extensions;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<CleanCodeJN.GenericApis.Chat.Sample.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCleanCodeJNDataGrid();

builder.Services.AddCleanCodeJNWithAiChat(options =>
{
    options.BackendUrl = "https://localhost:7132";
    options.ShowToolCalls = true;
    options.Title = "CleanCodeJN AI Assistant";
});

await builder.Build().RunAsync();
