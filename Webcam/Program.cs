using System;
using System.Diagnostics;
using System.Collections;
using OpenCvSharp;
using csharp;

namespace ModularLaserPointer
{
    class Program
    {
        static bool enableLaserDetection = true;
        static bool enablePlaySound = true;
        static bool enableAprilTagDetection = true;

        static void Main()
        {
            using var capture = new VideoCapture(0);
            if (!capture.IsOpened())
            {
                Console.WriteLine("Error: Could not open webcam.");
                return;
            }

            using var windowOriginal = new Window("Webcam Feed");
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8, 8));

            string mp3File = "/Users/alexanderdomino/Documents/SWIGX/shot.m4a";
            string deerImagePath = "/Users/alexanderdomino/Documents/SWIGX/deer.png";
            Mat deerImage = Cv2.ImRead(deerImagePath, ImreadModes.Unchanged);

            ApriltagDetector apriltagDetector = null;
            if (enableAprilTagDetection)
                apriltagDetector = new ApriltagDetector("canny", false, "tag25h9");

            LaserDetector laserDetector = new LaserDetector(kernel);
            SoundPlayer soundPlayer = new SoundPlayer();

            while (true)
            {
                using var frame = new Mat();
                capture.Read(frame);
                if (frame.Empty())
                    break;

                if (enableAprilTagDetection)
                    apriltagDetector?.Detect(frame);

                bool laserDetected = false;
                if (enableLaserDetection)
                    laserDetected = laserDetector.Process(frame);

                if (laserDetected && enablePlaySound)
                    soundPlayer.Play(mp3File);

                windowOriginal.ShowImage(frame);

                if (Cv2.WaitKey(1) == 27) // ESC to quit
                    break;
            }
        }
    }

    class LaserDetector
    {
        private readonly Mat _kernel;

        public LaserDetector(Mat kernel)
        {
            _kernel = kernel;
        }

        public bool Process(Mat frame)
        {
            using var gray = new Mat();
            using var brightMask = new Mat();
            using var redDominanceMask = new Mat();
            using var mask = new Mat();
            using var finalMask = new Mat();

            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.Threshold(gray, brightMask, 210, 255, ThresholdTypes.Binary);

            Cv2.Split(frame, out Mat[] channels);
            Mat redChannel = channels[2], greenChannel = channels[1], blueChannel = channels[0];

            Cv2.Threshold(redChannel - greenChannel, redDominanceMask, 30, 255, ThresholdTypes.Binary);
            Cv2.Threshold(redChannel - blueChannel, mask, 30, 255, ThresholdTypes.Binary);
            Cv2.BitwiseAnd(brightMask, redDominanceMask, finalMask);
            Cv2.BitwiseAnd(finalMask, mask, finalMask);

            Cv2.Erode(finalMask, finalMask, _kernel);
            Cv2.MorphologyEx(finalMask, finalMask, MorphTypes.Close, _kernel);

            Cv2.FindContours(finalMask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > 0 && area < 3000)
                {
                    var moments = Cv2.Moments(contour);
                    if (moments.M00 != 0)
                    {
                        int centerX = (int)(moments.M10 / moments.M00);
                        int centerY = (int)(moments.M01 / moments.M00);
                        Cv2.Circle(frame, new Point(centerX, centerY), 10, Scalar.Blue, 2);
                        return true;
                    }
                }
            }

            return false;
        }
    }

    class SoundPlayer
    {
        public void Play(string filePath)
        {
            string afplayCommand = $"afplay {filePath}";

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

    class ApriltagDetector
    {
        private readonly Apriltag _ap;

        public ApriltagDetector(string detector, bool debug, string tagFamily)
        {
            _ap = new Apriltag(detector, debug, tagFamily);
        }

        public void Detect(Mat frame)
        {
            ArrayList detections = _ap.detect(frame);
            foreach (TagDetection detection in detections)
            {
                Console.WriteLine("Tag ID: " + detection.id);
            }
        }
    }
}
