namespace aaa;
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

        Console.WriteLine("new");
        Console.WriteLine(K);
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
}


public struct DopplerResult
{
    public double Beta;        // total speed / c
    public double BetaRadial;  // radial component (+ = toward camera = blueshift)
    public double K;           // wavelength multiplier: observedLambda = emittedLambda * K
    public double Intensity;   // relative intensity multiplier (K^4 beaming)
}