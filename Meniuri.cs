using Spectre.Console;
using System;
using System.Linq;
using System.Text;

namespace ConsoleApp5
{
    public static class Meniuri
    {
        public static void AfiseazaDashboardClient(ClientAccount client, SistemMatcha sistem)
        {
            Console.Clear();

            int w = AnsiConsole.Profile.Width;
            int h = AnsiConsole.Profile.Height;

            // Ca să rămână loc pentru prompt-ul de opțiuni, limităm nr. de matcherii afișate
            // (produsele sunt toate vizibile în celula “Meniu complet”).
            int maxMatcherii = h < 30 ? 2 : (h < 38 ? 3 : 5);

            // -------------------- STÂNGA: matcherii + TOATE produsele per matcherie --------------------
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]🍵 MATCHERII & MENIURI[/]");

            t.AddColumn("Locație");
            t.AddColumn("Program");
            t.AddColumn(new TableColumn("Locuri libere").RightAligned());
            t.AddColumn("Meniu complet");

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                t.AddRow("[red]N/A[/]", "[red]N/A[/]", "-", "[grey]Nu există matcherii în sistem[/]");
            }
            else
            {
                var list = sistem.Magazine
                    .OrderBy(m => m.Nume)
                    .Take(maxMatcherii)
                    .ToList();

                foreach (var m in list)
                {
                    int rez = m.Rezervari?.Count ?? 0;
                    int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                    int libere = Math.Max(0, cap - rez);

                    string locuriCell = libere > 0
                        ? $"[green]{libere}/{cap}[/]"
                        : $"[red]{libere}/{cap}[/]";

                    string meniuCell = BuildMeniuCompletCell(m);

                    t.AddRow(
                        $"[white]{Markup.Escape(m.Nume)}[/]",
                        $"[grey]{Markup.Escape(m.Program)}[/]",
                        locuriCell,
                        meniuCell
                    );
                }

                if (sistem.Magazine.Count > maxMatcherii)
                {
                    t.AddRow(
                        "[grey]…[/]",
                        $"[grey](mai multe locații)[/]",
                        "[grey]…[/]",
                        $"[grey]Afișate {maxMatcherii} din {sistem.Magazine.Count} (mărește fereastra pentru mai mult)[/]"
                    );
                }
            }

            var leftPanel = new Panel(t)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("[bold green]Rețea[/]")
                .Expand();

            // -------------------- DREAPTA: profil --------------------
            int rezCount = client.Rezervari?.Count ?? 0;
            int ordersCount = client.Istoric?.Count ?? 0;

            var profil = new Rows(
                new Markup($"[bold]Utilizator:[/] {Markup.Escape(client.Nume)}"),
                new Markup($"[bold]Email:[/] [blue]{Markup.Escape(client.Email)}[/]"),
                new Rule("[yellow]Activitate[/]"),
                new Markup($"[bold]Rezervări:[/] [yellow]{rezCount}[/]"),
                new Markup($"[bold]Comenzi:[/] [green]{ordersCount}[/]"),
                new Rule(),
                new Markup("[grey]Alege o opțiune din meniu (mai jos)[/]")
            );

            var rightPanel = new Panel(profil)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Header("[bold cyan]👤 Profil[/]")
                .Expand();

            // -------------------- RENDER (Grid, nu Layout) --------------------
            if (w >= 110)
            {
                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(leftPanel, rightPanel);
                AnsiConsole.Write(grid);

            }
            else
            {
                AnsiConsole.Write(leftPanel);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(rightPanel);
            }

            AnsiConsole.WriteLine();
        }

        private static string BuildMeniuCompletCell(Matcherie m)
        {
            if (m.Meniu == null || m.Meniu.Count == 0)
                return "[grey italic]În curând... (meniu indisponibil)[/]";

            // TOATE produsele, multi-line în aceeași celulă
            var sb = new StringBuilder();

            foreach (var p in m.Meniu)
            {
                // format compact: nume + pret + stoc + kcal (fără descriere ca să rămână “tight”)
                sb.Append("[green]•[/] ");
                sb.Append(Markup.Escape(p.nume));
                sb.Append($" [grey]({p.pret} RON)[/]");
                sb.Append($" [grey]| stoc {p.cantitate}[/]");
                sb.Append($" [grey]| {p.calorii} kcal[/]");
                sb.Append('\n');
            }

            return sb.ToString().TrimEnd('\n');
        }
    }
}
