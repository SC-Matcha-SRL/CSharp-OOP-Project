using System;
using System.Linq;
using System.Text;
using Spectre.Console;

namespace ConsoleApp5
{
    public static class Meniuri
    {
        public static void AfiseazaDashboardClient(ContClient client, SistemMatcha sistem)
        {
            AnsiConsole.Clear();

            int w = AnsiConsole.Profile.Width;
            int h = AnsiConsole.Profile.Height;
            int maxMatcherii = h < 30 ? 2 : (h < 38 ? 3 : 5);

            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title("[bold green]🍵 MATCHERIES & MENUS[/]");

            t.AddColumn("Location");
            t.AddColumn("Schedule");
            t.AddColumn(new TableColumn("Free seats").RightAligned());
            t.AddColumn("Full menu");

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                t.AddRow("[red]N/A[/]", "[red]N/A[/]", "-", "[grey]No matcheries available[/]");
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
                    int free = Math.Max(0, cap - rez);

                    string seatsCell = free > 0 ? $"[green]{free}/{cap}[/]" : $"[red]{free}/{cap}[/]";
                    string menuCell = BuildMeniuCompletCell(m);

                    t.AddRow(
                        $"[white]{Markup.Escape(m.Nume)}[/]",
                        $"[grey]{Markup.Escape(m.Program)}[/]",
                        seatsCell,
                        menuCell
                    );
                }

                if (sistem.Magazine.Count > maxMatcherii)
                {
                    t.AddRow(
                        "[grey]...[/]",
                        "[grey](more locations)[/]",
                        "[grey]...[/]",
                        $"[grey]Showing {maxMatcherii} / {sistem.Magazine.Count} (increase window size)[/]"
                    );
                }
            }

            var leftPanel = new Panel(t)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("[bold green]Network[/]")
                .Expand();

            int rezCount = client.Rezervari?.Count ?? 0;
            int ordersCount = client.Istoric?.Count ?? 0;

            var profil = new Rows(
                new Markup($"[bold]User:[/] {Markup.Escape(client.Nume)}"),
                new Markup($"[bold]Email:[/] [blue]{Markup.Escape(client.Email)}[/]"),
                new Rule("[yellow]Activity[/]"),
                new Markup($"[bold]Reservations:[/] [yellow]{rezCount}[/]"),
                new Markup($"[bold]Orders:[/] [green]{ordersCount}[/]"),
                new Rule(),
                new Markup("[grey]Choose an option below[/]")
            );

            var rightPanel = new Panel(profil)
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Header("[bold cyan]👤 Profile[/]")
                .Expand();

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

        public static Table ConstruesteTabelMeniu(Matcherie m)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title($"[bold white on green] MENU - {Markup.Escape(m.Nume)} [/]");

            table.AddColumn("[bold]Product[/]");
            table.AddColumn("[bold]Description[/]");
            table.AddColumn(new TableColumn("[bold]Price[/]").RightAligned());
            table.AddColumn(new TableColumn("[bold]Stock[/]").RightAligned());
            table.AddColumn(new TableColumn("[bold]Kcal[/]").RightAligned());

            if (m.Meniu == null || m.Meniu.Count == 0)
            {
                table.AddRow("[grey]—[/]", "[grey]Menu is empty[/]", "-", "-", "-");
                return table;
            }

            foreach (var p in m.Meniu)
            {
                table.AddRow(
                    Markup.Escape(p.Nume),
                    Markup.Escape(p.Descriere ?? ""),
                    $"{p.Pret} RON",
                    p.Cantitate.ToString(),
                    p.Calorii.ToString()
                );
            }

            return table;
        }

        private static string BuildMeniuCompletCell(Matcherie m)
        {
            if (m.Meniu == null || m.Meniu.Count == 0)
                return "[grey italic]Coming soon... (menu unavailable)[/]";

            var sb = new StringBuilder();

            foreach (var p in m.Meniu)
            {
                sb.Append("[green]•[/] ");
                sb.Append(Markup.Escape(p.Nume));
                sb.Append($" [grey]({p.Pret} RON)[/]");
                sb.Append($" [grey]| stock {p.Cantitate}[/]");
                sb.Append($" [grey]| {p.Calorii} kcal[/]");
                sb.Append('\n');
            }

            return sb.ToString().TrimEnd('\n');
        }
       
        /// Construieste un tabel Spectre.Console pentru meniul unei matcherii.
        /// UI in engleza (pentru user).
        /// </summary>
        public static Table ConstruiesteTabelMeniu(Matcherie matcherie)
        {
            var t = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Green)
                .Title($"[bold green]Menu[/] - [white]{Markup.Escape(matcherie?.Nume ?? "N/A")}[/]");

            t.AddColumn("[bold]Product[/]");
            t.AddColumn("[bold]Description[/]");
            t.AddColumn(new TableColumn("[bold]Price[/]").RightAligned());
            t.AddColumn(new TableColumn("[bold]Stock[/]").RightAligned());
            t.AddColumn(new TableColumn("[bold]Kcal[/]").RightAligned());

            if (matcherie == null || matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                t.AddRow("[grey]—[/]", "[grey]Menu is empty.[/]", "-", "-", "-");
                return t;
            }

            foreach (var p in matcherie.Meniu)
            {
                t.AddRow(
                    Markup.Escape(p.Nume ?? ""),
                    Markup.Escape(p.Descriere ?? ""),
                    $"{p.Pret} RON",
                    p.Cantitate.ToString(),
                    p.Calorii.ToString()
                );
            }

            return t;
        }
    }
}
