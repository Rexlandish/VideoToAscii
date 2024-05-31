using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static VideoToAscii.VideoReader;

namespace VideoToAscii
{
    internal class Utility
    {

        public static string CreateTextFromBitmap(Bitmap b, int targetWidth, bool takePixelAverages = false, TextType textType = TextType.Regular)
        {

            switch (textType)
            {

                case TextType.Regular:

                    int width = b.Width;
                    int height = b.Height;

                    // How many pixels to skip when reading an image
                    int sampleGap = (int)Math.Floor(width / (float)targetWidth);

                    // Iterate through each pixel
                    List<string> final = new List<string>();
                    for (int y = 0; y < height; y += sampleGap)
                    {

                        string row = "";
                        for (int x = 0; x < width; x += sampleGap)
                        {
                            // Convert pixels to character based on brightness
                            float greyscaleValue = 0;

                            if (takePixelAverages)
                            {
                                // Loop through a radius of the current pixel to get an average pixel brightness

                                int pixelsCounted = 0;

                                for (int i = 0; i < sampleGap; i++)
                                {
                                    for (int j = 0; j < sampleGap; j++)
                                    {
                                        Color col = b.GetPixel(x, y);

                                        greyscaleValue += ColorToGreyscale(col);
                                        pixelsCounted++;
                                    }
                                }

                                greyscaleValue /= pixelsCounted;

                            }
                            else
                            {
                                Color col = b.GetPixel(x, y);

                                greyscaleValue = Utility.ColorToGreyscale(col);

                            }

                            row += Utility.GetCharFromDensity(greyscaleValue);
                        }

                        final.Add(row);
                    }

                    return string.Join("\n", final);

                case TextType.Dithered:


                    float ditherSampleGap = (float)Math.Floor(b.Width / (float)targetWidth);
                    float[,] matrixArray = new float[
                        (int)Math.Floor(b.Height / ditherSampleGap),
                        (int)Math.Floor(b.Width / ditherSampleGap)
                    ];

                    for (int y = 0; y < Math.Floor(b.Height / ditherSampleGap); y++)
                    {
                        for (int x = 0; x < Math.Floor(b.Width / ditherSampleGap); x++)
                        {
                            matrixArray[y, x] = Utility.ColorToGreyscale(b.GetPixel((int)Math.Floor(x*ditherSampleGap), (int)Math.Floor(y * ditherSampleGap)));
                        }
                    }

                    return Utility.ArrayToString(Dithering.Dither(matrixArray));

                default:
                    throw new Exception("Invalid TextType");

            }
        }

        public static float[,] CreateArrayFromBitmap(Bitmap b, float sampleRate = 1)
        {
            float[,] array = new float[(int)Math.Floor(b.Height / sampleRate), (int)Math.Floor(b.Width / sampleRate)];

            for (int x = 0; x < Math.Floor(b.Width / sampleRate); x++)
            {
                for (int y = 0; y < Math.Floor(b.Height / sampleRate); y++)
                {
                    array[y, x] = ColorToGreyscale(b.GetPixel((int)Math.Floor(x * sampleRate), (int)Math.Floor(y * sampleRate)));
                }
            }

            return array;
        }





        // Values must be 0, 1, array must be float[4,2]
        public static char ArrayToBraille(float[,] array)
        {
            Vector2[] dotOrder = new Vector2[8]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(2, 0),
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(2, 1),
                new Vector2(3, 0),
                new Vector2(3, 1),
            };

            string binaryString = "";

            foreach (var dotPos in dotOrder)
            {
                // Create a binary string from the values in the array
                int val = (int)array[(int)dotPos.X, (int)dotPos.Y];
                binaryString = val.ToString() + binaryString;
            }

            int unicodeChar = 10240 + Convert.ToInt32(binaryString, 2);
            Console.WriteLine(unicodeChar);

            return Convert.ToChar(unicodeChar);
        }


        // Assume max is 255
        public static string ArrayToString(float[,] b)
        {
            string finalString = "";

            for (int i = 0; i < b.GetLength(0); i++)
            {
                string currentRow = "";

                for (int j = 0; j < b.GetLength(1); j++)
                {
                    currentRow += GetCharFromDensity(b[i, j]);
                }

                finalString += currentRow;
                finalString += "\n";
            }
            return finalString;
        }


        // Arrays must be the same dimensions!!
        public static string ArraysToString(params float[][,] b)
        {
            string finalString = "";

            for (int i = 0; i < b[0].GetLength(0); i++)
            {
                string currentRow = "";


                foreach (var array in b)
                {
                    for (int j = 0; j < b[0].GetLength(1); j++)
                    {
                        currentRow += GetCharFromDensity(array[i, j]);
                    }
                }


                finalString += currentRow;
                finalString += "\n";
            }
            return finalString;
        }

        static float ColorToGreyscale(Color c)
        {
            if (c.A == 0) return 0;

            //return (0.299f + c.R + 0.587f * c.G + 0.114f * c.B)/255;
            return ((c.R + c.G + c.B) / 3f) / 255;
        }

        static char GetCharFromDensity(double density)
        {


            string charShadeString = " .:-=+*#%@";


            //" 123456789";
            //" `.-':_,^=;><+!rc*/z?sLTv)J7(|Fi{C}fI31tlu[neoZ5Yxjya]2ESwqkP6h9d4VpOGbUAKXHm8RD#$Bg0MNWQ%&@"

            //" .:-=+*#%@";

            density =
                density < 0 ? 0 :
                density > 1 ? 1 :
                density;

            double index = Math.Round((charShadeString.Length - 1) * density);
            return charShadeString[(int)index];
        }

    }
}
