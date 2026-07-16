namespace SystemCheck.Checks;

public interface IHardwareCheck
{
    string Name { get; }

    Task<CheckResult> RunAsync(CancellationToken cancellationToken = default);
}
