namespace DeviceOptimizer.Api.Services
{
    public class GreenReportResult
    {
        public double AvoidedCo2Kg { get; set; }
        public double TreesEquivalent { get; set; }
        public double CarKmEquivalent { get; set; }
    }

    public static class GreenReportCalculator
    {
        public const double ReplacementCycleYears = 3.0;
        public const double KgCo2PerTreePerYear = 21.0;
        public const double KgCo2PerCarKm = 0.12;

        public static double AvoidedCo2ForDevice(DateTime purchaseDate, double manufacturingKgCo2e)
        {
            var ageYears = (DateTime.UtcNow - purchaseDate).TotalDays / 365.0;
            var yearsBeyondCycle = ageYears - ReplacementCycleYears;

            if (yearsBeyondCycle <= 0) return 0;

            return yearsBeyondCycle * manufacturingKgCo2e;
        }

        public static GreenReportResult Summarize(double avoidedCo2Kg)
        {
            return new GreenReportResult
            {
                AvoidedCo2Kg = Math.Round(avoidedCo2Kg, 1),
                TreesEquivalent = Math.Round(avoidedCo2Kg / KgCo2PerTreePerYear, 1),
                CarKmEquivalent = Math.Round(avoidedCo2Kg / KgCo2PerCarKm, 0)
            };
        }
    }
}
