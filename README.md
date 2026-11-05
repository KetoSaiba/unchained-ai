# ⛓️ UNCHAINED AI für EDOPro

Eine Computer-KI für **EDOPro (Project Ignis)**, die das Deck **„Unchained“** spielt – gratis, mit offenem Quellcode.
Du kannst gegen sie spielen oder sie gegen andere KIs antreten lassen.

📺 Die KI in Aktion: YouTube **[Keto Saiba](https://www.youtube.com/@KetoSaibaYGO)** · Instagram **[@ketosaiba.ygo](https://www.instagram.com/ketosaiba.ygo)** · Patreon **[Keto Saiba](https://www.patreon.com/KetoSaibaYgo)**

> *English: A free, open-source WindBot AI for EDOPro that plays the "Unchained" deck. Download the ZIP under **Releases** and follow `ANLEITUNG.txt` in the ZIP (German).*

## ⬇️ Download
Rechts unter **Releases** die neueste `Unchained-KI-fuer-EDOPro-…….zip` herunterladen. Die Anleitung (`ANLEITUNG.txt`) liegt im ZIP – kurz:

1. EDOPro einmal starten (Updates laden lassen), dann schließen.
2. ZIP: Rechtsklick → Eigenschaften → „Zulassen“ anhaken (falls vorhanden) → entpacken.
3. Den Inhalt von `In-ProjectIgnis-kopieren` in deinen `ProjectIgnis`-Ordner ziehen.
4. Den Text aus `bots-json-Eintrag.txt` in `ProjectIgnis\WindBot\bots.json` direkt unter die erste Zeile `[` einfügen.
5. In EDOPro einen Raum erstellen → „KI hinzufügen“ → **UNCHAINED AI**. Das Deck nutzt auch OCG-Karten, also Prerelease/alle Karten erlauben oder „Deck nicht überprüfen“ anhaken.

## 🧠 Was die KI kann
- **Eigener Zugplaner:** Sie probiert von der echten Hand aus Zugfolgen durch und nimmt das beste Endfeld (Unchained Firelord of Yama, Soul of Abominable Rage im Friedhof)
- Zerstört eigene Karten gezielt so, dass ein noch freier Auslöser losgeht
- Plan B, wenn der Starter von einer Handfalle gestoppt wird, sofern ein Zerstörer auf der Hand liegt
- Stört im Zug des Gegners, unter anderem mit der Kamra-Bombe
- Legt keine Zerstörungseffekte auf Karten, die nicht zerstört werden können
- Rund 53 % Siege in 560 Simulator-Duellen gegen 9 andere KI-Decks. Das sagt nichts über die Stärke gegen Menschen aus, gegen Menschen verliert sie oft

## 🛠️ Selbst bauen (für Programmierer)
- Quellcode: `src/` (C#, .NET Framework 4.8)
- `ExecutorBase.dll` aus deinem `ProjectIgnis\WindBot`-Ordner nach `src/lib/` kopieren
- `dotnet build -c Release` in `src/` → `UnchainedAI.dll` nach `ProjectIgnis\WindBot\Executors`
- Decks: `deck/AI_UnchainedAI.ydk` (Deck der KI), `deck/Unchained.ydk` (zum Selberspielen)

Version: 2026.11.05

## ❤️ Unterstützen
Die KI ist und bleibt **kostenlos**. Wenn du die Arbeit trotzdem unterstützen möchtest: auf **[Patreon](https://www.patreon.com/KetoSaibaYgo)** – ganz freiwillig, du bekommst dafür keine zusätzlichen Inhalte an der KI.

## ⚖️ Lizenz & Hinweise
- Lizenz: **GNU AGPL-3.0** (siehe `LICENSE`). Die KI baut auf [WindBot Ignite](https://github.com/ProjectIgnis/windbot) von Project Ignis auf, das ebenfalls unter der AGPL-3.0 steht. Du darfst den Code nutzen, ändern und weitergeben, solange du deine Änderungen ebenfalls unter der AGPL-3.0 mit Quellcode veröffentlichst.
- **Inoffizielles Fan-Projekt.** Keine Verbindung zu Konami, Shueisha, Studio Dice oder Project Ignis. „Yu-Gi-Oh!“ sowie alle Karten- und Monsternamen sind Marken bzw. Eigentum ihrer jeweiligen Rechteinhaber. Dieses Projekt enthält keine Kartenbilder, Kartentexte oder Spieldateien – nur die KI-Logik und Decklisten.
- Ohne Gewähr: Nutzung auf eigene Verantwortung.
