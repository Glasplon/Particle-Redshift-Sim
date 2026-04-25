using System;
namespace aaa;

public static class BlackbodySpectrum
{
    // Physical constants
    private const double H  = 6.62607015e-34;  // Planck's constant (J·s)
    private const double C  = 2.99792458e8;    // Speed of light (m/s)
    private const double KB = 1.380649e-23;    // Boltzmann constant (J/K)

    private const double IRON_EMISSIVITY = 0.85;

    /// <summary>
    /// Computes the Planck spectral radiance for hot iron at a given temperature.
    /// Returns an array where index i corresponds to wavelength (wlStartNm + i * step).
    /// Units: W · sr⁻¹ · m⁻² · m⁻¹
    /// </summary>
    /// <param name="tempK">Temperature in Kelvin</param>
    /// <param name="wlStartNm">Start wavelength in nanometres (default: 380 visible)</param>
    /// <param name="wlEndNm">End wavelength in nanometres (default: 780 visible)</param>
    /// <param name="numSamples">Number of wavelength samples (default: 401 → 1 nm steps)</param>
    /// <param name="emissivity">Surface emissivity, 0–1 (default: 0.85 for rough iron)</param>
    /// <returns>
    ///   wavelengthsNm: wavelength at each index (nm)
    ///   radiance:      spectral radiance at each index
    /// </returns>
    public static (double[] wavelengthsNm, double[] radiance) GetSpectrum(
        double tempK,
        double wlStartNm  = 380.0,
        //double wlEndNm    = 780.0, //visible
        double wlEndNm    = 2415, //well into infrared.
        int    numSamples = 401,
        double emissivity = IRON_EMISSIVITY)
    {
        if (tempK <= 0)
            throw new ArgumentOutOfRangeException(nameof(tempK), "Temperature must be > 0 K.");
        if (numSamples < 2)
            throw new ArgumentOutOfRangeException(nameof(numSamples), "Need at least 2 samples.");

        double[] wavelengthsNm = new double[numSamples];
        double[] radiance      = new double[numSamples];

        double step = (wlEndNm - wlStartNm) / (numSamples - 1);

        for (int i = 0; i < numSamples; i++)
        {
            double wlNm = wlStartNm + i * step;
            double wlM  = wlNm * 1e-9;  // convert nm → metres

            double exponent = (H * C) / (wlM * KB * tempK);
            double spectral = (2.0 * H * C * C) / (Math.Pow(wlM, 5) * (Math.Exp(exponent) - 1.0));

            wavelengthsNm[i] = wlNm;
            radiance[i]      = spectral * emissivity;
        }

        return (wavelengthsNm, radiance);
    }

    /// <summary>
    /// Normalises an array to the range [0, 1] by dividing by its maximum value.
    /// Useful for feeding into a renderer without worrying about raw watt values.
    /// </summary>
    public static double[] Normalise(double[] values)
    {
        double max = 0.0;
        foreach (double v in values)
            if (v > max) max = v;

        if (max == 0.0) return new double[values.Length];

        double[] result = new double[values.Length];
        for (int i = 0; i < values.Length; i++)
            result[i] = values[i] / max;

        return result;
    }

    public static (double[] wavelengthsNm, double[] radiance) GetHydrogenSpectrum(
        double electronsInUpperLevel = 1e20,   // tune this for your scene brightness
        double wlStartNm = 380.0,
        double wlEndNm   = 1000.0,
        int    numSamples = 401)
    {
        // (wavelength nm, Einstein A coefficient s^-1)
        var lines = new (double wl, double A)[]
        {
            (656.28, 4.410e7),
            (486.13, 1.672e7),
            (434.05, 6.908e6),
            (410.17, 2.699e6),
            (397.01, 1.388e6),
        };

        double[] wavelengthsNm = new double[numSamples];
        double[] radiance      = new double[numSamples];

        double step = (wlEndNm - wlStartNm) / (numSamples - 1);
        for (int i = 0; i < numSamples; i++)
            wavelengthsNm[i] = wlStartNm + i * step;

        double sigma = 1.0; // nm, spike width

        foreach (var (wl, A) in lines)
        {
            // Photon energy in joules
            double wlM      = wl * 1e-9;
            double ePhoton  = (H * C) / wlM;

            // Power of this line in watts
            double power = A * electronsInUpperLevel * ePhoton;

            // Distribute into Gaussian spike
            // power is in W, we spread it over the spike so it integrates to `power`
            double norm = power / (sigma * Math.Sqrt(2.0 * Math.PI));

            for (int i = 0; i < numSamples; i++)
            {
                double diff = wavelengthsNm[i] - wl;
                radiance[i] += norm * Math.Exp(-(diff * diff) / (2.0 * sigma * sigma));
            }
        }

        return (wavelengthsNm, radiance);
    }

    public static (double[] wavelengthsNm, double[] radiance) GetHydrogenSpectrumNormOld(
        double wlStartNm = 380.0,
        double wlEndNm   = 1000.0,
        int    numSamples = 401)
    {
        // Balmer series lines: (wavelength nm, relative intensity)
        var lines = new (double wl, double strength)[]
        {
            (656.28, 1.000),   // H-alpha
            (486.13, 0.360),   // H-beta
            (434.05, 0.170),   // H-gamma
            (410.17, 0.090),   // H-delta
            (397.01, 0.040),   // H-epsilon
        };

        double[] wavelengthsNm = new double[numSamples];
        double[] radiance      = new double[numSamples];

        double step = (wlEndNm - wlStartNm) / (numSamples - 1);

        for (int i = 0; i < numSamples; i++)
            wavelengthsNm[i] = wlStartNm + i * step;

        // Each line is a Gaussian spike, width (sigma) ~1nm
        // Real spectral lines have width, this approximates it
        double sigma = 1.0;
        foreach (var (wl, strength) in lines)
        {
            for (int i = 0; i < numSamples; i++)
            {
                double diff = wavelengthsNm[i] - wl;
                radiance[i] += strength * Math.Exp(-(diff * diff) / (2.0 * sigma * sigma));
            }
        }

        return (wavelengthsNm, radiance);
    }

    /// <summary>
    /// Wien's displacement law: quick estimate of peak emission wavelength.
    /// Good sanity check — your spectrum array peak should be near this.
    /// </summary>
    public static double PeakWavelengthNm(double tempK)
        => 2_897_771.9 / tempK;
}