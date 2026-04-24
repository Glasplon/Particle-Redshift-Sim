using System;

public static class WavelengthToColor
{
    // CIE 1931 color matching functions, sampled at 5nm intervals from 380–780nm
    // Each row is { wavelength, x-bar, y-bar, z-bar }
    // Source: CIE 15:2004 standard
    private static readonly double[,] CmfTable =
    {
        { 380, 0.001368, 0.000039, 0.006450 },
        { 385, 0.002236, 0.000064, 0.010550 },
        { 390, 0.004243, 0.000120, 0.020050 },
        { 395, 0.007650, 0.000217, 0.036210 },
        { 400, 0.014310, 0.000396, 0.067850 },
        { 405, 0.023190, 0.000640, 0.110200 },
        { 410, 0.043510, 0.001210, 0.207400 },
        { 415, 0.077630, 0.002180, 0.371300 },
        { 420, 0.134380, 0.004000, 0.645600 },
        { 425, 0.214770, 0.007300, 1.039050 },
        { 430, 0.283900, 0.011600, 1.385600 },
        { 435, 0.328500, 0.016840, 1.622960 },
        { 440, 0.348280, 0.023000, 1.747060 },
        { 445, 0.348060, 0.029800, 1.782600 },
        { 450, 0.336200, 0.038000, 1.772110 },
        { 455, 0.318700, 0.048000, 1.744100 },
        { 460, 0.290800, 0.060000, 1.669200 },
        { 465, 0.251100, 0.073900, 1.528100 },
        { 470, 0.195360, 0.090980, 1.287640 },
        { 475, 0.142100, 0.112600, 1.041900 },
        { 480, 0.095640, 0.139020, 0.812950 },
        { 485, 0.057950, 0.169300, 0.616200 },
        { 490, 0.032010, 0.208020, 0.465180 },
        { 495, 0.014700, 0.258600, 0.353300 },
        { 500, 0.004900, 0.323000, 0.272000 },
        { 505, 0.002400, 0.407300, 0.212300 },
        { 510, 0.009300, 0.503000, 0.158200 },
        { 515, 0.029100, 0.608200, 0.111700 },
        { 520, 0.063270, 0.710000, 0.078250 },
        { 525, 0.109600, 0.793200, 0.057250 },
        { 530, 0.165500, 0.862000, 0.042160 },
        { 535, 0.225750, 0.914850, 0.029840 },
        { 540, 0.290400, 0.954000, 0.020300 },
        { 545, 0.359700, 0.980300, 0.013400 },
        { 550, 0.433450, 0.994950, 0.008750 },
        { 555, 0.512050, 1.000000, 0.005750 },
        { 560, 0.594500, 0.995000, 0.003900 },
        { 565, 0.678400, 0.978600, 0.002750 },
        { 570, 0.762100, 0.952000, 0.002100 },
        { 575, 0.842500, 0.915400, 0.001800 },
        { 580, 0.916300, 0.870000, 0.001650 },
        { 585, 0.978600, 0.816300, 0.001400 },
        { 590, 1.026300, 0.757000, 0.001100 },
        { 595, 1.056700, 0.694900, 0.001000 },
        { 600, 1.062200, 0.631000, 0.000800 },
        { 605, 1.045600, 0.566800, 0.000600 },
        { 610, 1.002600, 0.503000, 0.000340 },
        { 615, 0.938400, 0.441200, 0.000240 },
        { 620, 0.854450, 0.381000, 0.000190 },
        { 625, 0.751400, 0.321000, 0.000100 },
        { 630, 0.642400, 0.265000, 0.000050 },
        { 635, 0.541900, 0.217000, 0.000030 },
        { 640, 0.447900, 0.175000, 0.000020 },
        { 645, 0.360800, 0.138200, 0.000010 },
        { 650, 0.283500, 0.107000, 0.000000 },
        { 655, 0.218700, 0.081600, 0.000000 },
        { 660, 0.164900, 0.061000, 0.000000 },
        { 665, 0.121200, 0.044580, 0.000000 },
        { 670, 0.087400, 0.032000, 0.000000 },
        { 675, 0.063600, 0.023200, 0.000000 },
        { 680, 0.046770, 0.017000, 0.000000 },
        { 685, 0.032900, 0.011920, 0.000000 },
        { 690, 0.022700, 0.008210, 0.000000 },
        { 695, 0.015840, 0.005723, 0.000000 },
        { 700, 0.011359, 0.004102, 0.000000 },
        { 705, 0.008111, 0.002929, 0.000000 },
        { 710, 0.005790, 0.002091, 0.000000 },
        { 715, 0.004109, 0.001484, 0.000000 },
        { 720, 0.002899, 0.001047, 0.000000 },
        { 725, 0.002049, 0.000740, 0.000000 },
        { 730, 0.001440, 0.000520, 0.000000 },
        { 735, 0.001000, 0.000361, 0.000000 },
        { 740, 0.000690, 0.000249, 0.000000 },
        { 745, 0.000476, 0.000172, 0.000000 },
        { 750, 0.000332, 0.000120, 0.000000 },
        { 755, 0.000235, 0.000085, 0.000000 },
        { 760, 0.000166, 0.000060, 0.000000 },
        { 765, 0.000117, 0.000042, 0.000000 },
        { 770, 0.000083, 0.000030, 0.000000 },
        { 775, 0.000059, 0.000021, 0.000000 },
        { 780, 0.000042, 0.000015, 0.000000 },
    };

