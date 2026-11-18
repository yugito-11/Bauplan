using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Bauplan.Core;

namespace Bauplan.Views;

/// <summary>„Als Aufgaben ans Team“: Etappen, Baupläne oder Ebenen als Aufgaben in die Gruppe schreiben.</summary>
public sealed class SendDialog : Window
{
    readonly Project p; readonly PlanData pl; readonly string db, group, me;
    readonly List<string> allPlayers;
    readonly TextBox name = new();
    readonly ComboBox mode = new() { HorizontalAlignment = HorizontalAlignment.Stretch };
    readonly NumericUpDown layerH = new() { Value = 4, Minimum = 1, Maximum = 64, FormatString = "0", Increment = 1, Width = 130 };
    readonly ComboBox art = new() { ItemsSource = new[] { "Sammeln + Platzieren", "Platzieren", "Nur sammeln" }, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
    readonly ComboBox dim = new() { ItemsSource = new[] { "Oberwelt", "Nether", "End" }, SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
    readonly CheckBox place = new(), order = new() { Content = "In Reihenfolge freischalten (nächste Etappe erst, wenn die vorige fertig ist)", IsChecked = true };
    readonly CheckBox skipDone = new() { Content = "Fertige Etappen weglassen", IsChecked = true };
    readonly CheckBox rest = new() { Content = "Übrige Blöcke als Material eintragen", IsChecked = true };
    readonly WrapPanel players = new();
    readonly TextBlock preview = new() { TextWrapping = TextWrapping.Wrap };
    readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap };
    readonly Button send = new() { Content = "Senden", Classes = { "accent" }, MinWidth = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
    readonly HashSet<string> who = new(StringComparer.OrdinalIgnoreCase);
    readonly List<Team.Mode> modes = new();
    DispatcherTimer? previewTimer;

    /// <summary>Wurde etwas gesendet? Dann steht in Project.Extra["team"], welche Aufgabe zu welcher Etappe gehört.</summary>
    public bool Sent { get; private set; }

    public SendDialog(Project project, PlanData plan, string database, string groupCode, string myName, List<string> groupPlayers)
    {
        p = project; pl = plan; db = database; group = groupCode; me = myName; allPlayers = groupPlayers;
        Title = "Als Aufgaben ans Team senden"; Width = 560; SizeToContent = SizeToContent.Height; CanResize = false;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        name.Text = p.Name;
        int ns = pl.Stages.Count, np = pl.Parts.Count(t => !t.Base);
        var labels = new List<string>();
        if (ns > 0) { modes.Add(Team.Mode.Stages); labels.Add($"Etappen ({ns}) – je Etappe eine Aufgabe"); }
        if (np > 0) { modes.Add(Team.Mode.Parts); labels.Add($"Baupläne ({np}) – je Bauplan eine Aufgabe"); }
        modes.Add(Team.Mode.Layers); labels.Add("Ebenen – von unten nach oben");
        mode.ItemsSource = labels; mode.SelectedIndex = 0;
        bool hasPlace = p.OriginX != 0 || p.OriginZ != 0 || p.OriginY != 64;
        place.Content = $"Baustelle in der Welt bei X {p.OriginX}  Y {p.OriginY}  Z {p.OriginZ} – die Mod zählt Block für Block nach Plan";
        place.IsChecked = hasPlace;

        TextBlock L(string t) => new() { Text = t, Classes = { "note" }, Margin = new Thickness(0, 4, 0, 0) };
        var root = new StackPanel { Margin = new Thickness(22), Spacing = 6 };
        root.Children.Add(new TextBlock { Text = "Als Aufgaben ans Team senden", FontSize = 17, FontWeight = FontWeight.SemiBold });
        root.Children.Add(new TextBlock { Text = $"Legt in der Gruppe „{group}“ ein neues Projekt mit Aufgaben an. Jede Blockart wird ein Ziel; die Mod zählt im Spiel automatisch mit.", Classes = { "note" }, TextWrapping = TextWrapping.Wrap });
        if (!hasPlace) root.Children.Add(new TextBlock { Text = "Tipp: Trag zuerst links unter „Lage in der Welt“ ein, wo gebaut wird. Dann zählt die Mod Block für Block an der richtigen Stelle.", Foreground = UI.Res("AccentBrush"), TextWrapping = TextWrapping.Wrap });
        root.Children.Add(L("Name des Projekts in der Gruppe")); root.Children.Add(name);
        root.Children.Add(L("Aufteilen nach")); root.Children.Add(mode);
        var layerRow = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Children = { new TextBlock { Text = "Ebenen je Aufgabe", VerticalAlignment = VerticalAlignment.Center }, layerH } };
        root.Children.Add(layerRow);
        var g3 = new Grid { ColumnDefinitions = new ColumnDefinitions("*,10,*") };
        g3.Children.Add(new StackPanel { Children = { L("Ziel"), art } });
        g3.Children.Add(new StackPanel { [Grid.ColumnProperty] = 2, Children = { L("Dimension"), dim } });
        root.Children.Add(g3);
        foreach (var c in new[] { place, order, skipDone, rest }) { c.Margin = new Thickness(0, 2, 0, 0); if (c.Content is string s) c.Content = new TextBlock { Text = s, TextWrapping = TextWrapping.Wrap }; root.Children.Add(c); }
        root.Children.Add(L("Für wen (keiner ausgewählt = alle)")); root.Children.Add(players);
        if (allPlayers.Count == 0) root.Children.Add(new TextBlock { Text = "Noch keine Spieler in der Gruppe. Sie erscheinen, sobald jemand mit der Mod verbunden war.", Classes = { "note" }, TextWrapping = TextWrapping.Wrap });
        foreach (var n in allPlayers)
        {
            var cb = new CheckBox { Content = n, Margin = new Thickness(0, 0, 14, 0) };
            cb.IsCheckedChanged += (_, _) => { if (cb.IsChecked == true) who.Add(n); else who.Remove(n); };
            players.Children.Add(cb);
        }
        root.Children.Add(new Border { Background = UI.Res("SunkBrush"), CornerRadius = new CornerRadius(8), Padding = new Thickness(10, 8), Margin = new Thickness(0, 8, 0, 0), Child = preview });
        root.Children.Add(status);
        var close = new Button { Content = "Schließen", MinWidth = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
        close.Click += (_, _) => Close();
        root.Children.Add(new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 6, 0, 0), Children = { close, send } });
        Content = new ScrollViewer { Content = root, MaxHeight = 860 };

        void ModeChanged()
        {
            var m = CurMode();
            layerRow.IsVisible = m == Team.Mode.Layers;
            skipDone.IsVisible = m == Team.Mode.Stages;
            order.IsVisible = m != Team.Mode.Parts;
            Schedule();
        }
        mode.SelectionChanged += (_, _) => ModeChanged();
        art.SelectionChanged += (_, _) => Schedule();
        layerH.ValueChanged += (_, _) => Schedule();
        foreach (var c in new[] { place, order, skipDone, rest }) c.IsCheckedChanged += (_, _) => Schedule();
        send.Click += async (_, _) => await Send();
        Opened += (_, _) => ModeChanged();
    }

