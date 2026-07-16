using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SystemCheck.Checks;

public sealed class RamSpeedCheck : IHardwareCheck
{
    private const uint FallbackDataRateMtPerSecond = 6000;

    public string Name => "RAM-snelheid";

    public async Task<CheckResult> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var modules = await ReadMemoryModulesAsync(cancellationToken);
            if (modules.Count == 0)
            {
                return CheckResult.Failed(Name, "Windows heeft geen RAM-snelheid gerapporteerd.");
            }

            var details = string.Join(", ", modules.Select(FormatModule));
            var slowModules = modules
                .Where(module => module.ConfiguredClockSpeed < GetRatedSpeed(module.PartNumber))
                .ToArray();

            if (slowModules.Length > 0)
            {
                return CheckResult.Warning(
                    Name,
                    $"{details}. Controleer XMP/EXPO in het BIOS.");
            }

            return CheckResult.Passed(
                Name,
                $"{details}. De actuele snelheid komt overeen met de sticks.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return CheckResult.Failed(Name, $"Kon RAM-snelheid niet uitlezen: {exception.Message}");
        }
    }

    private static async Task<IReadOnlyList<MemoryModule>> ReadMemoryModulesAsync(
        CancellationToken cancellationToken)
    {
        const string command =
            "Get-CimInstance Win32_PhysicalMemory | " +
            "Select-Object DeviceLocator,PartNumber,ConfiguredClockSpeed | ConvertTo-Json -Compress";

        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -Command \"{command}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("PowerShell kon niet worden gestart.");

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        var output = await outputTask;
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(error.Trim());
        }

        using var document = JsonDocument.Parse(output);
        var elements = document.RootElement.ValueKind == JsonValueKind.Array
            ? document.RootElement.EnumerateArray().ToArray()
            : new[] { document.RootElement };

        return elements
            .Select(element => new MemoryModule(
                element.GetProperty("DeviceLocator").GetString() ?? "RAM",
                element.GetProperty("PartNumber").GetString()?.Trim() ?? "Onbekend",
                element.GetProperty("ConfiguredClockSpeed").GetUInt32()))
            .ToArray();
    }

    private static string FormatModule(MemoryModule module)
    {
        var ratedSpeed = GetRatedSpeed(module.PartNumber);
        return $"{module.DeviceLocator}: {module.ConfiguredClockSpeed}/{ratedSpeed} MT/s ({module.PartNumber})";
    }

    private static uint GetRatedSpeed(string partNumber)
    {
        // Veel partnummers, zoals F5-6000 en CMH...6000, bevatten de volledige datasnelheid.
        var fullSpeed = Regex.Match(partNumber, @"(?<!\d)([3-9]\d{3})(?!\d)");
        if (fullSpeed.Success && uint.TryParse(fullSpeed.Groups[1].Value, out var parsedSpeed))
        {
            return parsedSpeed;
        }

        // KLEVV codeert bijvoorbeeld 6000 MT/s als "-60" in KD5AGU880-60B280F.
        var klevvSpeed = Regex.Match(partNumber, @"^KD5.*-(\d{2})[A-Z]");
        if (klevvSpeed.Success && uint.TryParse(klevvSpeed.Groups[1].Value, out var shortSpeed))
        {
            return shortSpeed * 100;
        }

        // Niet ieder merk gebruikt een decodeerbaar partnummer.
        return FallbackDataRateMtPerSecond;
    }

    private sealed record MemoryModule(
        string DeviceLocator,
        string PartNumber,
        uint ConfiguredClockSpeed);
}