    /// <summary>
    /// Converts a single wavelength in nm to an sRGB color (0–255 per channel).
    /// Input should be in the visible range ~380–780 nm.
    /// </summary>
    public static (int r, int g, int b) WavelengthToSRGB(double wavelengthNm)
    {
        var (x, y, z) = WavelengthToXYZ(wavelengthNm);
        var (lr, lg, lb) = XYZToLinearRGB(x, y, z);
        return LinearRGBToSRGB(lr, lg, lb);
    }

    /// <summary>
    /// Looks up CIE XYZ values for a wavelength using linear interpolation between table entries.
    /// </summary>
    public static (double x, double y, double z) WavelengthToXYZ(double wavelengthNm)
    {
        int rows = CmfTable.GetLength(0);

        // Clamp to table range
        if (wavelengthNm <= CmfTable[0, 0])
            return (CmfTable[0, 1], CmfTable[0, 2], CmfTable[0, 3]);
        if (wavelengthNm >= CmfTable[rows - 1, 0])
            return (CmfTable[rows - 1, 1], CmfTable[rows - 1, 2], CmfTable[rows - 1, 3]);

        // Find surrounding table entries and interpolate
        for (int i = 0; i < rows - 1; i++)
        {
            double wl0 = CmfTable[i,     0],  wl1 = CmfTable[i + 1, 0];
            if (wavelengthNm < wl0 || wavelengthNm > wl1) continue;

            double t = (wavelengthNm - wl0) / (wl1 - wl0);  // 0–1 blend factor

            double x = CmfTable[i, 1] + t * (CmfTable[i + 1, 1] - CmfTable[i, 1]);
            double y = CmfTable[i, 2] + t * (CmfTable[i + 1, 2] - CmfTable[i, 2]);
            double z = CmfTable[i, 3] + t * (CmfTable[i + 1, 3] - CmfTable[i, 3]);

            return (x, y, z);
        }

        return (0, 0, 0);
    }

    /// <summary>
    /// Converts CIE XYZ to linear RGB using the standard sRGB primaries matrix.
    /// Values may be negative or >1 for colors outside the sRGB gamut.
    /// </summary>
    public static (double r, double g, double b) XYZToLinearRGB(double x, double y, double z)
    {
        double r =  3.2406 * x - 1.5372 * y - 0.4986 * z;
        double g = -0.9689 * x + 1.8758 * y + 0.0415 * z;
        double b =  0.0557 * x - 0.2040 * y + 1.0570 * z;
        return (r, g, b);
    }