    Team.Mode CurMode() => modes[Math.Clamp(mode.SelectedIndex, 0, modes.Count - 1)];

    Team.Options Opts() => new()
    {
        Mode = CurMode(), LayerHeight = (int)(layerH.Value ?? 4),
        Art = art.SelectedIndex switch { 1 => "platzieren", 2 => "sammeln", _ => "kombi" },
        Dim = dim.SelectedIndex switch { 1 => "nether", 2 => "end", _ => "overworld" },
        WithPlace = place.IsChecked == true, Order = order.IsChecked == true, SkipDone = skipDone.IsChecked == true, RestAsMaterial = rest.IsChecked == true,
        Who = string.Join(",", who), Me = me, Name = string.IsNullOrWhiteSpace(name.Text) ? p.Name : name.Text.Trim(),
    };

    /// <summary>Vorschau gesammelt nach kurzer Pause, da große Projekte etwas Rechenzeit brauchen.</summary>
    void Schedule()
    {
        previewTimer ??= new DispatcherTimer(TimeSpan.FromMilliseconds(250), DispatcherPriority.Background, (_, _) => { previewTimer!.Stop(); Preview(); });
        previewTimer.Stop(); previewTimer.Start();
    }

    void Preview()
    {
        var o = Opts(); var gs = Team.Groups(p, pl, o);
        if (gs.Count == 0)
        {
            preview.Text = o.Mode == Team.Mode.Stages ? "Keine Etappe mit Bauteilen (oder alle schon fertig)." : "Keine Blöcke gefunden.";
            send.IsEnabled = false; return;
        }
        long blocks = 0; var items = new HashSet<string>();
        foreach (var g in gs) foreach (var c in g.Cells) foreach (var (id, n) in Team.Items(p.Cells[c])) { blocks += n; items.Add(id); }
        var lines = gs.Take(8).Select((g, i) => $"{i + 1}. {g.Title} – {g.Cells.Count:N0} Blöcke").ToList();
        if (gs.Count > 8) lines.Add($"… und {gs.Count - 8} weitere");
        preview.Text = $"{gs.Count} Aufgaben · {items.Count} Blockarten · {blocks:N0} Blöcke\n" + string.Join("\n", lines)
            + (o.WithPlace ? "" : "\nOhne Baustelle zählt die Mod nur, was gesammelt wird.");
        send.IsEnabled = true;
    }

    async System.Threading.Tasks.Task Send()
    {
        var o = Opts();
        send.IsEnabled = false; status.Text = "Sende …";
        try
        {
            var payload = await System.Threading.Tasks.Task.Run(() => Team.Build(p, pl, o));
            await Team.Send(db, group, payload);
            // merken, welche Aufgabe zu welcher Etappe / welchem Bauplan gehört (für den Fortschritt im Programm)
            var sent = p.Extra["team"] as JsonObject ?? new JsonObject(); p.Extra["team"] = sent;
            sent["db"] = db; sent["gruppe"] = group; sent["projekt"] = payload.ProjectId; sent["name"] = o.Name; sent["at"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var refs = new JsonObject(); foreach (var (k, v) in payload.Refs) refs[k] = v; sent["refs"] = refs;
            Sent = true;
            status.Text = $"Fertig: Projekt „{o.Name}“ mit {payload.Tasks} Aufgaben und {payload.Goals} Zielen" + (payload.Materials > 0 ? $", {payload.Materials} Materialien" : "") + " angelegt. Im Spiel mit J öffnen.";
        }
        catch (Exception e) { status.Text = "Fehlgeschlagen: " + e.Message; send.IsEnabled = true; }
    }
}
