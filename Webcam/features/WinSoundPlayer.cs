using System;
using System.Media;

namespace Features
{
    class WinSoundPlayer
    {
        private readonly TimeSpan _bounceTime = TimeSpan.FromMilliseconds(200);
        private DateTime _lastPlayed = DateTime.MinValue;

        public void Play(string filePath)
        {
            if ((DateTime.Now - _lastPlayed) < _bounceTime)
                return;

            _lastPlayed = DateTime.Now;

            try
            {
                var player = new SoundPlayer(filePath);
                player.Play(); // or .PlaySync() to block until done
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error playing sound: {ex.Message}");
            }
        }
    }
}
