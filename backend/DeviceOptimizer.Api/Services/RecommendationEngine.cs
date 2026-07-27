namespace DeviceOptimizer.Api.Services
{
    public class RecommendationResult
    {
        public string Action { get; set; } = string.Empty;
        public List<string> Reasons { get; set; } = new();
    }

    public static class RecommendationEngine
    {
        public static RecommendationResult Recommend(
            int? healthScore,
            string? healthBand,
            List<string> healthReasons,
            List<string> healthFlags,
            DateTime purchaseDate,
            int repairCount,
            double lifetimeActiveUseHours)
        {
            var ageYears = (DateTime.UtcNow - purchaseDate).TotalDays / 365.0;
            var ageDisplay = Math.Round(ageYears, 1);
            var usageDisplay = Math.Round(lifetimeActiveUseHours);
            var bandDisplay = healthBand == "ActNow" ? "Act Now" : healthBand;

            var reasons = new List<string>
            {
                $"Purchased {ageDisplay} years ago and repaired {repairCount} time(s) so far.",
                $"Logged about {usageDisplay} active-use hours across its rental life."
            };

            if (repairCount >= 3)
            {
                reasons.Add($"Already repaired {repairCount} times — repairing again is unlikely to be worth the cost.");
                return new RecommendationResult { Action = "Retire", Reasons = reasons };
            }

            if (healthScore == null)
            {
                reasons.Add("No health check-in data yet — send it for a manual inspection before deciding.");
                return new RecommendationResult { Action = "Repair", Reasons = reasons };
            }

            var score = healthScore.Value;

            if (score >= 60 && score <= 75 && ageYears > 3)
            {
                reasons.Add($"Resale Candidate — health score is {score}% and the device is {ageDisplay} years old: good enough to sell as refurbished, but too old for an enterprise DaaS lease.");
                return new RecommendationResult { Action = "Resale", Reasons = reasons };
            }

            if (ageYears >= 3.5)
            {
                reasons.Add($"At {ageDisplay} years old, this device is well past the typical 3-year replacement cycle.");
                return new RecommendationResult { Action = "Retire", Reasons = reasons };
            }

            if (healthBand == "Healthy")
            {
                reasons.Add($"Health score is {score}% ({bandDisplay}) — in good shape to rent out again.");
                return new RecommendationResult { Action = "RentAgain", Reasons = reasons };
            }

            if (healthBand == "ActNow" && (repairCount > 0 || ageYears >= 2))
            {
                reasons.Add($"Health score is {score}% ({bandDisplay}) — condition is too poor to keep in the fleet.");
                reasons.AddRange(healthReasons);
                return new RecommendationResult { Action = "Retire", Reasons = reasons };
            }

            reasons.Add($"Health score is {score}% ({bandDisplay}) — worth repairing before renting out again.");
            reasons.AddRange(healthReasons);
            reasons.AddRange(healthFlags);
            return new RecommendationResult { Action = "Repair", Reasons = reasons };
        }
    }
}
