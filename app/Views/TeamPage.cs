using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Bauplan.Core;

namespace Bauplan.Views;

/// <summary>Seite „Team“: Verbindung zur Gruppe, Aufgaben mit Fortschritt aus dem Spiel, Spieler und Verlauf.</summary>
public sealed class TeamPage : UserControl
{
    readonly Func<(Project? p, PlanData? pl)> ctx; readonly Func<string, string?> get; readonly Action<string, string?> set; readonly Action saveProject;
    /// <summary>Zuletzt geladener Stand der Gruppe (auch für die Etappen-Seite).</summary>
    public static Team.GroupView? Last;
    public event Action? Updated;
    string? error; bool loading; string? selProject; DateTime loadedAt;
    readonly DispatcherTimer timer;

    public TeamPage(Func<(Project?, PlanData?)> context, Func<string, string?> getSetting, Action<string, string?> setSetting, Action onSaveProject)
    {
        ctx = context; get = getSetting; set = setSetting; saveProject = onSaveProject;
        timer = new DispatcherTimer(TimeSpan.FromSeconds(20), DispatcherPriority.Background, async (_, _) => { if (IsEffectivelyVisible && Connected) await Reload(); });
        timer.Start();
    }

    string Db => get("teamDb") ?? "";
    string Group => get("teamGroup") ?? "";
    string Me => get("who") ?? "";
    public bool Connected => Db.Length > 0 && Group.Length > 0;

    /// <summary>Stand der Gruppe neu laden (still im Hintergrund).</summary>
    public async Task Reload()
    {
        if (!Connected || loading) return;
        loading = true;
        try { Last = new Team.GroupView(await Team.Load(Db, Group)); error = null; loadedAt = DateTime.Now; Updated?.Invoke(); }
        catch (Exception e) { error = e.Message; }
        finally { loading = false; }
        if (IsEffectivelyVisible) Refresh();
    }

    static string Ago(long t)
    {
        var s = (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - t) / 1000;
        return s < 60 ? "gerade eben" : s < 3600 ? $"vor {s / 60} min" : s < 86400 ? $"vor {s / 3600} h" : DateTimeOffset.FromUnixTimeMilliseconds(t).LocalDateTime.ToString("d");
    }

