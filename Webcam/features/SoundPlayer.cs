using System;
using System.Diagnostics;

namespace Features
{
    class SoundPlayer
    {
        private readonly TimeSpan _bounceTime = TimeSpan.FromMilliseconds(200); // 500ms cooldown
        private DateTime _lastPlayed = DateTime.MinValue;

        public void Play(string filePath)
        {
            if ((DateTime.Now - _lastPlayed) < _bounceTime)
                return;

            _lastPlayed = DateTime.Now;

            string afplayCommand = $"afplay \"{filePath}\"";

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "bash",
                    Arguments = $"-c \"{afplayCommand}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process? process = Process.Start(startInfo);

                if (process == null)
                {
                    Console.WriteLine("Failed to start the audio process.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}
