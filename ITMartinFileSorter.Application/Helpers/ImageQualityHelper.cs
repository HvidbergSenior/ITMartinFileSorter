using OpenCvSharp;

namespace ITMartinFileSorter.Application.Helpers;

public static class ImageQualityHelper
{
    public static bool IsBlurry(string path)
    {
        try
        {
            using var image = Cv2.ImRead(path, ImreadModes.Grayscale);

            if (image.Empty())
                return false;

            using var laplacian = new Mat();
            Cv2.Laplacian(image, laplacian, MatType.CV_64F);

            Cv2.MeanStdDev(laplacian, out var mean, out var stddev);

            double variance = stddev.Val0 * stddev.Val0;

            return variance < 100; // threshold
        }
        catch
        {
            return false;
        }
    }
}