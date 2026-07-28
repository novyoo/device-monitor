using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Tests
{
    public class RecommendationEngineTests
    {
        [Fact]
        public void NoHealthDataYet_RecommendsRepair()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: null,
                healthBand: null,
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-1),
                repairCount: 0,
                lifetimeActiveUseHours: 10);

            Assert.Equal("Repair", result.Action);
        }

        [Fact]
        public void RepairedThreeTimes_RecommendsRetireEvenIfHealthy()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 95,
                healthBand: "Healthy",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-1),
                repairCount: 3,
                lifetimeActiveUseHours: 100);

            Assert.Equal("Retire", result.Action);
        }

        [Fact]
        public void YoungHealthyDevice_RecommendsRentAgain()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 90,
                healthBand: "Healthy",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-1),
                repairCount: 0,
                lifetimeActiveUseHours: 50);

            Assert.Equal("RentAgain", result.Action);
        }

        [Fact]
        public void WatchBandOldDevice_RecommendsResale()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 70,
                healthBand: "Watch",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-4),
                repairCount: 0,
                lifetimeActiveUseHours: 50);

            Assert.Equal("Resale", result.Action);
        }

        [Fact]
        public void AgedButStillHealthy_RecommendsRentAgainNotRetire()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 90,
                healthBand: "Healthy",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-4),
                repairCount: 0,
                lifetimeActiveUseHours: 50);

            Assert.Equal("RentAgain", result.Action);
        }

        [Fact]
        public void AgedAndUnhealthy_StillRecommendsRetire()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 50,
                healthBand: "ActNow",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-4),
                repairCount: 0,
                lifetimeActiveUseHours: 50);

            Assert.Equal("Retire", result.Action);
        }

        [Fact]
        public void ActNowBandWithRepairHistory_RecommendsRetire()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 50,
                healthBand: "ActNow",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-1),
                repairCount: 1,
                lifetimeActiveUseHours: 10);

            Assert.Equal("Retire", result.Action);
        }

        [Fact]
        public void ActNowBandYoungNeverRepaired_GetsOneChanceAtRepair()
        {
            var result = RecommendationEngine.Recommend(
                healthScore: 50,
                healthBand: "ActNow",
                healthReasons: new List<string>(),
                healthFlags: new List<string>(),
                purchaseDate: DateTime.UtcNow.AddYears(-1),
                repairCount: 0,
                lifetimeActiveUseHours: 10);

            Assert.Equal("Repair", result.Action);
        }
    }
}
