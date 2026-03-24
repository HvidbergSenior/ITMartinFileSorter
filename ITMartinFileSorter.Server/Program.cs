using ITMartinFileSorter.Application.Services;
using ITMartinFileSorter.Domain.Interfaces;
using ITMartinFileSorter.Infrastructure.FileSystem;
using ITMartinFileSorter.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

builder.Services.AddScoped<IFileScanner, FileScanner>(); // your scanner
builder.Services.AddScoped<IHashService, Sha256HashService>();  // your hash service

builder.Services.AddScoped<MediaCategorizer>();
builder.Services.AddScoped<AudioCategorizer>();
builder.Services.AddScoped<ImageCategorizer>();
builder.Services.AddScoped<VideoCategorizer>();
builder.Services.AddScoped<DocumentCategorizer>();

builder.Services.AddSingleton<DuplicateService>();
builder.Services.AddScoped<StepService>();
builder.Services.AddScoped<TripGroupingService>();

builder.Services.AddSingleton<VideoConverterService>();
builder.Services.AddSingleton<MediaServerProOptions>();
builder.Services.AddSingleton<MediaServerProService>();
builder.Services.AddScoped<ProfessionalOrganizerService>();
builder.Services.AddScoped<HomeLocationService>();
builder.Services.AddScoped<JunkDetectionService>();
builder.Services.AddScoped<EventDetectionService>();
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
app.UseRouting();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();