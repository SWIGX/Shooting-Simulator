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
    static bool enableAprilTagDetection = false;

    static void Main()
    {
        using var capture = new VideoCapture(0);
        if (!capture.IsOpened())
        {
            Console.WriteLine("Error: Could not open webcam.");
            return;
        }

        //using var windowOriginal = new Window("Webcam Feed", WindowMode.Fullscreen);
        Cv2.NamedWindow("Shooting Simulator", WindowFlags.Normal);
        Cv2.SetWindowProperty("Shooting Simulator", WindowPropertyFlags.Fullscreen, 1);
        using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(8, 8));

        string mp3File = "C:/Users/ECE3D/Downloads/SWIGX-feature-newrec/SWIGX-feature-newrec/shot_win.wav";
        string deerImagePath = "/Users/alexanderdomino/Documents/SWIGX/deer.png";
        Mat deerImage = Cv2.ImRead(deerImagePath, ImreadModes.Unchanged);

        ApriltagDetector? apriltagDetector = null;
        if (enableAprilTagDetection)
            apriltagDetector = new ApriltagDetector("canny", false, "tag25h9");

        LaserDetector laserDetector = new LaserDetector(kernel);
        WinSoundPlayer soundPlayer = new WinSoundPlayer();

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

            //windowOriginal.ShowImage(frame);
            Cv2.ImShow("Shooting Simulator", frame);

            if (Cv2.WaitKey(1) == 27) // ESC to quit
                break;
        }
    }
}
