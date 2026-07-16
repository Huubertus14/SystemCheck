using System.Reflection;
using SystemCheck.Checks;
using SystemCheck.Notifications;

namespace SystemCheck;

internal static class Program
{
    [STAThread]
    private static async Task Main()
    {
        ApplicationConfiguration.Initialize();

        var checks = DiscoverChecks();
        var results = await Task.WhenAll(checks.Select(check => check.RunAsync()));

        foreach (var result in results)
        {
            Console.WriteLine($"[{result.Status}] {result.Name}: {result.Message}");
        }

        using var notification = new WindowsNotificationService();
        notification.ShowSummary(results);
    }

    private static IReadOnlyList<IHardwareCheck> DiscoverChecks() =>
        Assembly.GetExecutingAssembly().GetTypes().Where(type => !type.IsAbstract && typeof(IHardwareCheck).IsAssignableFrom(type) 
        && type.GetConstructor(Type.EmptyTypes) is not null).Select(type => (IHardwareCheck)Activator.CreateInstance(type)!).OrderBy(check => check.Name).ToArray();
}
