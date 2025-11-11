using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System.Text;

namespace SpriteConverter
{
    public class Program
    {
        private const int SpriteSize = 16;

        /// <summary>
        /// Defines the Alpha value threshold for determining an 'on' pixel (1).
        /// Pixels with an alpha value below this threshold are considered 'off' (0).
        /// </summary>
        private const byte AlphaThreshold = 128;

        public static void Main(string[] args)
        {
            Console.WriteLine("--- C# Binary Sprite Array Generator (ImageSharp) ---");

            if (args.Length == 0)
            {
                Console.WriteLine("Usage: dotnet run <path_to_16x16_image_file>");
                Console.WriteLine("Example: dotnet run ./pixil-frame-0.png");
                Console.ReadLine();
                return;
            }

            string imagePath = args[0];

            if (!File.Exists(imagePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: File not found at '{imagePath}'");
                Console.ResetColor();
                Console.ReadLine();
                return;
            }

            try
            {
                // Load the image using ImageSharp's methods
                // We use the Rgba32 format which is common for 8-bit sprite alpha checks
                using (var image = Image.Load<Rgba32>(imagePath))
                {
                    // Ensure the image is exactly 16x16 pixels
                    if (image.Width != SpriteSize || image.Height != SpriteSize)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: Image must be exactly {SpriteSize}x{SpriteSize} pixels.");
                        Console.WriteLine($"Image size is currently {image.Width}x{image.Height}.");
                        Console.ResetColor();
                        Console.ReadLine();
                        return;
                    }

                    var binaryLines = new List<string>();

                    // Process the image pixel by pixel using ImageSharp's convenient method
                    for (int y = 0; y < SpriteSize; y++)
                    {
                        var rowBinary = new StringBuilder("0b");
                        var rowSpan = image.DangerousGetPixelRowMemory(y).Span;

                        for (int x = 0; x < SpriteSize; x++)
                        {
                            Rgba32 pixel = rowSpan[x];

                            // Decision logic: A pixel is 'on' (1) if it is significantly non-transparent.
                            // We check the Alpha channel (A) of the Rgba32 pixel struct.
                            if (pixel.A >= AlphaThreshold)
                            {
                                rowBinary.Append('1');
                            }
                            else
                            {
                                rowBinary.Append('0');
                            }
                        }
                        binaryLines.Add(rowBinary.ToString());
                    }

                    OutputResult(Path.GetFileName(imagePath), binaryLines);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"An error occurred while processing the image: {ex.Message}");
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Formats and prints the resulting array definition to the console.
        /// </summary>
        private static void OutputResult(string fileName, List<string> binaryLines)
        {
            Console.WriteLine("\n--- Generated C# Binary Array ---");
            Console.WriteLine($"// Sprite generated from: {fileName}");
            Console.WriteLine($"// Use this in your C# code as a 16x16 sprite definition (ushort array).");
            Console.WriteLine("private readonly ushort[] SpriteData = new ushort[]");
            Console.WriteLine("{");

            for (int i = 0; i < binaryLines.Count; i++)
            {
                string line = binaryLines[i];
                // Format to match the requested style: 0b111...
                string formattedLine = $"    {line}";
                if (i < binaryLines.Count - 1)
                {
                    formattedLine += ",";
                }
                Console.WriteLine(formattedLine);
            }

            Console.WriteLine("};");
            Console.WriteLine("\n--- Conversion Complete ---");
            Console.ReadLine();
        }
    }
}