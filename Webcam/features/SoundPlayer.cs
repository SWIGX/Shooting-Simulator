namespace Features
{
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
}
