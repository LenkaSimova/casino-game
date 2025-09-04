# Hra Útěk z casina - Uživatelská dokumentace

## Přehled

"Útěk z casina" je aplikace navržená jako součást větší únikové táborové hry. Hra se skládá ze tří částí:

-   **Klientská aplikace** - běží na počítačích dostupných hráčům
-   **Centrální server** - koordinuje stav hry mezi všemi klienty

## Spuštění aplikace

### Požadavky

-   .NET 8.0
-   Pro LLM funkčnost: Ollama (volitelné)

### Spuštění serveru

```bash
cd server/CasinoServer
dotnet run
```

Server poběží na `http://localhost:5122`

### Spuštění klienta

```bash
cd client/CasinoClient
dotnet run
```

### Konfigurace terminálů

Každý klient lze nakonfigurovat úpravou souboru `terminal_config.json`:

```json
{
    "Id": 1,
    "Name": "Terminal-1",
    "AllowedCommands": ["help", "uploadvideo", "loopvideo", "llm"],
    "Prompt": "casino@terminal-1:~$ ",
    "ServerBaseUrl": "http://localhost:5122"
}
```

## Herní režimy

### 1. Herní automat (Slot Machine)

-   **Ovládání**: Stisknutím mezerníku nebo tlačítka "SPIN" roztočíte válce
-   **Sázka**: Můžete nastavit výši sázky (1-10 kreditů)
-   **Výhry**: Různé kombinace symbolů přinášejí různé výhry
-   **Přechod na terminál**: Zadejte první písmena aktuálně zobrazených symbolů

#### Symboly a jejich význam

-   🌟 Star (s)
-   🍊 Orange (o)
-   🍋 Lemon (l)
-   🍇 Grape (g)
-   🍒 Cherry (c)
-   🍎 Apple (a)
-   💎 Diamond (d)
-   🔔 Bell (b)

**Příklad**: Pokud jsou zobrazeny symboly Star-Orange-Lemon, zadejte "sol" pro přechod na terminál.

### 2. Terminál

Po úspěšném zadání kódu se zobrazí terminálové rozhraní.

#### Základní příkazy

-   `help` - zobrazí dostupné příkazy
-   `clear` - vyčistí obrazovku
-   `echo <text>` - vypíše zadaný text
-   `status` - zobrazí stav systému
-   `exit` - návrat na herní automat

#### Herní příkazy (závisí na konfiguraci terminálu)

-   `uploadvideo` - potvrdí nahrání videa (zařízení 1)
-   `loopvideo` - spustí smyčku videa (zařízení 3)
-   `updateprompt` - aktualizuje heslo pro LLM (zařízení 1)
-   `musicon` - zapne hudbu pro disco (zařízení 3)
-   `lightson` - zapne světla pro disco (zařízení 2)
-   `llm` - spustí konverzaci s AI asistentem

#### LLM režim

-   Spusťte příkazem `llm`
-   Konverzace s AI asistentem
-   Ukončení: `exit` nebo `quit`

## Herní mechanismy

### 1. Nahrání videa

1. Zařízení 1 musí potvrdit nahrání videa (`uploadvideo`)
2. Teprve poté může zařízení 3 spustit smyčku (`loopvideo`)

### 2. LLM komunikace

1. Zařízení 1 může nastavit heslo pro LLM (`updateprompt`) - bude nastaveno součástí system prompt
2. Zařízení 2 se pak může při komunikaci s LLM snažit heslo získat

### 3. Disco úkol

-   Zařízení 2 a 3 musí současně (do 5 sekund) potvrdit:
    -   Zařízení 2: `lightson`
    -   Zařízení 3: `musicon`
-   Po splnění je úkol disco dokončen

## LLM

TODO

## Řešení problémů

### Server se nespustí

-   Zkontrolujte, zda není port 5122 obsazen
-   Ujistěte se, že máte nainstalovaný .NET 8.0

### Klient se nemůže připojit k serveru

-   Ověřte, že server běží
-   Zkontrolujte `ServerBaseUrl` v `terminal_config.json`

### LLM nefunguje

-   Pro lokální LLM: Ujistěte se, že běží Ollama server (`ollama serve`)
-   Pro Google Gemini: Nastavte proměnnou prostředí `GEMINI_API_KEY`

### Terminál nezobrazuje některé příkazy

-   Zkontrolujte pole `AllowedCommands` v konfiguraci terminálu
-   Různé terminály mají různá oprávnění podle ID

## Tipy pro hráče

1. **Pozorně sledujte symboly** na herním automatu - jsou klíčem k přechodu na terminál
2. **Koordinujte se s ostatními hráči** - mnoho úkolů vyžaduje spolupráci
3. **Experimentujte s příkazy** - některé příkazy jsou skryté a objevíte je během hry
4. **Využijte LLM** - AI asistent vám může pomoci s řešením hádanek (pokud má správné heslo)
