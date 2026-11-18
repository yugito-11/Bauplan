# Bauplan – Anleitung

Bauplan besteht aus zwei Teilen, die zusammenarbeiten:

| | Wer braucht es | Wofür |
|---|---|---|
| **Bauplan.exe** (Windows-Programm) | wer plant, meist einer aus der Gruppe | Schematics ansehen (2D, 3D), Baupläne und Etappen festlegen, Material, daraus Aufgaben fürs Team machen, Fortschritt sehen |
| **bauplan-….jar** (Fabric-Mod) | jeder, der mitbaut | Aufgaben, Material, Lager, Orte im Spiel; zählt automatisch, was gesammelt und gebaut ist; Live-Verbindung zum Programm |

Beide holen sich neue Versionen selbst aus den [Releases](https://github.com/yugito-11/bauplan/releases/latest).

## 1. Mod installieren (jeder, der mitbaut)

1. Modrinth App öffnen und euer 26.3-Fabric-Profil auswählen.
2. Prüfen, ob **Fabric API** installiert ist. Falls nicht: in Modrinth nach „Fabric API“ suchen und installieren.
3. Die `bauplan-….jar` aus dem [neuesten Release](https://github.com/yugito-11/bauplan/releases/latest) ins Profil holen:
   „Inhalt hinzufügen“ → Datei auswählen, oder in den Ordner `mods` ziehen.
4. Spiel starten.

Auf dem Server muss nichts installiert sein. Wer die Mod nicht hat, merkt davon nichts.

## 2. Programm installieren (wer plant)

[Bauplan.exe](https://github.com/yugito-11/bauplan/releases/latest/download/Bauplan.exe) herunterladen, speichern, doppelklicken –
keine Installation. Windows warnt beim ersten Start, weil die Datei nicht signiert ist: „Weitere Informationen“ → „Trotzdem ausführen“.

Gibt es auf dem PC noch Projekte aus dem alten Programm **Blockplan**, fragt Bauplan beim ersten Start, ob es sie
übernehmen soll. Blockplan selbst bleibt dabei unverändert.

## 3. Gemeinsame Datenbank einrichten (nur EINER aus der Gruppe, einmalig, ca. 10 Minuten)

Mod und Programm speichern eure Aufgaben in einer kostenlosen Firebase-Datenbank von Google.
Dafür braucht man ein Google-Konto.

1. Auf https://console.firebase.google.com gehen und **Projekt erstellen** klicken.
   Name z. B. `bauplan`. Google Analytics kann man ausschalten.
2. Links im Menü unter **Erstellen** (Build) auf **Realtime Database** klicken, dann **Datenbank erstellen**.
3. Als Standort **Belgien (europe-west1)** wählen. Dann **Im gesperrten Modus starten**.
4. Oben auf den Reiter **Regeln** gehen, alles löschen und das hier einfügen:

   ```json
   {
     "rules": {
       "gruppen": {
         "$gruppe": {
           ".read": true,
           ".write": true
         }
       }
     }
   }
   ```

   Dann auf **Veröffentlichen** klicken. Firebase zeigt eine Warnung, dass die Regeln offen sind.
   Das ist hier gewollt: Lesen und schreiben kann nur, wer Adresse **und** Gruppencode kennt.
5. Zurück auf den Reiter **Daten**. Oben steht die Adresse, z. B.
   `https://bauplan-1a2b3-default-rtdb.europe-west1.firebasedatabase.app`.
   Diese Adresse an alle in der Gruppe schicken.


## 4. Verbinden

**Im Spiel (jeder):**
1. In einer Welt oder auf einem Server **J** drücken.
2. Im Reiter **Einstellungen** die Datenbank-Adresse einfügen (Strg+V) und einen **Gruppencode** eintragen.
   Alle nehmen denselben Code. Nehmt etwas Schwer-zu-Erratendes, z. B. `drachenburg-4821`.
3. **Speichern** klicken. Unten steht dann grün „Verbunden“.

**Im Programm:** links auf **Team**, dieselbe Adresse und denselben Gruppencode eintragen, dazu deinen
Minecraft-Namen, dann **Verbinden**.

## Vom Plan zu Aufgaben

1. Im Programm eine Schematic öffnen (`.litematic`, `.schem`, `.nbt`, auch per Ziehen ins Fenster).
2. Rechts unter **Startecke im Spiel** eintragen, wo gebaut wird (X / Y / Z der Ecke mit den kleinsten Koordinaten).
3. Im 2D-Plan mit **Bauplan aus Bereich** Teile festlegen (z. B. Fundament, Mauern, Türme) und sie unter
   **Etappen** zu Bauabschnitten zusammenfassen. Das ist optional – es geht auch einfach nach Ebenen.
4. **Team** → **„… als Aufgaben senden“**: aufteilen nach Etappen, Bauplänen oder Ebenen; Ziel
   (Sammeln + Platzieren, Platzieren oder nur Sammeln); für wen; in Reihenfolge freischalten.
   Die Vorschau zeigt, was angelegt wird.
5. Im Spiel erscheinen die Aufgaben sofort (J). Jede Blockart ist ein Ziel. Mit Startecke zählt die Mod
   **Block für Block an der richtigen Stelle** – ein falscher Block an der falschen Stelle zählt nicht.
6. Den Fortschritt seht ihr im Programm unter **Team** (alle Aufgaben mit Balken) und bei jeder Etappe („Im Spiel“).

## Live-Verbindung (optional)

Das Programm kann zeigen, was im Spiel gerade wirklich steht und wo die Spieler sind:

- **Im eigenen Spiel:** Im Spiel J → Einstellungen → **Live-Link kopieren**, im Programm links unter
  „Live-Verbindung“ einfügen → **Verbinden**. Programm und Spiel müssen auf demselben PC laufen.
- **Für alle auf einem Server:** die Mod zusätzlich in den `mods`-Ordner des Fabric-Servers legen. Der Link steht dann
  im Server-Log (`Bauplan Live läuft: http://<server-adresse>:8100/?t=…`); Port 8100 muss erreichbar sein.
  Einstellungen in `config/bauplan/config.properties`.

## Bedienung im Spiel

**J** öffnet das Menü (die Taste kann man unter Optionen → Steuerung ändern).

- **Aufgaben:** Text eingeben, dann **+** (ohne Ort) oder **+ hier** (mit deiner Position).
  Der erste Knopf ändert den Status (Offen → In Arbeit → Fertig).
  Der Knopf mit dem Namen gibt die Aufgabe dem nächsten Spieler; einfach so oft klicken, bis der richtige Name dasteht.
  „Alle/Meine“ filtert auf deine eigenen Aufgaben.
- **Material:** Item und Anzahl in Stück eintragen. Mit -64 / +1 / +64 zählt ihr hoch, was gesammelt ist.
  Die Mod rechnet aus, wie viele Stacks und Shulker noch fehlen.
- **Orte:** Namen eingeben und **Hier speichern**. Bei Oberwelt-Orten steht dabei, wo das Nether-Portal hin muss.
  **Kopieren** legt die Koordinaten in die Zwischenablage.

**Sammel-Aufgaben:** Trag im Menü bei „Anz.“ eine Zahl ein, nimm das Item in die Hand und klick auf **+**.
Oder schreib `/bp sammeln 640` (Item in der Hand) bzw. `/bp sammeln 640 dirt`.
Die Mod zählt bei dem Spieler, dem die Aufgabe gehört, wie viele davon er dabei hat, auch in Shulker-Kisten.
Sobald genug da ist, wird die Aufgabe automatisch fertig. Im Bauplan-Programm (Reiter Team) sieht man den Fortschritt.

**Abbauen und Platzieren:** Im Menü mit dem Knopf links neben „Anz.“ zwischen Sammeln, Abbauen und Platzieren wechseln.
Oder `/bp abbauen 500 stone` bzw. `/bp platzieren 200 stone_bricks`. Gezählt wird über die Spiel-Statistik,
erst ab dem Moment, in dem die Aufgabe dem Spieler gehört. Wechselt man den Server oder die Welt, zählt die Mod dort weiter,
ohne die alte Zahl zu verlieren.

**Nur an der Baustelle zählen:** Leg die Aufgabe mit **+ hier** statt **+** an (oder hänge `hier` an den Befehl,
z. B. `/bp platzieren 200 stone_bricks hier 40`). Dann zählen nur Blöcke im Umkreis von 32 Blöcken
(bzw. der angegebenen Zahl) um die Stelle, an der du beim Anlegen stehst, in derselben Dimension.
Ohne „hier“ zählt jeder Block, egal wo.

**Anzeige am Bildschirmrand:** Im Spiel siehst du links deine offenen Aufgaben (die in Arbeit zuerst, gelb).
Haben sie Koordinaten, steht dahinter, wie weit sie entfernt sind. Mit `/bp anzeige rechts` kommt die Liste
nach rechts, mit `/bp anzeige aus` verschwindet sie. Das geht auch im Menü unter Einstellungen.

In der Spielerliste tauchen alle auf, die die Mod mit eurem Gruppencode benutzen,
und alle, denen schon eine Aufgabe gegeben wurde.

Wenn dir jemand eine Aufgabe gibt, bekommst du eine Nachricht im Chat (nur du siehst sie).

**Außerdem im Menü (J):** Projekte (oben rechts wechseln, **+** legt eins an), Lager-Kisten (zählen beim Öffnen,
was drin ist – Sammel-Ziele rechnen das mit), Teilziele, Schritte und Kommentare unter „…“ bei einer Aufgabe,
Reihenfolge („erst nach …“), Kombi-Ziele (sammeln und verbauen) und der Wegweiser zur Baustelle.

## Discord

Änderungen landen automatisch in einem Kanal auf eurem Discord-Server, z. B. „**Ben** hat "Brücke" erledigt“.

**Einrichten (Admin, einmalig):**
1. In Discord: beim Kanal auf das Zahnrad → **Integrationen** → **Webhooks** → **Neuer Webhook**.
   Optional Namen und Bild ändern, dann **„Webhook-URL kopieren“**.
2. Im Bauplan-Programm: **Team** → **Discord** → Adresse einfügen → **Webhook speichern** (als Admin, falls es ein Admin-Passwort gibt).

Gepostet wird alles, was im Verlauf landet. Wer etwas ändert, schickt die
Nachricht selbst, aus der Mod oder aus dem Programm. Ein eigener Server ist nicht nötig.
Erwähnungen wie @everyone in Aufgabentexten pingen niemanden an.

**Absendername:** Gehört die Änderung zu einem Projekt, heißt der Absender im Kanal wie das Projekt
(z. B. „Rathaus“), sonst „Bauplan“. Discord verbietet ein paar Dinge im Namen (z. B. das Wort „Discord“, @ oder #),
die werden dabei weggelassen.

Hinweis: Die Webhook-Adresse liegt in eurer Datenbank. Wer die Datenbank-Adresse kennt, könnte sie
auslesen und in den Kanal schreiben. Falls das passiert: In Discord den Webhook löschen und einen neuen anlegen.

## Admin

Solange kein Admin-Passwort festgelegt ist, darf jeder alles. Sobald einer eins festlegt
(im Spiel: Menü → Einstellungen → Admin), gilt:

| | Admin | Alle anderen |
|---|---|---|
| Aufgaben anlegen, Status ändern | ✔ | ✔ |
| Sich selbst eintragen/austragen | ✔ | ✔ |
| Andere einteilen | ✔ | ✘ |
| Aufgaben, Material, Orte löschen | alle | nur selbst angelegte |
| Spieler aus dem Team entfernen | ✔ | ✘ |

Weitere Admins melden sich mit demselben Passwort an – im Spiel und im Programm unter Team → Admin.

Die Sperre gilt in Mod und Programm. Wer eure Datenbank-Adresse kennt und sich auskennt,
könnte sie umgehen, indem er direkt in die Datenbank schreibt. Gebt die Adresse also nur
an Leute aus eurer Gruppe weiter.


## Befehle (gehen auch, ohne das Menü zu öffnen)

| Befehl | Was er macht |
|---|---|
| `/bp` | Menü öffnen |
| `/bp aufgabe <Text>` | Aufgabe anlegen |
| `/bp sammeln <Anzahl> [Item]` | Sammel-Aufgabe für dich anlegen (ohne Item: das in deiner Hand) |
| `/bp abbauen <Anzahl> [Block]` | Abbau-Aufgabe für dich anlegen |
| `/bp platzieren <Anzahl> [Block]` | Bau-Aufgabe für dich anlegen |
| `… hier [Radius]` | an `abbauen`/`platzieren` angehängt: nur im Umkreis deiner Position zählen (Standard 32) |
| `/bp suche <Item>` | In welchen Lager-Kisten liegt wie viel davon? |
| `/bp ort <Name>` | Deine aktuelle Position als Ort speichern |
| `/bp meine` | Deine offenen Aufgaben im Chat anzeigen |
| `/bp anzeige links/rechts/aus` | Aufgaben-Anzeige an den linken/rechten Rand setzen oder ausschalten |
| `/bp update` | Nach einer neuen Version sehen und sie herunterladen |
| `/bp pruefen` | Offene Ziele im Projekt: Stand und warum sie (nicht) zählen |
| `/bp projekt ecke1` / `ecke2` | Bereich des gewählten Projekts mit zwei Ecken festlegen |
| `/bp projekt bereich aus` | Bereich des Projekts entfernen |
| `/bp anzeige bearbeiten` | Anzeige mit der Maus verschieben und Größe ändern |
| `/bp kombi <Anzahl> [Block] [hier]` | Kombi-Aufgabe: sammeln und verbauen |
| `/bp admin <Passwort>` | Als Admin anmelden (oder, wenn noch keins existiert, Passwort festlegen) |
| `/bp admin neu <Passwort>` | Admin-Passwort ändern (nur Admin) |
| `/bp admin abmelden` | Admin-Rechte auf diesem PC wieder abgeben |
| `/bp projekt neu <Name>` | Projekt anlegen und auswählen |
| `/bp projekt <Name>` / `/bp projekt alle` | Projekt auswählen / alle anzeigen |
| `/bp material <Anzahl> [Item]` | Material eintragen (ohne Item: das in deiner Hand), zählt im Lager mit |
| `/bp meldungen alle/meine/aus` | Meldungen im Chat einstellen |
| `/bp gruppe <Code>` | Gruppencode setzen |
| `/bp datenbank <Adresse>` | Datenbank-Adresse setzen |

Statt `/bp` geht auch `/bauplan`. Die Befehle laufen nur bei dir, der Server bekommt nichts davon mit.

## Probleme?

- **Unten steht rot „Keine Verbindung zur Datenbank“:** Adresse prüfen, sie muss mit `https://` anfangen.
- **„Datenbank verweigert den Zugriff“:** Die Regeln aus Schritt 3.4 sind nicht veröffentlicht.
- **Die anderen sehen meine Aufgaben nicht:** Alle müssen exakt denselben Gruppencode haben (Groß- und Kleinschreibung zählt).
- **Das Programm zeigt „Datenbank antwortet mit 401“:** wie oben, die Regeln aus Schritt 3.4 fehlen.
- **Ein Ziel nach Bauplan zählt nicht:** Stimmt die Startecke im Programm mit der Stelle im Spiel überein?
  `/bp pruefen` zeigt, was die Mod zählt.
- **Live-Verbindung klappt nicht:** Programm und Spiel auf demselben PC? Bei einem Server: Port 8100 freigegeben?

## Versionen

### Neu in 1.0

Erste Version von Bauplan: Windows-Programm und Mod in einem.
- Programm: Schematics in 2D und 3D, Baupläne, Etappen, Materialliste, Hilfslinien, Live-Ansicht.
- Neu: **Team** im Programm – Gruppe verbinden, Etappen/Baupläne/Ebenen als Aufgaben senden, Fortschritt aus dem Spiel,
  Spieler, Verlauf, Discord, Admin.
- Mod: Aufgaben, Material, Lager, Orte, Anzeige am Bildschirmrand, Zählen nach Bauplan – und die Live-Verbindung
  zum Programm (im eigenen Spiel oder auf dem Server).
- Projekte aus dem alten Blockplan werden beim ersten Start übernommen (auf Wunsch).
