Webový server přijímá http requesty přes lokální síť a má přehled o stavu hry. Umí rozeznat, ketrý ze tří zařízení posílá request. Řídí následující časti hry.

1. Nahrání videa:

    - zařízení 3 může požádat o nastavení loopu videozáznamu kamer
    - to lze potvrdit (označit jako hotové) pouze po té, co zařízení 1 potvrdí nahrání videa

2. Komunikace s llm

    - zařízení 2 může zaslat dotaz pro llm, který server zprostředkovaně pošle llm
    - pokud zařízení 1 zašle heslo, bude nadále zprostředkované requesty pro zařízení 2 posílat s tímto hesle (upraví prompt)

3. Disco - pokud zařízení 2 a 3 zároveň (v úzkém časovém úseku) pošlou request potvrzující rozsvícení světel (zařízení 2) a zapnutí hudby (zařízení 3), bude splněn úkol disco
   """
