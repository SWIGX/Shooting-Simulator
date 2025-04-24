using System;
using System.Diagnostics;
using System.Collections;
using OpenCvSharp;
using csharp;
using Features;

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
