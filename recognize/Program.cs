
using System;
using OpenCvSharp;

class LaserPointerImageTest
{
    static void Main()
    {
        string imagePath = "/Users/alexanderdomino/Documents/SWIGX/testimg.png";

        if (string.IsNullOrEmpty(imagePath) || !System.IO.File.Exists(imagePath))
        {
            Console.WriteLine("Invalid file path.");
            return;
        }

        using var image = Cv2.ImRead(imagePath);
        using var mask = new Mat();
        using var brightMask = new Mat();
        using var redDominanceMask = new Mat();
        using var finalMask = new Mat();

        // Convert to grayscale for brightness detection
        using var gray = new Mat();
        Cv2.CvtColor(image, gray, ColorConversionCodes.BGR2GRAY);
        Cv2.Threshold(gray, brightMask, 190, 255, ThresholdTypes.Binary); // Slightly increased threshold

        // Extract color channels
        Mat[] channels;
        Cv2.Split(image, out channels);
        Mat redChannel = channels[2], greenChannel = channels[1], blueChannel = channels[0];

        // Create red-dominance mask: (Red > Green) & (Red > Blue)
        Cv2.Threshold(redChannel - greenChannel, redDominanceMask, 30, 255, ThresholdTypes.Binary); // Increased threshold
        Cv2.Threshold(redChannel - blueChannel, mask, 30, 255, ThresholdTypes.Binary); // Increased threshold

        // Combine bright mask and red dominance mask
        Cv2.BitwiseAnd(brightMask, redDominanceMask, finalMask);
        Cv2.BitwiseAnd(finalMask, mask, finalMask);

        // Morphological operations to fill potential rings
        Mat kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8, 8)); // Reduced kernel size
        Cv2.Dilate(finalMask, finalMask, kernel);
        Cv2.MorphologyEx(finalMask, finalMask, MorphTypes.Close, kernel);

        // Find contours
        Cv2.FindContours(finalMask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

        foreach (var contour in contours)
        {
            double area = Cv2.ContourArea(contour);
            if (area > 5 && area < 500) // Adjust as needed
            {
                var moments = Cv2.Moments(contour);
                if (moments.M00 != 0)
                {
                    int centerX = (int)(moments.M10 / moments.M00);
                    int centerY = (int)(moments.M01 / moments.M00);

                    // Draw a circle around detected laser dot
                    Cv2.Circle(image, new Point(centerX, centerY), 10, Scalar.Blue, 2);
                }
            }
        }

        // Display results
        Cv2.ImShow("Original Image", image);
        Cv2.ImShow("Detected Laser Mask", finalMask);
        Cv2.WaitKey(0);
    }
}