    public void Refresh()
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("330,*"), Margin = new Thickness(20) };
        grid.Children.Add(new Border { Margin = new Thickness(0, 0, 16, 0), Child = UI.Card(new ScrollViewer { Content = Left() }, 14) });
        var right = UI.Card(new ScrollViewer { Content = Right() }); Grid.SetColumn(right, 1); grid.Children.Add(right);
        Content = grid;
    }

    Control Left()
    {
        var sp = new StackPanel { Spacing = 6 };
        sp.Children.Add(UI.H("Gruppe"));
        var db = new TextBox { Text = Db, PlaceholderText = "https://…firebasedatabase.app oder Einladungs-Link" };
        var gr = new TextBox { Text = Group, PlaceholderText = "Gruppencode" };
        var me = new TextBox { Text = Me, PlaceholderText = "Dein Minecraft-Name" };
        sp.Children.Add(UI.Note("Datenbank-Adresse (dieselbe wie in der Mod)")); sp.Children.Add(db);
        sp.Children.Add(UI.Note("Gruppencode")); sp.Children.Add(gr);
        sp.Children.Add(UI.Note("Dein Name")); sp.Children.Add(me);
        var connect = UI.Btn(Connected ? "Speichern und neu laden" : "Verbinden", async () =>
        {
            var d = Team.NormUrl(db.Text ?? ""); var g = Team.NormGroup(gr.Text ?? "");
            if (Team.ParseLink(db.Text ?? "", out var ld, out var lg)) { d = ld; g = lg; }
            if (!d.StartsWith("http") || g.Length == 0) { error = "Bitte Adresse und Gruppencode eintragen."; Refresh(); return; }
            set("teamDb", d); set("teamGroup", g); set("who", me.Text?.Trim() ?? "");
            Last = null; error = null; Refresh(); await Reload();
        }, true);
        sp.Children.Add(connect);
        if (error != null) sp.Children.Add(new TextBlock { Text = error, Foreground = UI.Res("DangerBrush"), TextWrapping = TextWrapping.Wrap });
        else if (Last != null) sp.Children.Add(new TextBlock { Text = $"Verbunden · aktualisiert {loadedAt:HH:mm:ss}", Foreground = UI.Res("AccentBrush") });
        else if (Connected) sp.Children.Add(UI.Note(loading ? "Lade …" : "Noch nicht geladen."));
        else sp.Children.Add(new TextBlock { Text = "Die Datenbank richtet einer aus der Gruppe einmal ein (siehe ANLEITUNG). Adresse und Gruppencode sind dieselben wie in der Mod unter J → Einstellungen.", Classes = { "note" }, TextWrapping = TextWrapping.Wrap });

        if (Last == null) return sp;
        sp.Children.Add(new Border { Height = 8 });
        sp.Children.Add(UI.H($"Spieler ({Last.Players.Count})"));
        if (Last.Players.Count == 0) sp.Children.Add(UI.Note("Noch keine – sie erscheinen, sobald jemand mit der Mod verbunden war."));
        var wrap = new WrapPanel();
        foreach (var n in Last.Players) wrap.Children.Add(new Border { Background = UI.Res("SunkBrush"), CornerRadius = new CornerRadius(10), Padding = new Thickness(8, 2), Margin = new Thickness(0, 0, 6, 6), Child = new TextBlock { Text = n } });
        sp.Children.Add(wrap);
        bool admin = Team.IsAdmin(Last.Raw, get("teamAdminHash"));
        if (Team.HasAdmin(Last.Raw))
        {
            sp.Children.Add(new Border { Height = 8 });
            sp.Children.Add(UI.H("Admin"));
            if (admin) sp.Children.Add(UI.Row(new TextBlock { Text = "Du bist Admin.", Foreground = UI.Res("AccentBrush"), VerticalAlignment = VerticalAlignment.Center }, UI.Btn("Abmelden", () => { set("teamAdminHash", null); Refresh(); })));
            else
            {
                var pw = new TextBox { PasswordChar = '•', PlaceholderText = "Admin-Passwort", Width = 170 };
                sp.Children.Add(new TextBlock { Text = "Ohne Admin-Anmeldung darfst du nur dich selbst einteilen (wie in der Mod).", Classes = { "note" }, TextWrapping = TextWrapping.Wrap });
                sp.Children.Add(UI.Row(pw, UI.Btn("Anmelden", () =>
                {
                    var h = Team.Hash((string?)Last.Raw["admin"]!["salt"] ?? "", pw.Text ?? "");
                    if (h == (string?)Last.Raw["admin"]!["hash"]) { set("teamAdminHash", h); error = null; } else error = "Falsches Admin-Passwort.";
                    Refresh();
                })));
            }
        }
        sp.Children.Add(new Border { Height = 8 });
        sp.Children.Add(UI.H("Discord"));
        var hook = new TextBox { Text = (string?)Last.Raw["discord"]?["webhook"] ?? "", PlaceholderText = "https://discord.com/api/webhooks/…" };
        sp.Children.Add(new TextBlock { Text = "Webhook-Adresse eines Kanals: Dann landen Änderungen (neue Aufgaben, Erledigtes …) dort als Nachricht.", Classes = { "note" }, TextWrapping = TextWrapping.Wrap });
        sp.Children.Add(hook);
        sp.Children.Add(UI.Btn("Webhook speichern", async () =>
        {
            if (!Team.IsAdmin(Last!.Raw, get("teamAdminHash"))) { error = "Den Discord-Webhook darf nur der Admin ändern."; Refresh(); return; }
            var u = hook.Text?.Trim() ?? "";
            if (u.Length > 0 && !System.Text.RegularExpressions.Regex.IsMatch(u, @"^https://(?:(?:ptb|canary)\.)?discord(?:app)?\.com/api/webhooks/\d+/[\w-]+$")) { error = "Das ist keine Discord-Webhook-Adresse."; Refresh(); return; }
            try { await Team.Update(Db, Group, new JsonObject { ["discord/webhook"] = u.Length > 0 ? u : null }); } catch (Exception e) { error = e.Message; }
            await Reload();
        }));
        sp.Children.Add(new Border { Height = 8 });
        sp.Children.Add(UI.H("Verlauf"));
        if (Last.Log.Count == 0) sp.Children.Add(UI.Note("Noch nichts passiert."));
        foreach (var (at, who, text) in Last.Log.Take(25))
            sp.Children.Add(new StackPanel { Margin = new Thickness(0, 2), Children = { new TextBlock { Text = (who.Length > 0 ? who + " " : "") + text, TextWrapping = TextWrapping.Wrap }, UI.Note(Ago(at)) } });
        return sp;
    }

    Control Right()
    {
        var (p, pl) = ctx();
        var sp = new StackPanel { Spacing = 12 };
        sp.Children.Add(UI.Title("Team-Aufgaben"));
        if (!Connected || Last == null)
        {
            sp.Children.Add(UI.Note("Verbinde links eure Gruppe. Dann kannst du Etappen, Baupläne oder Ebenen als Aufgaben ans Team schicken und siehst hier, was im Spiel schon gebaut ist."));
            return sp;
        }
        var linked = p?.Extra["team"] is JsonObject t && (string?)t["gruppe"] == Group ? (string?)t["projekt"] : null;
        if (selProject == null || !Last.Projects.ContainsKey(selProject)) selProject = linked != null && Last.Projects.ContainsKey(linked) ? linked : Last.Projects.Keys.LastOrDefault();

        var send = UI.Btn(p == null ? "Kein Projekt geöffnet" : $"„{p.Name}“ als Aufgaben senden …", async () =>
        {
            var (pp, ppl) = ctx(); if (pp == null || ppl == null) return;
            if (Me.Length == 0) { error = "Bitte links deinen Namen eintragen und speichern."; Refresh(); return; }
            bool admin = Last == null || Team.IsAdmin(Last.Raw, get("teamAdminHash"));
            // ohne Admin-Rechte nur sich selbst einteilen (oder niemanden = alle)
            var people = admin ? Last?.Players ?? new() : new List<string> { Me };
            var d = new SendDialog(pp, ppl, Db, Group, Me, people);
            if (TopLevel.GetTopLevel(this) is Window w) await d.ShowDialog(w);
            if (d.Sent) { saveProject(); selProject = null; await Reload(); }
        }, true);
        send.IsEnabled = p != null;
        var reload = UI.Btn("Aktualisieren", async () => await Reload());
        sp.Children.Add(UI.Row(send, reload));

        if (Last.Projects.Count == 0) { sp.Children.Add(UI.Note("In der Gruppe gibt es noch keine Projekte.")); return sp; }
        var ids = Last.Projects.Keys.ToList();
        var sel = new ComboBox { ItemsSource = ids.Select(i => Last.Projects[i] + (i == linked ? "  (aus diesem Projekt)" : "")).ToList(), SelectedIndex = Math.Max(0, ids.IndexOf(selProject ?? "")), MinWidth = 320 };
        sel.SelectionChanged += (_, _) => { if (sel.SelectedIndex >= 0) { selProject = ids[sel.SelectedIndex]; Refresh(); } };
        sp.Children.Add(UI.Row(new TextBlock { Text = "Projekt", VerticalAlignment = VerticalAlignment.Center }, sel));

        var tasks = Last.TopTasks(selProject ?? "").ToList();
        int done = tasks.Count(x => x.Status == "fertig");
        int avg = tasks.Count == 0 ? 0 : (int)tasks.Average(x => Last.Percent(x));
        sp.Children.Add(UI.Note($"{tasks.Count} Aufgaben · {done} fertig · insgesamt {avg} %"));
        if (tasks.Count == 0) sp.Children.Add(UI.Note("Keine Aufgaben in diesem Projekt."));
        foreach (var task in tasks) sp.Children.Add(TaskRow(task));
        return sp;
    }

    Control TaskRow(Team.TaskView t)
    {
        int pc = Last!.Percent(t); bool locked = Last.Locked(t);
        var g = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,160,Auto"), RowDefinitions = new RowDefinitions("Auto,Auto"), Margin = new Thickness(0, 4) };
        string icon = t.Status switch { "fertig" => "✓", "arbeit" => "◐", _ => locked ? "🔒" : "○" };
        var st = new Button { Content = icon, Width = 34, Padding = new Thickness(0, 2), HorizontalContentAlignment = HorizontalAlignment.Center };
        ToolTip.SetTip(st, "Status ändern (Offen → In Arbeit → Fertig)");
        st.Click += async (_, _) =>
        {
            var next = t.Status switch { "offen" => "arbeit", "arbeit" => "fertig", _ => "offen" };
            try
            {
                await Team.Update(Db, Group, new JsonObject { [$"aufgaben/{t.Id}/status"] = next });
                await Team.Log(Db, Group, Me, next switch { "fertig" => $"hat \"{t.Title}\" erledigt", "arbeit" => $"arbeitet an \"{t.Title}\"", _ => $"hat \"{t.Title}\" wieder geöffnet" });
            }
            catch (Exception e) { error = e.Message; }
            await Reload();
        };
        g.Children.Add(st);
        var title = new TextBlock { Text = t.Title, FontSize = 14.5, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0), TextWrapping = TextWrapping.Wrap, Opacity = t.Status == "fertig" ? .6 : 1 };
        Grid.SetColumn(title, 1); g.Children.Add(title);
        var bar = new ProgressBar { Minimum = 0, Maximum = 100, Value = pc, Height = 8, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(bar, 2); g.Children.Add(bar);
        var pt = new TextBlock { Text = $"{pc} %", Width = 52, Margin = new Thickness(8, 0, 0, 0), TextAlignment = TextAlignment.Right, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(pt, 3); g.Children.Add(pt);
        var goals = new List<Team.TaskView>(); if (t.HasGoal) goals.Add(t); goals.AddRange(t.Children.Where(c => c.HasGoal));
        string goalText = goals.Count switch
        {
            0 => "",
            1 => $"{goals[0].GoalTotal(Last.Storage):N0} / {goals[0].Goal:N0} {Team.Label(goals[0].Item)}",
            _ => $"{goals.Count} Ziele, {goals.Sum(c => Math.Min(c.Goal, c.GoalTotal(Last.Storage))):N0} / {goals.Sum(c => c.Goal):N0} Blöcke",
        };
        var lockText = locked && Last.Tasks.TryGetValue(t.After, out var before) ? $"erst nach „{before.Title}“" : "";
        var meta = UI.Note(string.Join(" · ", new[] { t.Who.Length > 0 ? t.Who.Replace(",", ", ") : "alle", goalText, lockText }.Where(x => x.Length > 0)));
        meta.Margin = new Thickness(8, 0); meta.TextWrapping = TextWrapping.Wrap;
        Grid.SetColumn(meta, 1); Grid.SetRow(meta, 1); Grid.SetColumnSpan(meta, 3); g.Children.Add(meta);
        return new Border { BorderBrush = UI.Res("LineBrush"), BorderThickness = new Thickness(0, 0, 0, 1), Padding = new Thickness(0, 4), Child = g };
    }
}
