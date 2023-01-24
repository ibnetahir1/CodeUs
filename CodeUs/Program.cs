using CodeUs.Hubs;
using CodeUs.Shared.StateContainers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddScoped<PlayInfoContainer>();
builder.Services.AddSingleton<IRoomsService, RoomsService>();
builder.Services.AddScoped<IWordsService, WordsService>();
builder.Services.AddScoped(sp =>
{
    var navMan = sp.GetRequiredService<NavigationManager>();

    var _hubUrl = navMan.BaseUri.TrimEnd('/') + GameHub.HubUrl;

    return new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<GameHub>(GameHub.HubUrl);

app.Run();
