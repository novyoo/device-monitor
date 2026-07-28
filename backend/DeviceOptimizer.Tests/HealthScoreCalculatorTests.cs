using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Tests
{
    public class HealthScoreCalculatorTests
    {
        [Fact]
        public void AllMetricsHealthy_ReturnsScore100AndHealthyBand()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 100,
                diskWearPercent: 0,
                diskErrorCount: 0,
                suddenShutdownCount: 0,
                crashCount: 0,
                temperatureCelsius: 20,
                ramUsagePercent: 50,
                daysSinceOsUpdate: 5);

            Assert.Equal(100, result.Score);
            Assert.Equal("Healthy", result.Band);
            Assert.Empty(result.Reasons);
            Assert.Empty(result.Flags);
        }

        [Fact]
        public void SeveralBadMetrics_SubtractsEachPenaltyAndReturnsActNowBand()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 40,
                diskWearPercent: 90,
                diskErrorCount: 25,
                suddenShutdownCount: 0,
                crashCount: 0,
                temperatureCelsius: 20,
                ramUsagePercent: 50,
                daysSinceOsUpdate: 5);

            Assert.Equal(35, result.Score);
            Assert.Equal("ActNow", result.Band);
            Assert.Equal(3, result.Reasons.Count);
        }

        [Fact]
        public void EveryPenaltyAtMax_ScoreClampsToZero()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 0,
                diskWearPercent: 100,
                diskErrorCount: 100,
                suddenShutdownCount: 10,
                crashCount: 10,
                temperatureCelsius: 90,
                ramUsagePercent: 50,
                daysSinceOsUpdate: 5);

            Assert.Equal(0, result.Score);
            Assert.Equal("ActNow", result.Band);
        }

        [Fact]
        public void HighRamUsage_AddsUnderpoweredFlagWithoutAffectingScore()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 100,
                diskWearPercent: 0,
                diskErrorCount: 0,
                suddenShutdownCount: 0,
                crashCount: 0,
                temperatureCelsius: 20,
                ramUsagePercent: 95,
                daysSinceOsUpdate: 5);

            Assert.Equal(100, result.Score);
            Assert.Contains(result.Flags, f => f.Contains("Underpowered"));
        }

        [Fact]
        public void StaleOsUpdate_AddsUpdateOverdueFlag()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 100,
                diskWearPercent: 0,
                diskErrorCount: 0,
                suddenShutdownCount: 0,
                crashCount: 0,
                temperatureCelsius: 20,
                ramUsagePercent: 50,
                daysSinceOsUpdate: 90);

            Assert.Contains(result.Flags, f => f.Contains("Update overdue"));
        }

        [Fact]
        public void MidRangeMetrics_ReturnsWatchBand()
        {
            var result = HealthScoreCalculator.Calculate(
                batteryHealthPercent: 75,
                diskWearPercent: 60,
                diskErrorCount: 3,
                suddenShutdownCount: 0,
                crashCount: 0,
                temperatureCelsius: 30,
                ramUsagePercent: 50,
                daysSinceOsUpdate: 5);

            Assert.Equal(79, result.Score);
            Assert.Equal("Watch", result.Band);
        }
    }
}
