namespace Features
{
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
}
