//dotnet add package SixLabors.ImageSharp
namespace aaa;
using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Program
{
    static int ExportWidth = 200;
    static int ExportHeight = 200;
    static string outputPath = "output.png";
    static Image<Rgba32> image = new Image<Rgba32>(ExportWidth, ExportHeight);
    static void Main(string[] args)
    {
        for (int y = 0; y < ExportHeight; y++)
        {
            for (int x = 0; x < ExportWidth; x++)
            {
                image[x, y] = new Rgba32(
                    r: (byte)(x * 255 / ExportWidth),   // red gradient left→right
                    g: (byte)(y * 255 / ExportHeight),  // green gradient top→bottom
                    b: 128,
                    a: 255                         // fully opaque
                );
            }
        }

        image.SaveAsPng(outputPath);
        Console.WriteLine($"Saved to {outputPath}");
    }
}

class Particle
{
    public Vector3 prevPos;
    public Vector3 curPos;
    public Particle(Vector3 pos)
    {
        curPos = pos;
        prevPos = pos;
    }
}