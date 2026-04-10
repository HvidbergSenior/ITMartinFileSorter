using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using ITMartinFileSorter.Application.Services;
using ITMartinFileSorter.Domain.Interfaces;
using ITMartinFileSorter.Infrastructure.FileSystem;
using ITMartinFileSorter.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<UniversalImageConverterService>();
builder.Services.AddScoped<ImageBatchExportService>();
builder.Services.AddScoped<SubtitleService>();
builder.Services.AddScoped<FolderPathInfoService>();


builder.Services.AddScoped<HomeLocationService>();
builder.Services.AddScoped<JunkDetectionService>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddScoped<ThumbnailService>();
builder.Services.AddScoped<ArchivePathBuilder>();

builder.Services.AddControllers();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();

var configuredRoot = builder.Configuration["MediaSettings:LibraryRoot"];

var libraryRoot = !string.IsNullOrWhiteSpace(configuredRoot) &&
                  Directory.Exists(Path.GetPathRoot(configuredRoot))
    ? configuredRoot
    : Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        "FileSorter");

var libraryPath = Path.Combine(libraryRoot, "Library");

Directory.CreateDirectory(libraryPath);

var contentTypeProvider = new FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".vtt"] = "text/vtt";

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(libraryPath),
    RequestPath = "/libraryfiles",
    ContentTypeProvider = contentTypeProvider
});

app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
