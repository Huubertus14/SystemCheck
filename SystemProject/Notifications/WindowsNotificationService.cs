using SystemCheck.Checks;

namespace SystemCheck.Notifications;

public sealed class WindowsNotificationService : IDisposable
{
    private readonly NotifyIcon _notifyIcon = new()
    {
        Icon = SystemIcons.Information,
        Text = "PC hardwarecontrole",
        Visible = true
    };

    public void ShowSummary(IReadOnlyCollection<CheckResult> results)
    {
        var warnings = results.Count(result => result.Status == CheckStatus.Warning);
        var failures = results.Count(result => result.Status == CheckStatus.Failed);
        var isHealthy = warnings == 0 && failures == 0;

        var title = isHealthy ? "Hardwarecontrole voltooid" : "Hardwarecontrole: aandacht nodig";
        var message = string.Join(
            Environment.NewLine,
            results.Select(result => $"{GetSymbol(result.Status)} {result.Name}: {result.Message}"));

        _notifyIcon.BalloonTipTitle = title;
        _notifyIcon.BalloonTipText = message;
        _notifyIcon.BalloonTipIcon = isHealthy ? ToolTipIcon.Info : ToolTipIcon.Warning;
        _notifyIcon.ShowBalloonTip(8_000);

        // Houd de app kort actief zodat Windows de melding kan tonen.
        Application.DoEvents();
        Thread.Sleep(8_500);
    }

    public void Dispose() => _notifyIcon.Dispose();

    private static string GetSymbol(CheckStatus status) => status switch
    {
        CheckStatus.Passed => "OK",
        CheckStatus.Warning => "!",
        CheckStatus.Failed => "X",
        _ => "?"
    };
}
