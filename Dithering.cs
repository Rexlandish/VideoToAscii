using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoToAscii
{
    internal class Dithering
    {


        class PixelOffset
        {

            public int xOffset;
            public int yOffset;
            public float diffusionAmount;

            public PixelOffset(int xOffset, int yOffset, float diffusionAmount)
            {
                this.xOffset = xOffset;
                this.yOffset = yOffset;
                this.diffusionAmount = diffusionAmount;
            }
        }

        // x+ : Right
        // y+ : Down
        static List<PixelOffset> floydSteinbergDithering = new List<PixelOffset>()
        {
            new PixelOffset( 1,  0,  7/16f),
            new PixelOffset( 1,  1,  1/16f),
            new PixelOffset( 0,  1,  5/16f),
            new PixelOffset(-1,  1,  3/16f),
        };

        static List<PixelOffset> jarvisJudiceNinkeDithering = new List<PixelOffset>()
        {
            //7
            new PixelOffset( 1,  0,  7/48f),
            new PixelOffset( 0,  1,  7/48f),

            //5
            new PixelOffset( 2,  0,  5/48f),
            new PixelOffset( 1,  1,  5/48f),
            new PixelOffset( 0,  2,  5/48f),
            new PixelOffset( -1,  1,  5/48f),

            //3
            new PixelOffset( 2,  1,  3/48f),
            new PixelOffset( 1,  2,  3/48f),
            new PixelOffset( -1,  2,  3/48f),
            new PixelOffset( -2,  1,  3/48f),

            //1
            new PixelOffset( 2,  2,  1/48f),
            new PixelOffset( -2,  2,  1/48f)

        };

        public static float[,] Dither(float[,] b)
        {

            int width = b.GetLength(1);
            int height = b.GetLength(0);
            float[,] ditheredArray = new float[height, width];
            float[,] errorDiffusion = new float[height, width];

            float maxVal = 1f;
            float threshold = maxVal / 2;

            // Iterate through all pixels
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Combine the current working pixel with the error diffusion
                    float errorDiff = errorDiffusion[y, x];
                    float workingPixel = b[y, x];
                    float currentVal = workingPixel + errorDiff;

                    // Is the current pixel (with the diffused error) closer to the maximum value or zero? Set as such.
                    float setValue = currentVal >= threshold ?
                        maxVal
                        : 0;

                    ditheredArray[y, x] = setValue;

                    // Diffuse error
                    float currentErrorAmount = setValue - currentVal; //workingVal; // 127
                    foreach (var pixelOffset in jarvisJudiceNinkeDithering)
                    {
                        int targetPixelx = x + pixelOffset.xOffset;
                        int targetPixely = y + pixelOffset.yOffset;

                        if (
                            targetPixelx >= width ||
                            targetPixely >= height ||
                            targetPixelx <= 0 ||
                            targetPixely <= 0
                            )
                            continue;

                        errorDiffusion[targetPixely, targetPixelx] += -currentErrorAmount * pixelOffset.diffusionAmount;
                        // Normalize error diffusion
                        /*
                        float currentErrorDiffusion = errorDiffusion[targetPixely, targetPixelx];
                        
                        if (currentErrorDiffusion < -threshold)
                        {
                            errorDiffusion[targetPixely, targetPixelx] = -threshold;
                        }
                        else if (currentErrorDiffusion > threshold)
                        {
                            errorDiffusion[targetPixely, targetPixelx] = threshold;
                        }
                        */
                        
                    }


                    // --- DEBUG ---
                    if (false)
                    {

                        // Show each of the 2D Arrays

                        float[,] bDisplay = (float[,])b.Clone();
                        bDisplay[y, x] = 0;

                        float[,] ditherDisplay = (float[,])ditheredArray.Clone();
                        ditherDisplay[y, x] = 1;

                        float[,] errorDisplay = (float[,])errorDiffusion.Clone();
                        errorDisplay[y, x] = 1;

                        Console.SetCursorPosition(0, 0);

                        if (x == 0)
                        {
                            /*
                            Console.WriteLine("---------------Dithered------------------");
                            Console.WriteLine($"{workingPixel} + {errorDiff}      ");
                            Console.WriteLine($"Current error: {errorAmount}      ");
                            //Console.WriteLine(ArrayToString(ditheredArray));
                            Console.WriteLine(ArrayToString(ditherDisplay));
                            Console.WriteLine("---------Error Diffusion Amount----------");
                            //Console.WriteLine(ArrayToString(errorDiffusion));
                            Console.WriteLine(ArrayToString(errorDisplay));
                            Console.WriteLine("---------------------------------");
                            //Console.ReadLine();
                            */

                            Console.WriteLine(Utility.ArraysToString(ditherDisplay, errorDisplay));
                        }

                    }



                }
            }

            return ditheredArray;
        }
    }
}
