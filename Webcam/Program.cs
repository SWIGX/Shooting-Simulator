using System;
using System.Diagnostics;
using System.Collections;
using OpenCvSharp;
using csharp;

namespace csharp
{
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

            // Load deer.png image
            Mat deerImage = Cv2.ImRead("/Users/alexanderdomino/Documents/SWIGX/deer.png", ImreadModes.Unchanged); // Ensure you load with alpha channel

            // AprilTag detector setup
            Apriltag ap = new Apriltag("canny", false, "tag25h9");

            while (true)
            {
                using var frame = new Mat();
                capture.Read(frame);
                if (frame.Empty())
                    break;

                // === AprilTag Detection ===
                ArrayList detections = ap.detect(frame);
                foreach (TagDetection detection in detections)
                {
                    Console.WriteLine("Tag ID: " + detection.id);

                    Point[] points = detection.points;
                    if (points != null && points.Length == 4)
                    {
                        // Draw the square around the tag
                        for (int i = 0; i < 4; i++)
                        {
                            Point p1 = points[i];
                            Point p2 = points[(i + 1) % 4]; // Wrap to connect last point to first
                            Cv2.Line(frame, p1, p2, Scalar.LimeGreen, 2);
                        }

                        // Draw the tag ID at the center
                        int centerX = (points[0].X + points[2].X) / 2;
                        int centerY = (points[0].Y + points[2].Y) / 2;
                        Cv2.PutText(frame, detection.id.ToString(), new Point(centerX, centerY),
                                    HersheyFonts.HersheySimplex, 0.5, Scalar.Yellow, 2);

                        // Calculate the bounding box of the detected AprilTag
                        Rect tagRect = new Rect(
                            Math.Min(Math.Min(points[0].X, points[1].X), Math.Min(points[2].X, points[3].X)),
                            Math.Min(Math.Min(points[0].Y, points[1].Y), Math.Min(points[2].Y, points[3].Y)),
                            Math.Abs(points[0].X - points[2].X),
                            Math.Abs(points[0].Y - points[1].Y)
                        );

                        // Resize the deer image to fit inside the AprilTag bounding box
                        Mat resizedDeerImage = new Mat();
                        Cv2.Resize(deerImage, resizedDeerImage, new Size(tagRect.width, tagRect.height));

                        // Create a mask for the alpha channel of the deer image
                        Mat deerAlphaChannel = resizedDeerImage.Split()[3]; // Assuming alpha channel is the fourth channel

                        // Use the tagRect to overlay the deer image on the frame
                        for (int y = 0; y < resizedDeerImage.Rows; y++)
                        {
                            for (int x = 0; x < resizedDeerImage.Cols; x++)
                            {
                                if (deerAlphaChannel.At<byte>(y, x) > 0) // Only blend where alpha > 0
                                {
                                    frame.At<Vec3b>(tagRect.Top + y, tagRect.Left + x) = resizedDeerImage.At<Vec3b>(y, x);
                                }
                            }
                        }
                    }
                }

                // === Laser Detection ===
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
                //windowMask.ShowImage(finalMask);

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
}
