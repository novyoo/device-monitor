using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Services;

namespace DeviceOptimizer.Tests
{
    public class TrendPredictorTests
    {
        private static HealthHistoryPointDto Point(int daysAgo, int score) => new()
        {
            Timestamp = DateTime.UtcNow.AddDays(-daysAgo),
            Score = score
        };

        private static HealthHistoryPointDto PointMinutesAgo(int minutesAgo, int score) => new()
        {
            Timestamp = DateTime.UtcNow.AddMinutes(-minutesAgo),
            Score = score
        };

        [Fact]
        public void FewerThanThreeReadings_ReturnsNull()
        {
            var history = new List<HealthHistoryPointDto> { Point(1, 90), Point(0, 85) };

            var result = TrendPredictor.Predict(history);

            Assert.Null(result);
        }

        [Fact]
        public void FlatOrImprovingScore_ReturnsNull()
        {
            var history = new List<HealthHistoryPointDto> { Point(10, 70), Point(5, 75), Point(0, 80) };

            var result = TrendPredictor.Predict(history);

            Assert.Null(result);
        }

        [Fact]
        public void AlreadyAtOrBelowRedBand_ReturnsNull()
        {
            var history = new List<HealthHistoryPointDto> { Point(10, 70), Point(5, 65), Point(0, 55) };

            var result = TrendPredictor.Predict(history);

            Assert.Null(result);
        }

        [Fact]
        public void ReadingsLessThanADayApart_ReturnsNullEvenIfDeclining()
        {
            var history = new List<HealthHistoryPointDto>
            {
                PointMinutesAgo(2, 100),
                PointMinutesAgo(1, 98),
                PointMinutesAgo(0, 96)
            };

            var result = TrendPredictor.Predict(history);

            Assert.Null(result);
        }

        [Fact]
        public void DecliningScore_PredictsWeeksUntilRed()
        {
            var history = new List<HealthHistoryPointDto> { Point(10, 100), Point(5, 90), Point(0, 80) };

            var result = TrendPredictor.Predict(history);

            Assert.NotNull(result);
            Assert.Equal(1.4, result!.WeeksUntilRed);
            Assert.Contains("~1.4 week", result.Message);
        }
    }
}
