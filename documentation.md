# Hra Útěk z casina - Programátorská dokumentace

## Architektura

Projekt se skládá ze dvou hlavních komponent:

### 1. Server (ASP.NET Core Web API)

-   **Umístění**: `server/CasinoServer/`
-   **Framework**: ASP.NET Core 8.0
-   **Port**: 5122 (HTTP)

### 2. Klient (Avalonia Desktop App)

-   **Umístění**: `client/CasinoClient/`
-   **Framework**: Avalonia UI + .NET 8.0
-   **Architektura**: MVVM pattern

## Struktura serveru

### Hlavní soubory

#### `Program.cs`

Hlavní vstupní bod serveru. Definuje všechny API endpointy:

```csharp
// Video endpointy
app.MapPost("/video/upload", ...);
app.MapPost("/video/loop", ...);

// LLM endpointy
app.MapPost("/llm/password", ...);
app.MapGet("/llm/isupdated", ...);

// Disco endpointy
app.MapPost("/disco/lights", ...);
app.MapPost("/disco/music", ...);

// Admin endpointy
app.MapPost("/admin/reset", ...);
app.MapPost("/admin/save", ...);
```

#### `GameState.cs`

Reprezentuje aktuální stav hry:

```csharp
public class GameState
{
    public bool VideoUploaded { get; set; }
    public bool LoopStarted { get; set; }
    public bool PasswordUpdated { get; set; }
    public ConcurrentDictionary<string, DateTime> DiscoState { get; set; }
    public bool DiscoCompleted { get; set; }

    public bool CheckDiscoCompletion() // Kontroluje 5s okno pro disco
}
```

#### `GameStatePersistence.cs`

Zajišťuje ukládání a načítání stavu do/ze souboru `gamestate.json`.

### API Endpointy

| Endpoint            | Metoda | Zařízení | Popis                                    |
| ------------------- | ------ | -------- | ---------------------------------------- |
| `/video/upload`     | POST   | 1        | Potvrzení nahrání videa                  |
| `/video/loop`       | POST   | 3        | Spuštění smyčky (vyžaduje nahrané video) |
| `/llm/password`     | POST   | 1        | Nastavení hesla pro LLM                  |
| `/llm/isupdated`    | GET    | 2        | Kontrola, zda je heslo nastaveno         |
| `/disco/lights`     | POST   | 2        | Zapnutí světel                           |
| `/disco/music`      | POST   | 3        | Zapnutí hudby                            |
| `/disco/status`     | GET    | -        | Stav disco úkolu                         |
| `/status`           | GET    | -        | Celkový stav hry                         |
| `/admin/reset`      | POST   | -        | Reset stavu hry                          |
| `/admin/save`       | POST   | -        | Manuální uložení                         |
| `/admin/savedstate` | DELETE | -        | Smazání uloženého stavu                  |

### Identifikace zařízení

Každý request musí obsahovat query parametr `device` s ID zařízení (1, 2, nebo 3).

## Struktura klienta

### MVVM Architektura

#### ViewModels

##### `MainWindowViewModel`

Hlavní ViewModel řídící přepínání mezi views:

-   `CurrentViewModel` - aktuálně zobrazený ViewModel
-   `ShowTerminal()` - přepne na terminál
-   `ShowSlotMachine()` - přepne na herní automat

##### `SlotMachineViewModel`

ViewModel pro herní automat:

```csharp
// Vlastnosti
public int Credits { get; set; }           // Kredity hráče
public Bitmap Reel1Symbol { get; set; }    // Symbol na válci 1
// ... další vlastnosti

// Metody
public async Task SpinAsync()              // Roztočení válců
public void OnKeyPressed(char key)         // Zpracování kláves pro heslo
private void CheckForWins()                // Kontrola výher
```

**Algoritmus pro detekci hesla**:

1. Ukládá poslední 3 stisknuté klávesy
2. Porovnává s iniciálami aktuálních symbolů
3. Při shodě spustí přechod na terminál

##### `TerminalViewModel`

ViewModel pro terminál:

```csharp
// Stavy
public enum TerminalState { Normal, LLMConversation }

// Vlastnosti
public ObservableCollection<TerminalLine> TerminalLines { get; set; }
public string CurrentInput { get; set; }
public TerminalState CurrentState { get; set; }

// Metody pro příkazy
private async Task<CommandResult> HandleHelpCommand(string[] args)
private async Task<CommandResult> HandleApiCommand<T>(...)
```

#### Views

##### `SlotMachineView`

