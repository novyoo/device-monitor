using Microsoft.EntityFrameworkCore;
using DeviceOptimizer.Api.Data;
using DeviceOptimizer.Api.DTOs;
using DeviceOptimizer.Api.Models;

namespace DeviceOptimizer.Api.BackgroundServices
{
    public class HealthCheckInSimulator : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<HealthCheckInSimulator> _logger;
        private readonly int _tickIntervalSeconds;
        private readonly Random _random = new();

        public HealthCheckInSimulator(
            IServiceScopeFactory scopeFactory,
            ILogger<HealthCheckInSimulator> logger,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _tickIntervalSeconds = config.GetValue<int?>("Simulator:TickIntervalSeconds") ?? 20;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Health check-in simulator started - one tick represents one simulated day, every {Seconds}s.",
                _tickIntervalSeconds);

            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_tickIntervalSeconds));

            do
            {
                await RunTick();
            } while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task RunTick()
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var devices = await db.Devices
                    .Where(d => d.Status != DeviceStatus.Retired && d.Personality != DevicePersonality.RealDevice)
                    .ToListAsync();

                foreach (var device in devices)
                {
                    var dto = BuildNextCheckIn(device);
                    await CheckInRecorder.RecordAsync(db, dto);
                }

                _logger.LogInformation(
                    "Health check-in simulator tick: recorded check-ins for {Count} device(s).",
                    devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check-in simulator tick failed - will retry on the next tick.");
            }
        }

        private CheckInDto BuildNextCheckIn(Device device)
        {
            double batteryDropMin, batteryDropMax;
            double diskWearMin, diskWearMax;
            double crashBaseChance;
            double tempBase;

            switch (device.Personality)
            {
                case DevicePersonality.Aging:
                    batteryDropMin = 0.15; batteryDropMax = 0.35;
                    diskWearMin = 0.15; diskWearMax = 0.30;
                    crashBaseChance = 0.06;
                    tempBase = 40;
                    break;
                case DevicePersonality.TroubleProne:
                    batteryDropMin = 0.6; batteryDropMax = 1.2;
                    diskWearMin = 0.5; diskWearMax = 1.0;
                    crashBaseChance = 0.10;
                    tempBase = 42;
                    break;
                default:
                    batteryDropMin = 0.02; batteryDropMax = 0.08;
                    diskWearMin = 0.02; diskWearMax = 0.06;
                    crashBaseChance = 0.02;
                    tempBase = 38;
                    break;
            }

            var previousBattery = device.LastBatteryHealthPercent ?? 100;
            var previousDisk = device.LastDiskWearPercent ?? 0;

            var newBattery = Math.Clamp(previousBattery - NextDouble(batteryDropMin, batteryDropMax), 0, 100);
            var newDisk = Math.Clamp(previousDisk + NextDouble(diskWearMin, diskWearMax), 0, 100);
            var wear = 100 - newBattery;

            var crashChance = Math.Clamp(crashBaseChance + wear * 0.004, 0, 0.8);
            var maxCrashesWhenTriggered = device.Personality == DevicePersonality.TroubleProne ? 4 : 2;
            var crashCount = _random.NextDouble() < crashChance ? _random.Next(1, maxCrashesWhenTriggered) : 0;

            var temperature = Math.Clamp(tempBase + wear * 0.15 + NextDouble(-2, 2), 25, 90);

            var activeUseHours = device.Status == DeviceStatus.Rented
                ? NextDouble(2, 9)
                : NextDouble(0, 1);

            var diskErrorChance = Math.Clamp(0.02 + newDisk * 0.006, 0, 0.9);
            var maxDiskErrorsWhenTriggered = newDisk >= 70 ? 10 : newDisk >= 40 ? 5 : 2;
            var diskErrorCount = _random.NextDouble() < diskErrorChance ? _random.Next(1, maxDiskErrorsWhenTriggered + 1) : 0;

            var shutdownBaseChance = device.Personality == DevicePersonality.TroubleProne ? 0.04 : 0.01;
            var shutdownChance = Math.Clamp(shutdownBaseChance + wear * 0.0035, 0, 0.6);
            var maxShutdownsWhenTriggered = device.Personality == DevicePersonality.TroubleProne ? 5 : 2;
            var suddenShutdownCount = _random.NextDouble() < shutdownChance ? _random.Next(1, maxShutdownsWhenTriggered + 1) : 0;

            var ramUsagePercent = (int)Math.Round(Math.Clamp(device.RamUsageBaselinePercent + NextDouble(-4, 4), 0, 100));

            var previousDaysSinceUpdate = device.LastDaysSinceOsUpdate ?? 0;
            var updateResetChance = device.IsSlowToUpdate ? 0.005 : 0.05;
            var daysSinceOsUpdate = _random.NextDouble() < updateResetChance ? 0 : previousDaysSinceUpdate + 1;

            return new CheckInDto
            {
                ApiKey = device.ApiKey,
                BatteryHealthPercent = (int)Math.Round(newBattery),
                DiskWearPercent = (int)Math.Round(newDisk),
                DiskErrorCount = diskErrorCount,
                CrashCount = crashCount,
                SuddenShutdownCount = suddenShutdownCount,
                TemperatureCelsius = Math.Round(temperature, 1),
                RamUsagePercent = ramUsagePercent,
                ActiveUseHours = Math.Round(activeUseHours, 1),
                DaysSinceOsUpdate = daysSinceOsUpdate
            };
        }

        private double NextDouble(double min, double max)
        {
            return min + _random.NextDouble() * (max - min);
        }
    }
}
