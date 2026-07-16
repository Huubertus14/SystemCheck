# PC hardwarecontrole

Een modulaire Windows-app die hardwarecontroles uitvoert en het resultaat rechtsonder als Windows-melding toont.

## Starten

```powershell
cd SystemProject
dotnet run
```

## Een controle toevoegen

Maak in `SystemProject/Checks` een nieuwe publieke class met een parameterloze constructor en implementeer `IHardwareCheck`:

```csharp
public sealed class MijnNieuweCheck : IHardwareCheck
{
    public string Name => "Mijn controle";

    public Task<CheckResult> RunAsync(CancellationToken cancellationToken = default)
    {
        // Voer hier de controle uit.
        return Task.FromResult(CheckResult.Passed(Name, "Alles is in orde."));
    }
}
```

De app ontdekt deze class automatisch. `Program.cs` hoeft dus niet aangepast te worden.

De voorbeeldcontroles bewaken vrije ruimte op de Windows-schijf en beschikbaar fysiek geheugen. De grenswaarden staan als constants bovenaan de betreffende classes.

## Installeren en automatisch opstarten

Bouw het zelfstandige installatiepakket vanuit de repository-root:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File .\Build-Installer.ps1
```

Pak daarna `artifacts/SystemCheck-win-x64.zip` uit en dubbelklik op `Install.cmd`. De app wordt voor de huidige gebruiker geïnstalleerd in `%LOCALAPPDATA%\SystemCheck` en krijgt een snelkoppeling in de Windows-opstartmap.

Dubbelklik op `Uninstall.cmd` in hetzelfde uitgepakte pakket om zowel de app als de automatische opstartinstelling te verwijderen.
