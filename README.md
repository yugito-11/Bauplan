# Bauplan

Bauplaner und Team-Werkzeug für Minecraft-Gruppenprojekte – ein Windows-Programm zum Planen und eine Fabric-Mod zum Bauen.

**Herunterladen:** [Bauplan.exe](https://github.com/yugito-11/bauplan/releases/latest/download/Bauplan.exe) und die
`bauplan-….jar` aus dem [neuesten Release](https://github.com/yugito-11/bauplan/releases/latest) (Minecraft 26.3, Fabric).
Einrichtung und Bedienung: [ANLEITUNG.md](ANLEITUNG.md).

```
Schematic ──► Bauplan.exe ──► Baupläne, Etappen ──► „Als Aufgaben senden“
                   ▲                                        │
                   │ Fortschritt                            ▼
                   └──────── Firebase (eure Gruppe) ◄──── Mod im Spiel zählt mit
                   ▲
                   └── Live-Link: was im Spiel wirklich steht, wo die Spieler sind
```

| Teil | Ordner | Was |
|---|---|---|
| Programm | [`app/`](app) | C# mit Avalonia 12, 3D über OpenGL, Projekte in einer eigenen SQLite-Datenbank (`%LOCALAPPDATA%\Bauplan\bauplan.db`) |
| Mod | [`mod/`](mod) | Fabric-Mod für Minecraft 26.3. Auf dem Client: Aufgaben, Material, Lager, Zählen nach Bauplan, Live-Link für das Programm. Auf dem Server (optional): Live-Verbindung für alle |

## Funktionen

**Programm**
- Schematics öffnen: `.schem`, `.litematic`, `.nbt` – Ausschnitt wählen, Wasser/Schnee entfernen, drehen
- 2D-Plan (Draufsicht oder Schicht, echte Texturen, Chunk-Linien) und 3D-Ansicht mit Flugmodus, auch für sehr große Bauten
- Baupläne (Teile des Baus), Etappen, Materialliste mit Stacks und Shulkern, Hilfslinien, Messen, Merkliste
- **Team:** Gruppe verbinden, Etappen / Baupläne / Ebenen als Aufgaben ans Team senden, Fortschritt aus dem Spiel
  (auch je Etappe), Status ändern, Spieler, Verlauf, Discord-Webhook, Admin-Anmeldung
- Live-Ansicht der echten Welt und der Spieler über die Mod
- Übernimmt auf Wunsch die Projekte aus dem alten Blockplan; importiert Blockplan-Artefakt-Exporte

**Mod**
- Aufgaben mit Status, mehreren Leuten, Teilzielen, Schritten, Kommentaren und Reihenfolge
- Ziele Sammeln / Abbauen / Platzieren / Sammeln + Platzieren – zählt die Mod automatisch, nach Bauplan Block für Block
- Projekte mit Bereich, Lager-Kisten, Materialliste, Orte, Anzeige am Bildschirmrand mit Wegweiser
- Admin-Passwort, Discord-Meldungen, Verlauf, automatische Updates
- Live-Verbindung zum Programm (eigener Webserver unter `localhost`, auf dem Server für alle)

## Bauen

Programm (.NET 10):
```
dotnet publish app -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o publish/win-x64
```

Mod (JDK 25):
```
cd mod
./gradlew build
```

**Neue Version veröffentlichen:** Nummer in `VERSION` erhöhen, in der `ANLEITUNG.md` einen Abschnitt
`### Neu in <x.y>` schreiben und auf `main` hochladen. Die GitHub Action baut Programm und Mod und legt ein Release
mit beiden Dateien an; beide aktualisieren sich daraus selbst.

Herkunft: zusammengeführt aus [Blockplan](https://github.com/yugito-11/blockplan) (Programm, Live-Mod) und
[Bautrupp](https://github.com/yugito-11/bautrupp) (Team-Mod). Die Datenbank ist mit Bautrupp kompatibel.
