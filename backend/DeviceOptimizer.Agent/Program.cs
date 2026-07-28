using System.Net.Http.Json;
using System.Text.Json;
using DeviceOptimizer.Agent;
using Microsoft.Win32;

var configPath = Path.Combine(AppContext.BaseDirectory, "agent-config.json");
if (!File.Exists(configPath))
{
    Console.WriteLine($"Config file not found: {configPath}");
    return;
}

var config = JsonSerializer.Deserialize<AgentConfig>(
    File.ReadAllText(configPath),
    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

if (config == null || string.IsNullOrWhiteSpace(config.ServerUrl) || string.IsNullOrWhiteSpace(config.ApiKey) || config.ApiKey.StartsWith("PASTE"))
{
    Console.WriteLine("Open agent-config.json and fill in ServerUrl and the ApiKey you were given.");
    return;
}

AddToWindowsStartup();

using var http = new HttpClient();

Console.WriteLine("PULSLE device agent started.");
Console.WriteLine($"Reporting to {config.ServerUrl} every {config.IntervalMinutes} minute(s). Press Ctrl+C to stop.");
Console.WriteLine("It only ever sends the 9 hardware numbers printed below - no files, no apps, no personal data.");

while (true)
{
    var checkIn = VitalsReader.ReadCheckIn(config.ApiKey, config.IntervalMinutes);

    Console.WriteLine();
    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Sending:");
    Console.WriteLine(JsonSerializer.Serialize(checkIn, new JsonSerializerOptions { WriteIndented = true }));

    try
    {
        var response = await http.PostAsJsonAsync($"{config.ServerUrl}/api/checkins", checkIn);
        Console.WriteLine(response.IsSuccessStatusCode
            ? "Check-in accepted by the server."
            : $"Server rejected the check-in: {(int)response.StatusCode} {response.StatusCode}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Could not reach the server: {ex.Message}");
    }

    await Task.Delay(TimeSpan.FromMinutes(config.IntervalMinutes));
}

static void AddToWindowsStartup()
{
    try
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath)) return;

        using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", writable: true);
        key?.SetValue("FleetPulseAgent", $"\"{exePath}\"");
        Console.WriteLine("Set to start automatically next time you log in to Windows.");
    }
    catch
    {
        Console.WriteLine("Could not set up auto-start - you'll need to open this again after a restart.");
    }
}
