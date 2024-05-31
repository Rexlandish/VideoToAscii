using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VideoToAscii
{
    internal class Program
    {
        float frameData;

        static void Main(string[] args)
        {


            VideoReader reader = new VideoReader();
            var frames = reader.Start(@"C:\Path\To\.gif", 50, out int millisecondsDelay, VideoReader.TextType.Dithered);
            VideoReader.Execute(frames, millisecondsDelay);



            // Create and format gradient array
            /*
            float[,] gradientArray = new float[20, 20];

            for (int i = 0; i < gradientArray.GetLength(0); i++)
            {
                for (int j = 0; j < gradientArray.GetLength(1); j++)
                {
                    float percentage = (i + j) / (0.5f * (20f + 20f));

                    if (percentage >= 1)
                        percentage = 2 - percentage;

                    //Console.WriteLine(percentage);
                    gradientArray[i, j] = percentage;
                }
            }
            */

            /*
            string filepath = @"C:\Filepath";
            Bitmap b = reader.BitmapFromFile(filepath);
            float[,] f = Utility.CreateArrayFromBitmap(b, 5);

            Console.WriteLine("⢙");

            //Console.WriteLine(reader.ArrayToString(gradientArray));
            Console.ReadLine();
            Utility.ArrayToString(Dithering.Dither(f));
            */

            /*
            float[,] arr = new float[4, 2]
            {
                { 1, 0 },
                { 1, 1 },
                { 0, 1 },
                { 1, 1 }
            };

            Console.WriteLine(Utility.ArrayToBraille(arr));
            */

        }


    }
}
