//dotnet add package SixLabors.ImageSharp
namespace aaa;
using System.Numerics;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

class Program
{
    static Vector3 cameraPos = new Vector3(0,0.1f,1.2f);
    static int ExportWidth = 200;
    static int ExportHeight = 200;
    static string outputPath = "testing6.png";
    static Image<Rgba32> image = new Image<Rgba32>(ExportWidth, ExportHeight);
    static double[,] zDepth = new double[ExportWidth,ExportHeight];
    static Particle[] particles;

    static double rpm = 1_700_000_000;
    static double radiansPerNanosecond = (rpm * (2.0 * Math.PI / 60.0) * 1e-9)*4;
    static double timeStep = 0; // time in ns
    //static double radiansPerNanosecond = 0;
    //static double radiansPerNanosecond = 0.1781*5;
    static void Main(string[] args)
    {
        /*particles = new Particle[1000];
        for (int i = 0; i < particles.Length; i++)
        {
            particles[i] = new Particle(new Vector3Double((int)(i/100)-5,((int)(i/10)%10)-5,i%10-5));
        }
        for (int y = 0; y < ExportHeight; y++)
        {
            for (int x = 0; x < ExportWidth; x++)
            {
                image[x, y] = new Rgba32(
                    r: 128,
                    g: 128,
                    b: 128,
                    a: 255
                );
            }
        }*/

        Random rand = new Random();


        particles = new Particle[20000];
        /*for (int i = 0; i < particles.Length; i+=10)
        {
            for (int y = 0; y < 10; y++)
            {
                particles[i+y] = new Particle(new Vector3Double((i/2000.0)-0.25,y/200.0,0));
            }
        }*/
        for (int i = 0; i < particles.Length; i++)
        {
                double angle = rand.NextDouble() * 2 * Math.PI;
                double radius = Math.Sqrt(rand.NextDouble()) * (0.1/4f);

                double xTMP = 0 + radius * Math.Cos(angle);
                double yTMP = 0 + radius * Math.Sin(angle);
                //Console.WriteLine(xTMP);
                //Console.WriteLine(yTMP);
                particles[i] = new Particle(new Vector3Double((i/(particles.Length*2f))-0.25,xTMP,yTMP));
                //particles[i].prevPos = Helper.rotateY(particles[i].prevPos, 1.5f);
                //particles[i].curPos = particles[i].prevPos;
        }
        Console.WriteLine("particles Done");
        //return;
        
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
                zDepth[x,y] = 1000000; // z depth far distance
            }
        }

        float focalLength = 400f;
        float cameraDistance = 40f;
        if(args.Length >= 1 && args[0] != "w")
        {
            if (float.TryParse(args[0], out float resultTIMESTAMP)) {
                //outputPath = $"./outputData/{(int)resultTIMESTAMP:D4}.png";
                outputPath = $"./outputData/"+resultTIMESTAMP+".png";
                //timeStep = resultTIMESTAMP;
                if(args.Length == 2 && args[1] == "s")
                {
                    radiansPerNanosecond/=10;
                    timeStep=resultTIMESTAMP*1;
                    Console.WriteLine(timeStep);
                } else
                {
                    timeStep=resultTIMESTAMP*0.1f;;
                    Console.WriteLine(timeStep);
                }
            } else {
                Console.WriteLine("timestamp not recognized, quitting.");
                return;
            }
        }

        if(args.Length == 1 && args[0] == "w")
        {
            var (wl, spec) = BlackbodySpectrum.GetSpectrum(tempK: 4000.0);
            double[] normSpec = BlackbodySpectrum.Normalise(spec);
            //var (wl, normSpec) = BlackbodySpectrum.GetHydrogenSpectrumNormOld();
            for (int k = 0; k < wl.Length; k++)
            {
                //wl[k] *= 0.12160563989462705;
            }

            //Console.WriteLine($"Peak wavelength estimate: {BlackbodySpectrum.PeakWavelengthNm(1200):F0} nm");
            for (int i = 0; i < wl.Length; i++)
            {
                if (i >= 0 && i < ExportWidth)
                {
                    double[] testWl = [wl[i],wl[i]+1];
                    double[] testSpec = [1,0];
                    //var (r, g, b) = WavelengthToColor.WavelengthToSRGB(wl[i]);
                    double[] wavelengths = { 400, 450, 500, 550, 600, 650, 700 };
                    double[] power       = { 0.1, 0.5, 1.0, 0.8, 0.3, 0.1, 0.0 };


                    var (r, g, b) = SpectrumToRGB.Convert(testWl,testSpec);
                    Console.WriteLine(r);
                    Console.WriteLine(g);
                    Console.WriteLine(b);
                    Console.WriteLine(wl[i]);
                    Console.WriteLine(normSpec[i]);
                    for (int j = 0; j < normSpec[i]*100f; j++)
                    {
                        if (ExportHeight-j >= 0 && ExportHeight-j < ExportHeight)
                        {
                            image[(int)i, ExportHeight-j] = new Rgba32(
                                r: (byte)r, 
                                g: (byte)g, 
                                b: (byte)b,
                                a: 255                  
                            );
                        }
                    }
                }
            }
            for (int i = 0; i < wl.Length; i += 40) {  // print every 40th sample
                //Console.WriteLine($"  {wl[i],6:F1} nm  →  {normSpec[i]:F6}");
                //Console.WriteLine($"{wl[i]} nm  →  {spec[i]}");
            }
                
        } else
        {

            // Color only (no distance)
            //var (r, g, b) = SpectrumToRgb.SpectrumToSrgb(wl, norm);

            // With distance falloff + brightness tuning

            for (int i = 0; i < particles.Length; i++)
            {
                // Perspective (what to add)
                //float z = (float)particles[i].curPos.Z + cameraDistance; // push world in front of camera
                //float screenX = ExportWidth/2 + ((float)particles[i].curPos.X / z) * focalLength;
                //float screenY = ExportHeight/2 + ((float)particles[i].curPos.Y / z) * focalLength;

                //particles[i].curPos = Helper.rotateY(particles[i].prevPos, radiansPerNanosecond);
                particles[i].prevPos = Helper.rotateY(particles[i].startPos, timeStep*radiansPerNanosecond);
                particles[i].curPos = Helper.rotateY(particles[i].prevPos, radiansPerNanosecond);

                float dxBeforeSOL = (float)particles[i].curPos.X - cameraPos.X;
                float dyBeforeSOL = (float)particles[i].curPos.Y - cameraPos.Y;
                float dzBeforeSOL = (float)particles[i].curPos.Z - cameraPos.Z;
                float distBeforeSOL = MathF.Sqrt(dxBeforeSOL*dxBeforeSOL + dyBeforeSOL*dyBeforeSOL + dzBeforeSOL*dzBeforeSOL);

                double lightTravelNs = distBeforeSOL / 0.299792458; // how many nanoseconds light takes to reach camera

                double emitTimeStep = timeStep - lightTravelNs; // what "time" the particle was at when it emitted

                particles[i].prevPos = Helper.rotateY(particles[i].startPos, emitTimeStep * radiansPerNanosecond);
                particles[i].curPos  = Helper.rotateY(particles[i].prevPos, radiansPerNanosecond);

                Vector3 camSpaceVertex = new Vector3((float)particles[i].curPos.X,(float)particles[i].curPos.Y,(float)particles[i].curPos.Z) - cameraPos;
                float z = camSpaceVertex.Z; // this is your depth
                float screenX = ExportWidth/2 + (camSpaceVertex.X / z) * focalLength;
                float screenY = ExportHeight/2 + (camSpaceVertex.Y / z) * focalLength;

                if (screenX >= 0 && screenX < ExportWidth && screenY >= 0 && screenY < ExportHeight)
                {

                    //Console.WriteLine("info:");
                    //Console.WriteLine(particles[i].startPos.Z);
                    //Console.WriteLine(particles[i].prevPos.Z);
                    //Console.WriteLine(particles[i].curPos.Z);
                    

                    var result = Helper.Calculate(particles[i].prevPos, particles[i].curPos, new Vector3Double(cameraPos.X,cameraPos.Y,cameraPos.Z));

                    //double observedWavelength = emittedWavelength * result.K;
                    //double observedIntensity  = baseIntensity * result.Intensity;

                    //Console.WriteLine((particles[i].curPos.X+5) * 300);
                    //var (wl, spec) = BlackbodySpectrum.GetSpectrum(tempK: ((particles[i].curPos.X+5)*100)+1000);

                    
                    Vector3Double delta = new Vector3Double(
                        particles[i].curPos.X - particles[i].prevPos.X,
                        particles[i].curPos.Y - particles[i].prevPos.Y,
                        particles[i].curPos.Z - particles[i].prevPos.Z);

                    double speedMperNs = Math.Sqrt(delta.X*delta.X + delta.Y*delta.Y + delta.Z*delta.Z);
                    double speedMperS  = speedMperNs * 1e9;
                    double speedFracC  = speedMperNs / 0.299792458;

                    //Console.WriteLine($"Speed: {speedMperS:F0} m/s  |  {speedMperNs:F6} m/ns  |  {speedFracC:F4}c");
                    


                    //var (wl, spec) = BlackbodySpectrum.GetSpectrum(tempK: 4000);
                    var (wl, spec) = BlackbodySpectrum.GetHydrogenSpectrum();
                    double[] normSpec = BlackbodySpectrum.Normalise(spec);
                    //double[] norm = BlackbodySpectrum.Normalise(spec);
                    int highestIndex = -1000;
                    double highestCurr = 0;
                    for (int k = 0; k < wl.Length; k++)
                    {
                        wl[k] *= (result.K);
                        spec[k] *= (result.Intensity / result.K);
                        //if(spec[k] > highestCurr)
                        //{
                            //highestCurr = spec[k];
                            //highestIndex = k;
                        //}
                    }

                    //Console.WriteLine($"Spectrum range after shift: {wl[0]:F1} - {wl[^1]:F1} nm");

                    float dx = (float)particles[i].curPos.X - cameraPos.X;
                    float dy = (float)particles[i].curPos.Y - cameraPos.Y;
                    float dz = (float)particles[i].curPos.Z - cameraPos.Z;
                    float dist = MathF.Sqrt(dx*dx + dy*dy + dz*dz);

                    //Vector3Double particlePos = Helper.rotateX(baseParticlePos, currentAngle);

                    //var (r, g, b) = SpectrumToRgb.SpectrumToSrgb(wl, spec, distanceM: dist, brightnessScale: 1e-11);
                    double[] wavelengths = { 400, 450, 500, 550, 600, 650, 700 };
                    double[] power       = { 0.1, 0.5, 1.0, 0.8, 0.3, 0.1, 0.0 };

                    //var (r, g, b) = SpectrumToRGB.Convert(wavelengths,power);
                    //var (r, g, b) = SpectrumToRGB.Convert(wl,spec);
                    //var (r, g, b) = SpectrumToRgbOLD.SpectrumToSrgb(wl, spec, distanceM: dist, brightnessScale: 1e-9);
                    //var (r, g, b) = SpectrumToRgbOLD.SpectrumToSrgb(wl, spec, distanceM: dist, brightnessScale: 2e-14);

                    var (r, g, b) = SpectrumToRgbOLD.SpectrumToSrgb(wl, spec, distanceM: dist, brightnessScale: 4e-11);
                    //var (r, g, b) = SpectrumToRgbOLD.SpectrumToSrgb(wl, spec, distanceM: dist, brightnessScale: 1e-16);

                    //Console.WriteLine(rNew);
                    //Console.WriteLine(gNew);
                    //Console.WriteLine(bNew);zDepth

                    Vector3 lookAt = new Vector3(0,0.1f,0);
                    Vector3 forward = Vector3.Normalize(new Vector3(
                        lookAt.X - cameraPos.X,
                        lookAt.Y - cameraPos.Y,
                        lookAt.Z - cameraPos.Z));

                    float depth = dx * forward.X + dy * forward.Y + dz * forward.Z;
                    //Console.WriteLine(depth);
                    if (zDepth[(int)screenX, (int)screenY] > depth)
                    {
                        zDepth[(int)screenX, (int)screenY] = depth;

                    //var (r, g, b) = WavelengthToColor.WavelengthToSRGB(wl[highestIndex]);
                    //Console.WriteLine("info:");
                    //Console.WriteLine(wl[highestIndex]);
                        /*image[(int)screenX, (int)screenY] = new Rgba32(
                            r: (byte)r, 
                            g: (byte)g, 
                            b: (byte)b,
                            a: 255                  
                        );*/
                    }
                    Helper.DrawPixelAA(image,screenX,screenY,r,g,b,1f); // no need for z depth if addetive
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
    public Vector3Double startPos;
    public Vector3Double prevPos;
    public Vector3Double curPos;
    public Particle(Vector3Double pos)
    {
        curPos = pos;
        prevPos = pos;
        startPos = pos;
    }
}

public class Vector3Double
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

    public double Length()
    {
        double length = Math.Sqrt(X * X + Y * Y + Z * Z);
        return length;
    }
}