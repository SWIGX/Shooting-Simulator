using System;
using OpenCvSharp;

class Program
{
    static void Main()
    {
        using var capture = new VideoCapture(0);  // Open the webcam (0 is typically the default camera)
        if (!capture.IsOpened())
        {
            Console.WriteLine("Failed to open webcam.");
            return;
        }

        using var window = new Window("Webcam Feed");
        using var maskWindow = new Window("Mask View");  // Debugging window for mask
        using var frame = new Mat();
        using var hsvFrame = new Mat();
        using var mask = new Mat();
        using var blurredFrame = new Mat();

        // Specific HSV range for detecting the red laser point
        int lowerH = 160, upperH = 190;  // Hue range (example: red hue)
        int lowerS = 15, upperS = 30;   // Saturation range
        int lowerV = 90, upperV = 100;   // Value range

        lowerS = (int)(lowerS * 2.55);  // Convert Saturation percentage to OpenCV scale (0-255)
        upperS = (int)(upperS * 2.55);  // Convert Saturation percentage to OpenCV scale (0-255)
        lowerV = (int)(lowerV * 2.55);  // Convert Value percentage to OpenCV scale (0-255)
        upperV = (int)(upperV * 2.55);

        while (true)
        {
            capture.Read(frame);
            if (frame.Empty()) break;

            // Apply Gaussian Blur to reduce noise
            Cv2.GaussianBlur(frame, blurredFrame, new Size(7, 7), 0);

            // Convert the frame to HSV color space
            Cv2.CvtColor(blurredFrame, hsvFrame, ColorConversionCodes.BGR2HSV);

            // Define the lower and upper bounds for the HSV range
            Scalar lowerBound = new Scalar(lowerH, lowerS, lowerV);  // Lower HSV bound
            Scalar upperBound = new Scalar(upperH, upperS, upperV);  // Upper HSV bound

            // Threshold the HSV image for the red color range (laser point)
            Cv2.InRange(hsvFrame, lowerBound, upperBound, mask);

            // Morphological transformations to remove noise
            Cv2.MorphologyEx(mask, mask, MorphTypes.Open, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)));
            Cv2.MorphologyEx(mask, mask, MorphTypes.Close, Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3)));

            // Find contours of the red laser point
            Cv2.FindContours(mask, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                double area = Cv2.ContourArea(contour);
                if (area > 5 && area < 500)  // Filter by area to detect red dot
                {
                    var moments = Cv2.Moments(contour);
                    if (moments.M00 != 0)
                    {
                        int centerX = (int)(moments.M10 / moments.M00);
                        int centerY = (int)(moments.M01 / moments.M00);

                        // Draw a circle around the detected laser dot
                        Cv2.Circle(frame, new Point(centerX, centerY), 10, Scalar.Blue, 2);
                    }
                }
            }

            // Show the frame with detection
            window.ShowImage(frame);
            maskWindow.ShowImage(mask);  // Show the mask for debugging

            // Check for key press
            int key = Cv2.WaitKey(1);
            if (key == 'q')  // Press 'q' to exit
                break;
            else if (key == 'p')  // Press 'p' to print HSV values for debugging
            {
                Console.WriteLine("Printing HSV values:");
                for (int y = 0; y < hsvFrame.Rows; y++)
                {
                    for (int x = 0; x < hsvFrame.Cols; x++)
                    {
                        Vec3b pixel = hsvFrame.At<Vec3b>(y, x);
                        Console.WriteLine($"Pixel({x}, {y}) - H: {pixel[0]}, S: {pixel[1]}, V: {pixel[2]}");
                    }
                }
            }
        }

        capture.Release();  // Release the webcam
    }
}
