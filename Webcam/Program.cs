using System;
using System.Diagnostics;
using OpenCvSharp;

class LaserPointerWebcam
{
    static void Main()
    {
        using var capture = new VideoCapture(0);
        if (!capture.IsOpened())
        {
            Console.WriteLine("Error: Could not open webcam.");
            return;
        }

        using var windowOriginal = new Window("Webcam Feed");
        using var windowMask = new Window("Detected Laser Mask");
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8, 8));

        // Path to your MP3 file
        string mp3File = "/Users/alexanderdomino/Documents/SWIGX/shot.m4a";

        while (true)
        {
            using var frame = new Mat();
            capture.Read(frame);
            if (frame.Empty())
                break;

            using var mask = new Mat();
            using var brightMask = new Mat();
            using var redDominanceMask = new Mat();
            using var finalMask = new Mat();

            // Convert to grayscale for brightness detection
            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, brightMask, 210, 255, ThresholdTypes.Binary);

            // Extract color channels
            Mat[] channels;
            Cv2.Split(frame, out channels);
            Mat redChannel = channels[2], greenChannel = channels[1], blueChannel = channels[0];

            // Create red-dominance mask: (Red > Green) & (Red > Blue)
            Cv2.Threshold(redChannel - greenChannel, redDominanceMask, 30, 255, ThresholdTypes.Binary);
            Cv2.Threshold(redChannel - blueChannel, mask, 30, 255, ThresholdTypes.Binary);

            // Combine bright mask and red dominance mask
            Cv2.BitwiseAnd(brightMask, redDominanceMask, finalMask);
            Cv2.BitwiseAnd(finalMask, mask, finalMask);

            // Morphological operations to fill potential rings
            Cv2.Erode(finalMask, finalMask, kernel);
            Cv2.MorphologyEx(finalMask, finalMask, MorphTypes.Close, kernel);

            // Find contours
            Cv2.FindContours(finalMask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            bool laserDetected = false;

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                Console.WriteLine($"Contour area: {area}");
                if (area > 0 && area < 3000)
                {
                    var moments = Cv2.Moments(contour);
                    if (moments.M00 != 0)
                    {
                        int centerX = (int)(moments.M10 / moments.M00);
                        int centerY = (int)(moments.M01 / moments.M00);

                        // Draw a circle around detected laser dot
                        Cv2.Circle(frame, new Point(centerX, centerY), 10, Scalar.Blue, 2);

                        laserDetected = true;
                    }
                }
            }

            // Play sound if laser is detected
            if (laserDetected)
            {
                PlaySound(mp3File);
            }

            // Display results
            windowOriginal.ShowImage(frame);
            windowMask.ShowImage(finalMask);

            if (Cv2.WaitKey(1) == 27) // Press 'Esc' to exit
                break;
        }
    }

    // Method to play the sound using afplay
    static void PlaySound(string mp3File)
    {
        string afplayCommand = $"afplay {mp3File}";

        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{afplayCommand}\"",
                CreateNoWindow = true,
                UseShellExecute = false
            };
            Process process = Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error playing sound: {ex.Message}");
        }
    }
}
