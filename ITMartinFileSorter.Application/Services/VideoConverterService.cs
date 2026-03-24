using System.Diagnostics;

namespace ITMartinFileSorter.Application.Services;

public class VideoConverterService
{
    private readonly string _ffmpegPath =
        Path.Combine(AppContext.BaseDirectory, "Ffmpeg", "ffmpeg.exe");

    private readonly string[] _videoExtensions =
    {
        ".avi", ".mov", ".mkv", ".wmv", ".flv",
        ".mpeg", ".mpg", ".ts", ".m4v"
    };

    private void Log(string message)
    {
        Console.WriteLine($"[VideoConverter] {message}");
    }

    public async Task ConvertFolderAsync(
        string rootFolder,
        Action<string>? progress = null,
        bool deleteOriginal = false)
    {
        Log($"Scanning folder: {rootFolder}");

        var files = Directory
            .EnumerateFiles(rootFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => _videoExtensions.Contains(
                Path.GetExtension(f).ToLower()))
            .ToList();

        Log($"Found {files.Count} video files");

        int total = files.Count;
        int done = 0;

        progress?.Invoke($"0/{total} videos processed");

        if (total == 0)
        {
            Log("No videos to process.");
            return;
        }

        await Parallel.ForEachAsync(
            files,
            new ParallelOptions { MaxDegreeOfParallelism = 1 }, // easier debugging
            async (file, _) =>
            {
                Log($"Processing: {file}");

                if (ShouldSkip(file))
                {
                    Log($"Skipped: {file}");
                    var skipped = Interlocked.Increment(ref done);
                    progress?.Invoke($"{skipped}/{total} videos processed");
                    return;
                }

                await ConvertFileAsync(file, deleteOriginal);

                var current = Interlocked.Increment(ref done);
                progress?.Invoke($"{current}/{total} videos processed");
            });
    }

    private bool ShouldSkip(string file)
    {
        var info = new FileInfo(file);

        if (info.Length < 1_000_000)
        {
            Log($"Skip small file: {file}");
            return true;
        }

        var name = Path.GetFileName(file).ToLower();

        if (name.Contains("sample") || name.Contains("trailer"))
        {
            Log($"Skip sample/trailer: {file}");
            return true;
        }

        return false;
    }

    private async Task ConvertFileAsync(string inputFile, bool deleteOriginal)
    {
        Log($"Converting: {inputFile}");

        var ext = Path.GetExtension(inputFile).ToLower();
        var outputFile = Path.ChangeExtension(inputFile, ".mp4");

        if (File.Exists(outputFile))
        {
            Log($"Output already exists: {outputFile}");
            return;
        }

        if (ext == ".mp4")
        {
            Log("Already MP4, skipping");
            return;
        }

        // 🚀 1) PHONE-OPTIMIZED REMUX (FAST)
        // Copies ONLY main video + audio streams
        var remuxArgs =
            $"-y -i \"{inputFile}\" " +
            "-map 0:v:0 -map 0:a:0 " +
            "-c copy -movflags +faststart " +
            $"\"{outputFile}\"";

        Log("Attempt fast remux (video+audio only)");

        if (await RunFfmpegAsync(remuxArgs))
        {
            Log("Remux success");
            Cleanup(inputFile, outputFile, deleteOriginal);
            return;
        }

        // 🚀 2) GPU ENCODE (Intel QuickSync)
        var gpuArgs =
            $"-y -i \"{inputFile}\" " +
            "-c:v h264_qsv -preset fast " +
            "-c:a aac -b:a 192k " +
            $"\"{outputFile}\"";

        Log("Attempt GPU encode");

        if (await RunFfmpegAsync(gpuArgs))
        {
            Log("GPU encode success");
            Cleanup(inputFile, outputFile, deleteOriginal);
            return;
        }

        // 🧠 3) CPU FALLBACK (ULTRAFAST)
        var cpuArgs =
            $"-y -i \"{inputFile}\" " +
            "-c:v libx264 -preset ultrafast -crf 23 " +
            "-c:a aac -b:a 192k " +
            $"\"{outputFile}\"";

        Log("Attempt CPU encode");

        if (await RunFfmpegAsync(cpuArgs))
        {
            Log("CPU encode success");
            Cleanup(inputFile, outputFile, deleteOriginal);
        }
        else
        {
            Log("CPU encode FAILED");
        }
    }

    private static void Cleanup(
        string inputFile,
        string outputFile,
        bool deleteOriginal)
    {
        if (deleteOriginal && File.Exists(outputFile))
            File.Delete(inputFile);
    }

    private async Task<bool> RunFfmpegAsync(string args)
    {
        Log($"Running FFmpeg: {_ffmpegPath} {args}");

        if (!File.Exists(_ffmpegPath))
        {
            Log("FFmpeg executable NOT FOUND!");
            return false;
        }

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = args,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        // 🚀 Consume output streams live (prevents deadlocks)
        _ = Task.Run(async () =>
        {
            while (!process.StandardOutput.EndOfStream)
            {
                var line = await process.StandardOutput.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Log($"OUT: {line}");
            }
        });

        _ = Task.Run(async () =>
        {
            while (!process.StandardError.EndOfStream)
            {
                var line = await process.StandardError.ReadLineAsync();
                if (!string.IsNullOrWhiteSpace(line))
                    Log($"ERR: {line}");
            }
        });

        await process.WaitForExitAsync();

        Log($"FFmpeg exit code: {process.ExitCode}");

        return process.ExitCode == 0;
    }
}