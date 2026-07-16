using Microsoft.VisualBasic.Devices;

namespace SystemCheck.Checks;

public sealed class MemoryCheck : IHardwareCheck
{
    private const double WarningThresholdPercent = 10;

    public string Name => "Werkgeheugen";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var info = new ComputerInfo();
            var availablePercent = info.AvailablePhysicalMemory * 100d / info.TotalPhysicalMemory;
            var availableGb = info.AvailablePhysicalMemory / 1024d / 1024d / 1024d;
            var totalGb = info.TotalPhysicalMemory / 1024d / 1024d / 1024d;
            var message = $"{availableGb:F1} van {totalGb:F1} GB beschikbaar ({availablePercent:F1}%).";

            return Task.FromResult(availablePercent < WarningThresholdPercent
                ? CheckResult.Warning(Name, message)
                : CheckResult.Passed(Name, message));
        }
        catch (Exception exception)
        {
            return Task.FromResult(CheckResult.Failed(Name, exception.Message));
        }
    }
}
