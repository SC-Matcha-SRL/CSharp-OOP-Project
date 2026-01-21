using System;
using System.Collections.Generic;
using System.IO;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleApp5
{
    public static class EcranStart
    {
        public static string AfiseazaEcranStartSiAlegeRol(SistemMatcha sistem)
        {
            string ascii = IncarcaAsciiArtStart();
            int lastW = -1, lastH = -1;

            while (true)
            {
                int w = Console.WindowWidth;
                int h = Console.WindowHeight;

                if (w != lastW || h != lastH)
                {
                    lastW = w;
                    lastH = h;
                    RandareEcranStart(sistem, ascii);
                }

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1) return "Client";
                    if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2) return "Administrator";
                    if (key.Key == ConsoleKey.D3 || key.Key == ConsoleKey.NumPad3) return "CreareCont";
                    if (key.Key == ConsoleKey.D4 || key.Key == ConsoleKey.NumPad4 || key.Key == ConsoleKey.Escape) return "Iesire";

                    if (key.Key == ConsoleKey.R)
                    {
                        lastW = -1;
                        lastH = -1;
                    }
                }

                System.Threading.Thread.Sleep(60);
            }
        }

        private static void RandareEcranStart(SistemMatcha sistem, string asciiArt)
        {
            AnsiConsole.Clear();

            int w = AnsiConsole.Profile.Width;
            int h = AnsiConsole.Profile.Height;

            if (w < 90 || h < 26)
            {
                AnsiConsole.MarkupLine("[bold green]X Matcha[/]");
                AnsiConsole.MarkupLine("[grey]1) Client  2) Admin  3) Create account  4) Exit (ESC)[/]");
                AnsiConsole.MarkupLine("[grey](Increase window size for full UI)[/]");
                return;
            }

            var top = new Grid();
            top.AddColumn(new GridColumn().LeftAligned());
            top.AddColumn(new GridColumn().Centered());
            top.AddColumn(new GridColumn().RightAligned());
            top.AddRow(
                new Markup("[bold green]Home[/] [grey]|[/] Stats [grey]|[/] Reviews [grey]|[/] Start"),
                new Markup("[grey]X Matcha v1.0[/]"),
                new Markup($"[grey]{DateTime.Now:HH:mm}[/]")
            );

            AnsiConsole.Write(new Panel(top).Border(BoxBorder.Double).BorderColor(Color.Green).Expand());

            if (h >= 32)
                AnsiConsole.Write(Align.Center(new FigletText("X Matcha").Color(Color.Green)));
            else
                AnsiConsole.Write(new Panel(Align.Center(new Markup("[bold green]X Matcha[/]"))).Border(BoxBorder.Double).BorderColor(Color.Green).Expand());

            var subtitle = new Panel(
                new Rows(
                    Align.Center(new Markup("[green]Your matcha network in one place[/]")),
                    Align.Center(new Markup("[grey]Clients, admins and matcheries - together.[/]"))
                ))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();

            AnsiConsole.Write(subtitle);

            int cardLines = Math.Clamp(h - 24, 10, 18);

            if (w >= 140)
            {
                int colW = Math.Max(34, (w - 10) / 3);

                var stats = ConstruiestePanelStatistici(sistem, colW);
                var reviews = ConstruiestePanelReviews();
                var art = ConstruiestePanelAscii(asciiArt, colW);

                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(stats, reviews, art);

                AnsiConsole.Write(grid);
            }
            else if (w >= 105)
            {
                int colW = Math.Max(40, (w - 8) / 2);

                var stats = ConstruiestePanelStatistici(sistem, colW);
                var art = ConstruiestePanelAscii(asciiArt, colW);

                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(stats, art);

                AnsiConsole.Write(grid);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(ConstruiestePanelReviews());
            }
            else
            {
                AnsiConsole.Write(ConstruiestePanelStatistici(sistem, w));
                AnsiConsole.WriteLine();
                AnsiConsole.Write(ConstruiestePanelReviews());
                AnsiConsole.WriteLine();
                AnsiConsole.Write(ConstruiestePanelAscii(asciiArt, w));
            }

            AnsiConsole.WriteLine();
            AnsiConsole.Write(
                new Panel(new Markup(
                        "[black on green]   1  CLIENT                [/]\n" +
                        "[black on green]   2  ADMIN                 [/]\n" +
                        "[black on green]   3  CREATE ACCOUNT         [/]\n\n" +
                        "[white on darkred]   4  EXIT (or ESC)          [/] \n\n" +
                        $"[grey]{UiText.StartHint}[/]"
                ))
                .Header("[bold green]Start[/]")
                .Border(BoxBorder.Double)
                .BorderColor(Color.Green)
                .Expand()
            );
        }

        private static IRenderable ConstruiestePanelStatistici(SistemMatcha sistem, int colWidth)
        {
            int nrMagazine = sistem.Magazine?.Count ?? 0;
            int nrClienti = sistem.Clienti?.Count ?? 0;
            int nrAdmins = sistem.Administratori?.Count ?? 0;

            int totalProduse = 0, totalRezervari = 0;
            if (sistem.Magazine != null)
            {
                foreach (var m in sistem.Magazine)
                {
                    totalProduse += (m.Meniu?.Count ?? 0);
                    totalRezervari += (m.Rezervari?.Count ?? 0);
                }
            }

            int totalTranzactii = 0;
            if (sistem.Clienti != null)
                foreach (var c in sistem.Clienti)
                    totalTranzactii += (c.Istoric?.Count ?? 0);

            int chartW = Math.Clamp(colWidth - 10, 22, 60);

            var rows = new List<IRenderable>
            {
                new Markup($"[bold]Matcheries:[/] {nrMagazine}   [bold]Clients:[/] {nrClienti}   [bold]Admins:[/] {nrAdmins}"),
                new Markup($"[bold]Products:[/] {totalProduse}   [bold]Reservations:[/] {totalRezervari}   [bold]Transactions:[/] {totalTranzactii}"),
                new Rule("[green]Activity breakdown[/]")
            };

            int sum = totalProduse + totalRezervari + totalTranzactii;
            if (sum <= 0)
            {
                rows.Add(new Markup($"[grey]{UiText.NoData}[/]"));
            }
            else
            {
                var b = new BreakdownChart()
                    .Width(chartW)
                    .AddItem("Products", totalProduse, Color.Aqua)
                    .AddItem("Reservations", totalRezervari, Color.Yellow)
                    .AddItem("Transactions", totalTranzactii, Color.Magenta1);

                rows.Add(b);
            }

            rows.Add(new Rule("[green]Top matcheries[/]"));
            var top = ServiciiStatistica.GetTopMatcheriiByRezervari(sistem, 3);
            if (top.Count == 0) rows.Add(new Markup("[grey]N/A[/]"));
            else
                foreach (var x in top)
                    rows.Add(new Markup($"[white]{Markup.Escape(x.nume)}[/]  [green]{x.val}[/]"));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Stats[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        private static IRenderable ConstruiestePanelReviews()
        {
            var reviews = new[]
            {
                ("Alex B.", 5, "Great matcha, premium service, very friendly staff."),
                ("Mara D.", 4, "Cozy atmosphere, varied menu, latte is top."),
                ("Radu P.", 5, "Fast reservations and quick orders. Consistent quality."),
                ("Ioana S.", 4, "Good prices and I love seeing everything in one place.")
            };

            var rows = new List<IRenderable> { new Markup("[grey]Latest reviews[/]") };

            foreach (var r in reviews)
            {
                string stars = new string('★', r.Item2) + new string('☆', 5 - r.Item2);
                rows.Add(new Markup($"[yellow]{Markup.Escape(stars)}[/] [green]{Markup.Escape(r.Item1)}[/] [grey]-[/] [white]{Markup.Escape(r.Item3)}[/]"));
            }

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold green]Reviews[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        private static IRenderable ConstruiestePanelAscii(string asciiArt, int colWidth)
        {
            asciiArt ??= "";
            string normalized = asciiArt.Replace("\r", "").TrimEnd('\n');

            int safeWidth = Math.Max(20, colWidth - 8);
            int maxLine = 0;
            foreach (var line in normalized.Split('\n'))
                if (line.Length > maxLine) maxLine = line.Length;

            string body = (maxLine > safeWidth)
                ? "ASCII art too wide.\nResize the window or use a narrower art."
                : normalized;

            return new Panel(Align.Center(new Text(body)))
                .Header("[bold green]Art[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();
        }

        private static string IncarcaAsciiArtStart()
        {
            string fallback = """
                                                                     
                                               +++                 
                                             ++**                  
                                           +*+                   
                                         +#+                       
                                        =*+                        
                                     ...=*...                      
                                  ......=+......                   
                                 ... ...+=........                 
                               ...   ...*=::.....:                 
                               .. ...::-#+::.:-:.:.                
                              .::...::-===--=---::-:               
                              .:....:--=====-----=:.               
                               ::---==++******++*=                 
                               ::--=++#**####****+                 
                               ::--==++*#####***+=                 
                               ::---=+**#####***+=                 
                                ::--==+*####****+=                 
                                :::-==+***#****++                  
                                .::--==+++**++++=                  
                                ..:::--=++++=+==-                  
                                 ....=:-==#+==--:                  
                                 .....:::--=----:                  
                                 .....:--:----:::                  
                                 ......::::::::-:                  
                                 ......::::::---                   
                                 ......:::::---=                   
                                   ....:---===+                    
                                                                   
                              """;

            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "assets", "start_art.txt");
                if (File.Exists(path))
                    return File.ReadAllText(path);
            }
            catch { }

            return fallback;
        }
    }
}
