namespace aaa;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
public static class Helper
{
    public const double C = 0.299792458; //speed of light in meter s per nanosecond.
    public static Vector3Double rotateX(Vector3Double v, double Radangle) {
        double c = Math.Cos(Radangle);
        double s = Math.Sin(Radangle);

        return new Vector3Double(
            v.X,
            v.Y * c - v.Z * s,
            v.Y * s + v.Z * c
        );
    }

    public static Vector3Double rotateY(Vector3Double v, double Radangle) {
        double c = Math.Cos(Radangle);
        double s = Math.Sin(Radangle);

        return new Vector3Double(
            v.X * c + v.Z * s,
            v.Y,
            -v.X * s + v.Z * c
        );
    }

    public static Vector3Double rotateZ(Vector3Double v, double Radangle) {
        double c = Math.Cos(Radangle);
        double s = Math.Sin(Radangle);

        return new Vector3Double(
            v.X * c - v.Y * s,
            v.X * s + v.Y * c,
            v.Z
        );
    }

    /// <param name="particlePos">Current particle position (meters)</param>
    /// <param name="particlePosPlusOne">Particle position 1 nanosecond later (meters)</param>
    /// <param name="cameraPos">Camera/observer position (meters)</param>
    public static DopplerResult Calculate(Vector3Double particlePos, Vector3Double particlePosPlusOne, Vector3Double cameraPos)
    {
        // --- Velocity vector (meters per nanosecond) ---
        Vector3Double velocity = new Vector3Double(particlePosPlusOne.X - particlePos.X,particlePosPlusOne.Y - particlePos.Y,particlePosPlusOne.Z - particlePos.Z);
        
        // --- Total speed and beta ---
        double speed = velocity.Length();
        double beta = speed / C;

        // --- Unit vector FROM particle TOWARD camera ---
        Vector3Double toCamera = new Vector3Double(cameraPos.X - particlePos.X,cameraPos.Y - particlePos.Y,cameraPos.Z - particlePos.Z);
        double length = Math.Sqrt(toCamera.X * toCamera.X + toCamera.Y * toCamera.Y + toCamera.Z * toCamera.Z);
        toCamera = new Vector3Double(toCamera.X / length, toCamera.Y / length, toCamera.Z / length);

        // --- Radial velocity: project velocity onto the toward-camera direction ---
        // Positive = moving toward camera = blueshift
        double radialSpeed = velocity.X * toCamera.X + velocity.Y * toCamera.Y + velocity.Z * toCamera.Z;
        double betaRadial = radialSpeed / C;

        // --- K factor (the full relativistic Doppler multiplier) ---
        // observedWavelength = emittedWavelength * K
        // K < 1 = blueshift (approaching), K > 1 = redshift (receding)
        double gamma = 1.0 / Math.Sqrt(1.0 - beta * beta);
        double K = 1.0 / (gamma * (1.0 + betaRadial));

        // --- Relativistic beaming: intensity scales as K^4 ---
        double intensity = Math.Pow(K, 4.0);

        //Console.WriteLine("new");
        //Console.WriteLine(K);
        //Console.WriteLine(particlePos.X+", "+particlePos.Y+", "+particlePos.Z);
        //Console.WriteLine(particlePosPlusOne.X+", "+particlePosPlusOne.Y+", "+particlePosPlusOne.Z);
        //Console.WriteLine(cameraPos.X);

        return new DopplerResult
        {
            Beta = beta,
            BetaRadial = betaRadial,
            K = K,
            Intensity = intensity
        };
    }

    static public void DrawPixelAA(Image<Rgba32> image, float px, float py, float r, float g, float b, float alpha = 1f)
    {
        int x0 = (int)MathF.Floor(px);
        int y0 = (int)MathF.Floor(py);
        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float fx = px - x0; // fractional part, 0..1
        float fy = py - y0;

        // weights for each of the 4 surrounding pixels
        float w00 = (1 - fx) * (1 - fy);
        float w10 = fx       * (1 - fy);
        float w01 = (1 - fx) * fy;
        float w11 = fx       * fy;

        BlendPixel(image, x0, y0, r, g, b, alpha * w00);
        BlendPixel(image, x1, y0, r, g, b, alpha * w10);
        BlendPixel(image, x0, y1, r, g, b, alpha * w01);
        BlendPixel(image, x1, y1, r, g, b, alpha * w11);
    }
    static void BlendPixel(Image<Rgba32> image, int x, int y, float r, float g, float b, float w)
    {
        if (x < 0 || y < 0 || x >= image.Width || y >= image.Height) return;

        Rgba32 existing = image[x, y];
        image[x, y] = new Rgba32(
            r: (byte)Math.Clamp(existing.R + r * w, 0, 255),
            g: (byte)Math.Clamp(existing.G + g * w, 0, 255),
            b: (byte)Math.Clamp(existing.B + b * w, 0, 255),
            a: 255
        );
    }
}


public struct DopplerResult
{
    public double Beta;        // total speed / c
    public double BetaRadial;  // radial component (+ = toward camera = blueshift)
    public double K;           // wavelength multiplier: observedLambda = emittedLambda * K
    public double Intensity;   // relative intensity multiplier (K^4 beaming)
}