//dotnet add package SixLabors.ImageSharp
namespace aaa;
using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Program
{
    static int ExportWidth = 500;
    static int ExportHeight = 150;
    static string outputPath = "spectrumTest.png";
    static Image<Rgba32> image = new Image<Rgba32>(ExportWidth, ExportHeight);
    static Particle[] particles;
    static void Main(string[] args)
    {
        particles = new Particle[1000];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new Particle(new Vector3Double((int)(i/100)-5,((int)(i/10)%10)-5,i%10-5));
        }
        for (int y = 0; y < ExportHeight; y++)
        {
            for (int x = 0; x < ExportWidth; x++)
            {
                image[x, y] = new Rgba32(
                    r: 0,
                    g: 0,
                    b: 0,
                    a: 255
                );
            }
        }

        float focalLength = 400f;
        float cameraDistance = 40f;

        if(args.Length == 1)
        {
            var (wl, spec) = BlackbodySpectrum.GetSpectrum(tempK: 1200.0);
            double[] normSpec = BlackbodySpectrum.Normalise(spec);

            Console.WriteLine($"Peak wavelength estimate: {BlackbodySpectrum.PeakWavelengthNm(1200):F0} nm");
            for (int i = 0; i < wl.Length; i++)
            {
                if (i >= 0 && i < ExportWidth)
                {
                    for (int j = 0; j < normSpec[i]*100f; j++)
                    {
                        if (ExportHeight-j >= 0 && ExportHeight-j < ExportHeight)
                        {
                            image[(int)i, ExportHeight-j] = new Rgba32(
                                r: (byte)(255), 
                                g: (byte)(255), 
                                b: 128,
                                a: 255                  
                            );
                        }
                    }
                }
            }
            for (int i = 0; i < wl.Length; i += 40) {  // print every 40th sample
                Console.WriteLine($"  {wl[i],6:F1} nm  →  {normSpec[i]:F6}");
                Console.WriteLine($"{wl[i]} nm  →  {spec[i]}");
            }
                
        } else
        {
            for (int i = 0; i < particles.Length; i++)
            {
                // Perspective (what to add)
                float z = (float)particles[i].curPos.Z + cameraDistance; // push world in front of camera
                float screenX = ExportWidth/2 + ((float)particles[i].curPos.X / z) * focalLength;
                float screenY = ExportHeight/2 + ((float)particles[i].curPos.Y / z) * focalLength;
                if (screenX >= 0 && screenX < ExportWidth && screenY >= 0 && screenY < ExportHeight)
                {
                    image[(int)screenX, (int)screenY] = new Rgba32(
                        r: (byte)(255), 
                        g: (byte)(255), 
                        b: 128,
                        a: 255                  
                    );
                }
            }
        }
        /*for (int y = 0; y < ExportHeight; y++)
        {
            for (int x = 0; x < ExportWidth; x+=2)
            {
                image[x, y] = new Rgba32(
                    r: (byte)(x * 255 / ExportWidth),   // red gradient left→right
                    g: (byte)(y * 255 / ExportHeight),  // green gradient top→bottom
                    b: 128,
                    a: 255                         // fully opaque
                );
            }
        }*/

        image.SaveAsPng(outputPath);
        Console.WriteLine($"Saved to {outputPath}");

    }
}

class Particle
{
    public Vector3Double prevPos;
    public Vector3Double curPos;
    public Particle(Vector3Double pos)
    {
        curPos = pos;
        prevPos = pos;
    }
}

class Vector3Double
{
    public double X;
    public double Y;
    public double Z;
    public Vector3Double(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}