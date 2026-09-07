using AgentsDashboard.Components;
using AgentsDashboard.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 10 * 1024 * 1024);

builder.Services.AddSingleton<FeatureStore>();
builder.Services.AddSingleton<ProcessRunner>();
builder.Services.AddSingleton<GitService>();
builder.Services.AddSingleton<DockerService>();
builder.Services.AddSingleton<TerminalService>();
builder.Services.AddSingleton<AgentPrompts>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
