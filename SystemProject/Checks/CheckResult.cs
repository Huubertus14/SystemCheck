namespace SystemCheck.Checks;

public sealed record CheckResult(string Name, CheckStatus Status, string Message)
{
    public static CheckResult Passed(string name, string message) =>
        new(name, CheckStatus.Passed, message);

    public static CheckResult Warning(string name, string message) =>
        new(name, CheckStatus.Warning, message);

    public static CheckResult Failed(string name, string message) =>
        new(name, CheckStatus.Failed, message);
}

public enum CheckStatus
{
    Passed,
    Warning,
    Failed
}
