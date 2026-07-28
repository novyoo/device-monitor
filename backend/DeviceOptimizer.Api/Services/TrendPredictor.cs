using DeviceOptimizer.Api.DTOs;

namespace DeviceOptimizer.Api.Services
{
    public class TrendResult
    {
        public double WeeksUntilRed { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public static class TrendPredictor
    {
        public static TrendResult? Predict(List<HealthHistoryPointDto> history)
        {
            if (history.Count < 3) return null;

            var oldest = history[0];
            var newest = history[^1];

            var daysBetween = (newest.Timestamp - oldest.Timestamp).TotalDays;
            if (daysBetween < 1) return null;

            var scorePerDay = (newest.Score - oldest.Score) / daysBetween;
            if (scorePerDay >= 0) return null;
            if (newest.Score <= 60) return null;

            var daysUntilRed = (newest.Score - 60) / -scorePerDay;
            var weeks = Math.Round(daysUntilRed / 7.0, 1);

            return new TrendResult
            {
                WeeksUntilRed = weeks,
                Message = $"Declining about {Math.Round(-scorePerDay, 1)} point(s)/day — at this rate, red in ~{weeks} week(s)."
            };
        }
    }
}
