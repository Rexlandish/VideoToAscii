using System;
using System.Collections.Generic;

using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using GleamTech.VideoUltimate;

namespace VideoToAscii
{
    internal class VideoReader
    {

        public int width;
        public int height;
        List<Bitmap> bitmaps = new List<Bitmap>();
        public float downscaling = 2;



        public List<string> Start(string filepath, int targetWidth, out int millisecondsDelay, TextType textType = TextType.Regular)
        {

            //int targetWidth = 100;

            //string filepath = @"C:\Users\XXXXX\...\.gif";

            List<string> frames = ParseGif(filepath, targetWidth, out millisecondsDelay, false, textType);
            //List<string> frames = ParseVideo(filepath, targetWidth, out millisecondsDelay, false, TextType.Regular);

            //Execute(frames, millisecondsDelay);

            string res = Newtonsoft.Json.JsonConvert.SerializeObject(frames);
            res = res.Replace("\"", ""); // Removes the quotes in each string. Will be difficult to use if the string itself quotes

            string currentPath = Environment.CurrentDirectory;

            string pathToWriteTo = Path.Combine(currentPath, "output.txt");
            Console.WriteLine($"Writing to {pathToWriteTo}");

            File.Delete(pathToWriteTo);

            using (StreamWriter outputFile = new StreamWriter(pathToWriteTo))
            {
                outputFile.Write(res);   
            }

            return frames;

            

        }


        public static void Execute(List<string> frames, int millisecondsDelay)
        {
            /*
            float targetBpm = 160;
            float secondsPerBeat = 60 / targetBpm;
            float secondsPerLoop = 2 * secondsPerBeat;
            float frameCount = frames.Count;
            float secondsPerFrame = secondsPerLoop / frameCount;

            int millisecondsPerFrame = (int)Math.Floor(secondsPerFrame * 1000);
            */

            int millisecondsPerFrame = millisecondsDelay;

            while (true)
            {
                foreach (var frame in frames)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.WriteLine(frame);
                    Thread.Sleep(millisecondsPerFrame);
                }
            }
        }



        void print(object text) => Console.WriteLine(text);

        public Bitmap BitmapFromFile(string filename)
        {
            Image im = Image.FromFile(filename);
            return (Bitmap)im.Clone();
        }

        // Add frames to a list of bitmaps
        public List<string> ParseVideo(string filename, int targetWidth, out int frameDelay, bool takePixelAverages = false, TextType textType = TextType.Regular)
        {
            List<string> finalStrings = new List<string>();

            using (var videoFrameReader = new VideoFrameReader(filename))
            {
                frameDelay = (int)Math.Floor(1 / videoFrameReader.FrameRate * 1000);

                print($"Loading Data from video ({videoFrameReader.Duration.TotalSeconds * videoFrameReader.FrameRate} frames)");
                while (videoFrameReader.Read())
                {
                    print($"Reading Frame {videoFrameReader.CurrentFrameNumber}...");
                    // Get each frame and convert it to text
                    using (var frame = videoFrameReader.GetFrame())
                    {
                        Bitmap b = (Bitmap)frame;

                        string asciiFrame = Utility.CreateTextFromBitmap(b, targetWidth, takePixelAverages, textType);

                        finalStrings.Add(asciiFrame);
                    }
                }
            }


            print($"Ended with {bitmaps.Count} bitmaps");

            return finalStrings;

        }

        public List<string> ParseGif(string filename, int targetWidth, out int frameDelayMilliseconds, bool takePixelAverages = false, TextType textType = TextType.Regular)
        {
            List<string> finalStrings = new List<string>();

            Image im = Image.FromFile(filename);


            PropertyItem item = im.GetPropertyItem(0x5100); // FrameDelay in libgdiplus
            // Time is in milliseconds
            frameDelayMilliseconds = (item.Value[0] + item.Value[1] * 256) * 10;
            

            for (int i = 0; i < im.GetFrameCount(FrameDimension.Time); i++)
            {
                im.SelectActiveFrame(FrameDimension.Time, i);
                Bitmap b = (Bitmap)im.Clone();

                string asciiFrame = Utility.CreateTextFromBitmap(b, targetWidth, takePixelAverages, textType);

                Console.WriteLine($"Frame {i}/{im.GetFrameCount(FrameDimension.Time)}...");
                finalStrings.Add(asciiFrame);
            }

            return finalStrings;


        }


        public enum TextType
        {
            Regular,
            Dithered
        }


    }
}
