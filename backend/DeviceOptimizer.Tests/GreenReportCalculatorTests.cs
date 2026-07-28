using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Tests
{
    public class GreenReportCalculatorTests
    {
        [Fact]
        public void DeviceYoungerThanReplacementCycle_AvoidsNoCo2()
        {
            var avoided = GreenReportCalculator.AvoidedCo2ForDevice(
                purchaseDate: DateTime.UtcNow.AddYears(-2),
                manufacturingKgCo2e: 280);

            Assert.Equal(0, avoided);
        }

        [Fact]
        public void DeviceOneYearPastCycle_AvoidsRoughlyOneYearsWorthOfCo2()
        {
            var avoided = GreenReportCalculator.AvoidedCo2ForDevice(
                purchaseDate: DateTime.UtcNow.AddDays(-4 * 365),
                manufacturingKgCo2e: 280);

            Assert.InRange(avoided, 270, 290);
        }

        [Fact]
        public void Summarize_ComputesTreeAndCarKmEquivalents()
        {
            var result = GreenReportCalculator.Summarize(210.0);

            Assert.Equal(210.0, result.AvoidedCo2Kg);
            Assert.Equal(10.0, result.TreesEquivalent);
            Assert.Equal(1750.0, result.CarKmEquivalent);
        }

        [Fact]
        public void Summarize_ZeroAvoidedCo2_ReturnsAllZeros()
        {
            var result = GreenReportCalculator.Summarize(0);

            Assert.Equal(0, result.AvoidedCo2Kg);
            Assert.Equal(0, result.TreesEquivalent);
            Assert.Equal(0, result.CarKmEquivalent);
        }
    }
}