    /// <summary>
    /// Applies sRGB gamma correction and clamps to 0–255.
    /// </summary>
    public static (int r, int g, int b) LinearRGBToSRGB(double r, double g, double b)
    {
        static double GammaCorrect(double v)
        {
            v = Math.Max(0.0, v);  // clamp negatives (out-of-gamut)
            return v <= 0.0031308
                ? 12.92 * v
                : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;
        }

        int ri = (int)Math.Round(GammaCorrect(r) * 255.0);
        int gi = (int)Math.Round(GammaCorrect(g) * 255.0);
        int bi = (int)Math.Round(GammaCorrect(b) * 255.0);

        return (Math.Clamp(ri, 0, 255), Math.Clamp(gi, 0, 255), Math.Clamp(bi, 0, 255));
    }
}






public static class SpectrumToRgbOLD
{
    // CIE 1931 2° color matching functions, sampled at 5 nm intervals, 380–780 nm
    // Source: cvrl.org — these are the standard tabulated values
    private static readonly double[] CIE_WL = GenerateRange(380, 780, 5); // 81 entries
    private static readonly double[] CIE_X =
    {
        0.001368, 0.002236, 0.004243, 0.007650, 0.014310, 0.023190, 0.043510, 0.077630,
        0.134380, 0.214770, 0.283900, 0.328500, 0.348280, 0.348060, 0.336200, 0.318700,
        0.290800, 0.251100, 0.195360, 0.142100, 0.095640, 0.057950, 0.032010, 0.014700,
        0.004900, 0.002400, 0.009300, 0.029100, 0.063270, 0.109600, 0.165500, 0.225750,
        0.290400, 0.359700, 0.433450, 0.512050, 0.594500, 0.678400, 0.762100, 0.842500,
        0.916300, 0.978600, 1.026300, 1.056700, 1.062200, 1.045600, 1.002600, 0.938400,
        0.854450, 0.751400, 0.642400, 0.541900, 0.447900, 0.360800, 0.283500, 0.218700,
        0.164900, 0.121200, 0.087400, 0.063600, 0.046770, 0.032900, 0.022700, 0.015840,
        0.011359, 0.008111, 0.005790, 0.004109, 0.002899, 0.002049, 0.001440, 0.001000,
        0.000690, 0.000476, 0.000332, 0.000235, 0.000166, 0.000117, 0.000083, 0.000059,
        0.000042
    };
    private static readonly double[] CIE_Y =
    {
        0.000039, 0.000064, 0.000120, 0.000217, 0.000396, 0.000640, 0.001210, 0.002180,
        0.004000, 0.007300, 0.011600, 0.016840, 0.023000, 0.029800, 0.038000, 0.048000,
        0.060000, 0.073900, 0.090980, 0.112600, 0.139020, 0.169300, 0.208020, 0.258600,
        0.323000, 0.407300, 0.503000, 0.608200, 0.710000, 0.793200, 0.862000, 0.914850,
        0.954000, 0.980300, 0.994950, 1.000000, 0.995000, 0.978600, 0.952000, 0.915400,
        0.870000, 0.816300, 0.757000, 0.694900, 0.631000, 0.566800, 0.503000, 0.441200,
        0.381000, 0.321000, 0.265000, 0.217000, 0.175000, 0.138200, 0.107000, 0.081600,
        0.061000, 0.044580, 0.032000, 0.023200, 0.017000, 0.011920, 0.008210, 0.005723,
        0.004102, 0.002929, 0.002091, 0.001484, 0.001047, 0.000740, 0.000520, 0.000361,
        0.000249, 0.000172, 0.000120, 0.000085, 0.000060, 0.000042, 0.000030, 0.000021,
        0.000015
    };
    private static readonly double[] CIE_Z =
    {
        0.006450, 0.010550, 0.020050, 0.036210, 0.067850, 0.110200, 0.207400, 0.371300,
        0.645600, 1.039050, 1.385600, 1.622960, 1.747060, 1.782600, 1.772110, 1.744100,
        1.669200, 1.528100, 1.287640, 1.041900, 0.812950, 0.616200, 0.465180, 0.353300,
        0.272000, 0.212300, 0.158200, 0.111700, 0.078250, 0.057250, 0.042160, 0.029840,
        0.020300, 0.013400, 0.008750, 0.005750, 0.003900, 0.002750, 0.002100, 0.001800,
        0.001650, 0.001400, 0.001100, 0.001000, 0.000800, 0.000600, 0.000340, 0.000240,
        0.000190, 0.000100, 0.000050, 0.000030, 0.000020, 0.000010, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000, 0.000000,
        0.000000
    };

