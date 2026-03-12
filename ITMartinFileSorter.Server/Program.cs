using MediaCleaner.Application.Interfaces;
using MediaCleaner.Infrastructure.FileSystem;
using MediaCleaner.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddSingleton<IFileScanner, FileScanner>();
builder.Services.AddSingleton<IHashService, Sha256HashService>();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();