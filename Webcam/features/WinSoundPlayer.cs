using System;
using System.Media;

namespace Features
{
    class WinSoundPlayer
    {
        private readonly TimeSpan _bounceTime = TimeSpan.FromMilliseconds(200); // 200ms cooldown
        private DateTime _lastPlayed = DateTime.MinValue;

        public void Play(string filePath)
        {
            if ((DateTime.Now - _lastPlayed) < _bounceTime)
                return;

            _lastPlayed = DateTime.Now;

            try
            {
                using SoundPlayer player = new SoundPlayer(filePath);
                player.Play(); // Use PlaySync() if you want it to block
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}
