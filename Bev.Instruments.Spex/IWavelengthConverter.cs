namespace Bev.Instruments.Spex
{
    public interface IWavelengthConverter
    {
        double StepsToWavelength(int steps);
        int WavelengthToSteps(double wavelength);
    }
}
