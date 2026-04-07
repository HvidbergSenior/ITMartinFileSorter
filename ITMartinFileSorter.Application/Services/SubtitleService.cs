using System.Diagnostics;
using System.Text;

namespace ITMartinFileSorter.Application.Services;

public sealed class SubtitleService
{
    private const string WhisperExe =
        @"C:\Tools\Whisper\Release\whisper-cli.exe";

    private const string MediumModelPath =
        @"C:\Tools\Whisper\models\ggml-medium.bin";

    private static string FfmpegExe =>
        Path.Combine(
            AppContext.BaseDirectory,
            "ffmpeg",
            "ffmpeg.exe");

    public async Task<string?> GenerateDanishSubtitlesAsync(
        string videoPath,
        bool isLongFilm = false)
    {
        var wavPath = Path.ChangeExtension(videoPath, ".wav");
        var srtPath = $"{wavPath}.srt";
        var vttPath = Path.ChangeExtension(videoPath, ".da.vtt");

        var audioOk = await ExtractAudioAsync(videoPath, wavPath);

        if (!audioOk)
            return null;

        var beamSize = isLongFilm ? 6 : 4;

        var whisperOk =
            await RunWhisperAsync(wavPath, beamSize);

        if (!whisperOk || !File.Exists(srtPath))
            return null;

        ConvertSrtToVtt(srtPath, vttPath);

        CleanupTempFiles(wavPath, srtPath);

        return vttPath;
    }

    private async Task<bool> ExtractAudioAsync(
        string videoPath,
        string wavPath)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = FfmpegExe,
                Arguments =
                    $"-y -i \"{videoPath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{wavPath}\"",
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        await process.WaitForExitAsync();

        return File.Exists(wavPath);
    }

    private async Task<bool> RunWhisperAsync(
        string wavPath,
        int beamSize)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WhisperExe,
                Arguments =
                    $"-m \"{MediumModelPath}\" -f \"{wavPath}\" -l da -osrt --beam-size {beamSize}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        await process.WaitForExitAsync();

        return process.ExitCode == 0;
    }

    private void ConvertSrtToVtt(
        string srtPath,
        string vttPath)
    {
        var lines = File.ReadAllLines(srtPath);

        using var writer =
            new StreamWriter(vttPath, false, Encoding.UTF8);

        writer.WriteLine("WEBVTT");
        writer.WriteLine();

        foreach (var line in lines)
        {
            writer.WriteLine(line.Replace(',', '.'));
        }
    }

    private void CleanupTempFiles(
        string wavPath,
        string srtPath)
    {
        try
        {
            if (File.Exists(wavPath))
                File.Delete(wavPath);

            if (File.Exists(srtPath))
                File.Delete(srtPath);
        }
        catch
        {
        }
    }
}
