using OpenCvSharp;

namespace ITMartinFileSorter.Application.Helpers;

public static class VideoQualityHelper
{
    public static bool IsBlurryVideo(string path)
    {
        try
        {
            using var capture = new VideoCapture(path);

            if (!capture.IsOpened())
                return false;

            int frames = (int)capture.Get(VideoCaptureProperties.FrameCount);
            if (frames <= 0) return false;

            int sampleCount = 5;
            int step = Math.Max(frames / sampleCount, 1);

            double totalVariance = 0;
            int analyzed = 0;

            for (int i = 0; i < frames; i += step)
            {
                capture.Set(VideoCaptureProperties.PosFrames, i);
                using var frame = new Mat();
                capture.Read(frame);

                if (frame.Empty())
                    continue;

                using var gray = new Mat();
                Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                using var lap = new Mat();
                Cv2.Laplacian(gray, lap, MatType.CV_64F);

                var mean = Cv2.Mean(lap);
                double variance = mean.Val0 * mean.Val0;

                totalVariance += variance;
                analyzed++;
            }

            if (analyzed == 0)
                return false;

            double avgVariance = totalVariance / analyzed;

            return avgVariance < 120; // lower = blurry
        }
        catch
        {
            return false;
        }
    }
}