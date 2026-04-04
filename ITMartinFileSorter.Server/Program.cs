using ITMartinFileSorter.Application.Services;
using ITMartinFileSorter.Domain.Interfaces;
using ITMartinFileSorter.Infrastructure.FileSystem;
using ITMartinFileSorter.Infrastructure.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<IFileScanner, FileScanner>();
builder.Services.AddScoped<IHashService, Sha256HashService>();

builder.Services.AddScoped<MediaCategorizer>();
builder.Services.AddScoped<AudioCategorizer>();
builder.Services.AddScoped<ImageCategorizer>();
builder.Services.AddScoped<VideoCategorizer>();
builder.Services.AddScoped<DocumentCategorizer>();

builder.Services.AddSingleton<DuplicateService>();
builder.Services.AddScoped<TripGroupingService>();
builder.Services.AddScoped<FastUniversalVideoConverterService>();
builder.Services.AddScoped<FastVideoBatchExportService>();
builder.Services.AddScoped<HomeLocationService>();
builder.Services.AddScoped<JunkDetectionService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<ThumbnailService>();
builder.Services.AddScoped<ArchivePathBuilder>();

builder.Services.AddSingleton(new VideoThumbnailService(
    Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "media_temp")
));

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

var exportPath =
    @"C:\Users\hvidb\Desktop\BillederIphone\Film\Exported";

Console.WriteLine("===== STATIC FILE DEBUG =====");
Console.WriteLine($"Export path: {exportPath}");
Console.WriteLine($"Exists: {Directory.Exists(exportPath)}");

if (Directory.Exists(exportPath))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(exportPath),
        RequestPath = "/libraryfiles"
    });

    Console.WriteLine("Static library mapping enabled: /libraryfiles");
}
else
{
    Console.WriteLine("WARNING: export folder does not exist");
}

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();