    private static double[] GenerateRange(double start, double end, double step)
    {
        int count = (int)((end - start) / step) + 1;
        double[] arr = new double[count];
        for (int i = 0; i < count; i++)
            arr[i] = start + i * step;
        return arr;
    }

    /// <summary>
    /// Linearly interpolates a spectrum (defined at arbitrary wavelengths)
    /// to a query wavelength. Returns 0 outside the spectrum's range.
    /// </summary>
    private static double SampleSpectrum(double[] wlNm, double[] values, double queryNm)
    {
        if (queryNm < wlNm[0] || queryNm > wlNm[^1]) return 0.0;

        for (int i = 0; i < wlNm.Length - 1; i++)
        {
            if (queryNm <= wlNm[i + 1])
            {
                double t = (queryNm - wlNm[i]) / (wlNm[i + 1] - wlNm[i]);
                return values[i] + t * (values[i + 1] - values[i]);
            }
        }
        return values[^1];
    }

    /// <summary>
    /// Converts a spectrum to an sRGB byte triple (0-255).
    /// Pass normalised spectrum for color only.
    /// Pass raw spectrum + distanceM to get physically scaled brightness (1/d² falloff).
    /// </summary>
    /// <param name="wlNm">Wavelength array in nm</param>
    /// <param name="spectrum">Spectral values (normalised or raw radiance)</param>
    /// <param name="distanceM">Distance to camera in metres. null = no distance falloff</param>
    /// <param name="brightnessScale">Manual brightness multiplier (tune this for your scene)</param>
    public static (byte r, byte g, byte b) SpectrumToSrgb(
        double[] wlNm,
        double[] spectrum,
        double?  distanceM     = null,
        double   brightnessScale = 1.0)
    {
        // --- Step 1: integrate against CIE color matching functions → XYZ ---
        double X = 0, Y = 0, Z = 0;
        for (int i = 0; i < CIE_WL.Length; i++)
        {
            double s = SampleSpectrum(wlNm, spectrum, CIE_WL[i]);
            X += s * CIE_X[i];
            Y += s * CIE_Y[i];
            Z += s * CIE_Z[i];
        }

        // --- Step 2: distance falloff on brightness (1/d²), color unaffected ---
        if (distanceM.HasValue && distanceM.Value > 0)
        {
            double falloff = 1.0 / (distanceM.Value * distanceM.Value);
            X *= falloff;
            Y *= falloff;
            Z *= falloff;
        }

        X *= brightnessScale;
        Y *= brightnessScale;
        Z *= brightnessScale;

        // --- Step 3: XYZ → linear sRGB (IEC 61966-2-1 matrix) ---
        double linR =  3.2406 * X - 1.5372 * Y - 0.4986 * Z;
        double linG = -0.9689 * X + 1.8758 * Y + 0.0415 * Z;
        double linB =  0.0557 * X - 0.2040 * Y + 1.0570 * Z;

        // Clamp negatives (can occur at spectrum edges — outside gamut)
        linR = Math.Max(0, linR);
        linG = Math.Max(0, linG);
        linB = Math.Max(0, linB);

        // --- Step 4: tonemap (Reinhard) so HDR values don't just clip to white ---
        linR = linR / (1.0 + linR);
        linG = linG / (1.0 + linG);
        linB = linB / (1.0 + linB);

        // --- Step 5: gamma encode (linear → sRGB, γ ≈ 2.2) ---
        static double GammaEncode(double v)
            => v <= 0.0031308 ? 12.92 * v : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;

        byte R = (byte)Math.Round(GammaEncode(linR) * 255);
        byte G = (byte)Math.Round(GammaEncode(linG) * 255);
        byte B = (byte)Math.Round(GammaEncode(linB) * 255);

        return (R, G, B);
    }
}
