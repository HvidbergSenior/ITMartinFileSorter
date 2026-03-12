using OpenCvSharp;

namespace ITMartinFileScanner.Application.Helpers;

public static class ImageQualityHelper
{
    public static bool IsBlurry(string path)
    {
        try
        {
            using var mat = Cv2.ImRead(path, ImreadModes.Grayscale);
            if (mat.Empty()) return false;

            using var laplacian = new Mat();
            Cv2.Laplacian(mat, laplacian, MatType.CV_64F);

            var mean = Cv2.Mean(laplacian);
            double variance = mean.Val0 * mean.Val0;

            return variance < 100; // lower = blur
        }
        catch
        {
            return false;
        }
    }
}