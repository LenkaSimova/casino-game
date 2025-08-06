# Hra Útěk z casina

## Cíl projektu

Vyvinout aplikaci, která bude součástí větší únikové táborové hry na téma "Útěk z casina". Aplikace poběží současně na několika počítačích dostupných účastníkům (klienti) a jednom nedostupném centrálním (server). Každý klient

-   po spuštění zobrazuje herní automat (slot machine);

-   z aktuální konstelace symbolů automatu lze vyčíst heslo;

-   po odemknutí přepne obrazovku do terminálu, kde hráči zadávají textové příkazy;

-   komunikuje v reálném čase s centrálním serverem.

## Slot machine

V této fázi každá instance zobrazuje výherní automat pomocí grafického rozhraní s využitím UI frameworku Avalonia. Stisknutím klávesy mezerník se aktuálně zobrazované symboly přetočí. Po zadání správných písmen z právě zobrazovaných symbolů se otevře terminál (více v sekci níže). Pro zadání hesla se nezobrazuje žádné speciální pole, aplikace neustále čeká na zadání hesla a potvrzením klávesou enter. Špatně zadané heslo instance nijak nesignalizuje.

## Terminál

Zadáním příkazu `help` se hráči dozví některé dostupné příkazy. Jiné se dozví v jiných částech hry.

Pomocí terminálů plní hráči různé dílčí úkoly hlavní hry, například přepnou kamery na běžící smyčku pro oklamání ostrahy nebo získat později potřebné heslo.

## Komunikace instancí

Jendotlivé instance komunikují přes centrální server pomocí http (asynchronní metody). Centrální server udržuje aktuální globání stav hry, který mohou jednotliví klienti číst, měnit a případně být o něm notfikováni. To umožní například kontrolovat, že hráči zadají kombinaci příkazů v požadované sekvenci na různých terminálech nebo že zadají příkaz na všech terminálech v určitém omezeném časovém intervalu.
