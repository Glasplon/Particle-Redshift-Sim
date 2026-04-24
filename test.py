import numpy as np

# --- CIE 1931 2° CMFs (trimmed example — use full table in practice) ---
# Get the full table from: https://cvrl.ucl.ac.uk/
# Shape: (N, 4) columns: [wavelength_nm, x_bar, y_bar, z_bar]
def load_cmf():
    # Minimal hardcoded sample — replace with full 1nm table
    # Returns arrays for wavelengths 360-780nm at 1nm spacing
    from scipy.interpolate import interp1d
    # ... load your CMF data here ...
    pass

def spectrum_to_rgb(wavelengths, brightness, normalize=True):
    """
    wavelengths: array of wavelengths in nm
    brightness:  array of spectral power at each wavelength
    returns:     (R, G, B) as integers 0-255
    """
    # 1. Load CIE CMFs and interpolate to your wavelength grid
    cmf_wavelengths, x_bar, y_bar, z_bar = load_cmf_data()  # your CMF table
    
    from scipy.interpolate import interp1d
    x_interp = interp1d(cmf_wavelengths, x_bar, bounds_error=False, fill_value=0)
    y_interp = interp1d(cmf_wavelengths, y_bar, bounds_error=False, fill_value=0)
    z_interp = interp1d(cmf_wavelengths, z_bar, bounds_error=False, fill_value=0)
    
    x_vals = x_interp(wavelengths)
    y_vals = y_interp(wavelengths)
    z_vals = z_interp(wavelengths)

    # 2. Integrate (dot product, assuming uniform spacing)
    dλ = np.gradient(wavelengths)  # handles non-uniform spacing too
    X = np.sum(brightness * x_vals * dλ)
    Y = np.sum(brightness * y_vals * dλ)
    Z = np.sum(brightness * z_vals * dλ)

    # 3. XYZ → linear sRGB
    M = np.array([
        [ 3.2406, -1.5372, -0.4986],
        [-0.9689,  1.8758,  0.0415],
        [ 0.0557, -0.2040,  1.0570]
    ])
    rgb_lin = M @ np.array([X, Y, Z])

    # 4. Normalize
    if normalize:
        peak = np.max(rgb_lin)
        if peak > 0:
            rgb_lin /= peak
    rgb_lin = np.clip(rgb_lin, 0, 1)

    # 5. Gamma encode (linear → sRGB)
    def gamma(c):
        return np.where(c <= 0.0031308, 12.92 * c, 1.055 * c**(1/2.4) - 0.055)
    
    rgb_srgb = gamma(rgb_lin)
    return tuple((rgb_srgb * 255).astype(int))

# Example usage
wavelengths = np.arange(400, 701, 10)   # 400-700nm, 10nm steps
brightness  = np.random.rand(len(wavelengths))  # your actual data
r, g, b = spectrum_to_rgb(wavelengths, brightness)
print(f"RGB: ({r}, {g}, {b})")