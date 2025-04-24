namespace Features
{
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
