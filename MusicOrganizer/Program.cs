using System;
using System.IO;

namespace MusicOrganizer
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var paths = Directory.GetFiles(@"C:\Users\Daniel\Music\Bollwerk", "*.mp3", SearchOption.AllDirectories);
            foreach (var path in paths)
            {
                var file = TagLib.File.Create(path);
                Console.WriteLine("{0}BPM: {1}", file.Tag.BeatsPerMinute, Path.GetFileName(path));
            }

            Console.ReadLine();
        }
    }
}