-   **Struktura**: 3 válce s obrázky symbolů, ovládací prvky
-   **Animace**: Pulsující SPIN tlačítko pomocí `KeyFrame` animací
-   **Fokus**: Speciální zpracování pro zachycení kláves

##### `TerminalView`

-   **Struktura**: Scrollovací výstup + vstupní pole
-   **Styly**: Různé barvy pro různé typy zpráv
-   **Auto-scroll**: Automatické posouvání na konec při nových zprávách

### Služby

#### `ConfigurationService`

Načítání konfigurace z `terminal_config.json`:

```csharp
public static TerminalConfig LoadConfig()
{
    // Načte konfiguraci nebo vrátí výchozí
}
```

#### LLM Handlers

##### `ILLMHandler`

Interface pro LLM komunikaci:

```csharp
interface ILLMHandler
{
    void AddSystemMessage(string systemMessage);
    Task<string> SendMessageAsync(string userMessage);
    void ClearHistory();
}
```

##### `LocalLLMHandler`

Implementace pro Ollama:

-   Používá `OllamaSharp` knihovnu
-   Podporuje streaming odpovědí
-   Udržuje historii konverzace

##### `GeminiLLMHandler`

Implementace pro Google Gemini:

-   Používá `Refit` pro REST API
-   Vyžaduje `GEMINI_API_KEY` environment variable

#### API klienti (Refit)

##### `IVideoApi`, `ILLMApi`, `IDiscoApi`

Refit interfaces pro komunikaci se serverem:

```csharp
public interface IVideoApi
{
    [Post("/video/upload")]
    Task<ApiResponse<string>> UploadVideo([AliasAs("device")] int device);
}
```

## Konfigurace

### Terminal Config (`terminal_config.json`)

```json
{
    "Id": 1, // ID zařízení (1-3)
    "Name": "Terminal-1", // Název terminálu
    "AllowedCommands": ["help", "uploadvideo"], // Povolené příkazy
    "Prompt": "casino@terminal-1:~$ ", // Prompt
    "ServerBaseUrl": "http://localhost:5122", // URL serveru
    "Password": "System message for LLM..." // Systémová zpráva pro LLM
}
```

## Persistence

Server automaticky ukládá stav do `gamestate.json`:

```json
{
    "videoUploaded": false,
    "loopStarted": false,
    "passwordUpdated": false,
    "discoState": {
        "lights": "2024-01-01T10:00:00Z",
        "music": "2024-01-01T10:00:03Z"
    },
    "discoWindow": "00:00:05",
    "discoCompleted": true,
    "savedAt": "2024-01-01T10:00:05Z"
}
```

## Rozšíření aplikace

### Přidání nového příkazu

1. **Server**: Přidat endpoint do `Program.cs`
2. **Klient**:
    - Přidat metodu do `TerminalViewModel`
    - Zaregistrovat v `InitializeCommands()`
    - Přidat do `AllowedCommands` v konfiguraci

### Přidání nového API

1. Vytvořit Refit interface v `Services/Apis/`
2. Implementovat handler v `TerminalViewModel`
3. Použít `HandleApiCommand<T>()` helper metodu

### Přidání nového LLM providera

1. Implementovat `ILLMHandler`
2. Zaregistrovat v `TerminalViewModel`

## Buildování a nasazení

### Development

```bash
# Server
cd server/CasinoServer
dotnet run

# Klient
cd client/CasinoClient
dotnet run
```

### Production

```bash
# Server
dotnet publish -c Release

# Klient
dotnet publish -c Release
```

## Testování

### Manuální testování API

```bash
# Test video upload
curl -X POST "http://localhost:5122/video/upload?device=1"

# Test disco coordination
curl -X POST "http://localhost:5122/disco/lights?device=2" &
curl -X POST "http://localhost:5122/disco/music?device=3"
```

### Debugging

-   Server loguje do konzole
-   Klient má Avalonia DevTools v Debug módu
-   Stav hry lze zkontrolovat na `/status` endpointu

## Známé limitace

1. **Synchronizace**: Disco timing závisí na přesnosti systémových hodin
2. **Persistence**: Pouze manuální ukládání stavu
3. **Bezpečnost**: Žádná autentizace API endpointů
4. **Error handling**: Minimální zpracování chyb v klientu
5. **UI responsiveness**: LLM volání blokují UI

## Možná vylepšení

1. **Real-time updates**: SignalR pro živé aktualizace stavu
2. **Autentizace**: Token-based authentication
3. **Async UI**: Non-blocking LLM komunikace
4. **Auto-persistence**: Automatické ukládání při změnách
5. **Monitoring**: Logging a metriky
6. **Testing**: Unit a integration testy
