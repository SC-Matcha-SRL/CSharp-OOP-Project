using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleApp5
{
    public static class FluxAdmin
    {
        public static void Run(SistemMatcha sistem)
        {
            var admin = AutentificareAdmin(sistem);
            if (admin == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                AfiseazaDashboardAdmin(admin, sistem);

                string opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Admin actions[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                            "View matcheries & menus",
                            "Add product to matchery",
                            "Edit matchery (schedule/capacity)",
                            "Manage reservation types",
                            "View reservations",
                            "Create admin account",
                            "Logout"
                        }));

                switch (opt)
                {
                    case "View matcheries & menus":
                        VizualizeazaMatcheriiSiMeniuri(sistem);
                        break;

                    case "Add product to matchery":
                        AdaugaProdusInMatcherie(sistem);
                        break;

                    case "Edit matchery (schedule/capacity)":
                        EditeazaMatcherie(sistem);
                        break;

                    case "Manage reservation types":
                        GestioneazaTipuriRezervare(sistem);
                        break;

                    case "View reservations":
                        VizualizeazaRezervari(sistem);
                        break;

                    case "Create admin account":
                        ServiciiCont.CreeazaAdminNou(sistem);
                        break;

                    case "Logout":
                        UIComun.SalvareSistem(sistem);
                        inapoi = true;
                        break;
                }
            }
        }

        // -------------------------
        // AUTH
        // -------------------------
        private static ContAdmin? AutentificareAdmin(SistemMatcha sistem)
        {
            sistem.AsiguraColectii();

            if (sistem.Administratori.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No admin accounts exist. Seeding demo admins...[/]");
                SeedDateTest.IncarcaDateTest(sistem);
                UIComun.SalvareSistem(sistem);
            }

            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[green]Admin login[/]").RuleStyle("grey"));
                AnsiConsole.MarkupLine("[grey]Tip '0' as Admin ID to go back.[/]\n");

                string id = AnsiConsole.Ask<string>("Admin ID:");
                if (id == "0") return null;

                string parola = AnsiConsole.Prompt(new TextPrompt<string>("Password:").Secret());

                var admin = sistem.Administratori.FirstOrDefault(a =>
                    string.Equals(a.AdminId, id, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(a.Parola, parola, StringComparison.Ordinal));

                if (admin != null)
                {
                    AnsiConsole.MarkupLine("[green]Login successful.[/]");
                    System.Threading.Thread.Sleep(300);
                    return admin;
                }

                AnsiConsole.MarkupLine("[red]Invalid credentials.[/]");
                if (!AnsiConsole.Confirm("Try again?"))
                    return null;
            }
        }

        // -------------------------
        // DASHBOARD
        // -------------------------
        private static void AfiseazaDashboardAdmin(ContAdmin admin, SistemMatcha sistem)
        {
            AnsiConsole.Clear();

            int nrMatcherii = sistem.Magazine?.Count ?? 0;
            int nrClienti = sistem.Clienti?.Count ?? 0;

            int totalProduse = 0, totalRezervari = 0;
            if (sistem.Magazine != null)
            {
                foreach (var m in sistem.Magazine)
                {
                    totalProduse += (m.Meniu?.Count ?? 0);
                    totalRezervari += (m.Rezervari?.Count ?? 0);
                }
            }

            var header = new Grid();
            header.AddColumn(new GridColumn().LeftAligned());
            header.AddColumn(new GridColumn().Centered());
            header.AddColumn(new GridColumn().RightAligned());

            header.AddRow(
                new Markup("[bold green]Admin Panel[/]"),
                new Markup("[grey]X Matcha[/]"),
                new Markup($"[grey]{DateTime.Now:HH:mm}[/]")
            );

            AnsiConsole.Write(new Panel(header).Border(BoxBorder.Double).BorderColor(Color.Green).Expand());
            AnsiConsole.WriteLine();

            var left = new Panel(new Rows(
                    new Markup($"[bold]Admin:[/] {Markup.Escape(admin.Nume)}"),
                    new Markup($"[bold]ID:[/] [cyan]{Markup.Escape(admin.AdminId)}[/]"),
                    new Rule("[green]System[/]"),
                    new Markup($"[bold]Matcheries:[/] [yellow]{nrMatcherii}[/]"),
                    new Markup($"[bold]Clients:[/] [yellow]{nrClienti}[/]"),
                    new Markup($"[bold]Products:[/] [green]{totalProduse}[/]"),
                    new Markup($"[bold]Reservations:[/] [orange1]{totalRezervari}[/]")
                ))
                .Header("[bold green]Overview[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Expand();

            var right = ConstruiestePanelTopMatcherii(sistem);

            int w = AnsiConsole.Profile.Width;
            if (w >= 110)
            {
                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(left, right);
                AnsiConsole.Write(grid);
            }
            else
            {
                AnsiConsole.Write(left);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(right);
            }

            AnsiConsole.WriteLine();
        }

        private static Panel ConstruiestePanelTopMatcherii(SistemMatcha sistem)
        {
            var top = ServiciiStatistica.GetTopMatcheriiByRezervari(sistem, 5);

            var rows = new List<IRenderable>
            {
                new Markup("[grey]Top by reservations[/]"),
                new Rule()
            };

            if (top.Count == 0)
            {
                rows.Add(new Markup($"[grey]{UiText.NoData}[/]"));
            }
            else
            {
                foreach (var x in top)
                    rows.Add(new Markup($"[white]{Markup.Escape(x.nume)}[/]  [orange1]{x.val}[/]"));
            }

            rows.Add(new Rule());
            rows.Add(new Markup($"[grey]Transactions today:[/] [magenta]{ServiciiStatistica.GetTranzactiiInZi(sistem, DateTime.Now)}[/]"));

            return new Panel(new Rows(rows.ToArray()))
                .Header("[bold cyan]Stats[/]")
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Cyan1)
                .Expand();
        }

        // -------------------------
        // VIEW MATCHERIES
        // -------------------------
        private static void VizualizeazaMatcheriiSiMeniuri(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            while (true)
            {
                var matcherie = AlegeMatcherie(sistem, "Select a matchery to view:");
                if (matcherie == null) return;

                AnsiConsole.Clear();

                int rez = matcherie.Rezervari?.Count ?? 0;
                int cap = matcherie.Capacitate <= 0 ? 0 : matcherie.Capacitate;
                int free = cap <= 0 ? 0 : Math.Max(0, cap - rez);

                var info = new Panel(new Rows(
                        new Markup($"[bold]Matchery:[/] [green]{Markup.Escape(matcherie.Nume)}[/]"),
                        new Markup($"[bold]Schedule:[/] [grey]{Markup.Escape(matcherie.Program)}[/]"),
                        new Markup($"[bold]Capacity:[/] {cap}  [bold]Occupied:[/] {rez}  [bold]Free:[/] {(free > 0 ? $"[green]{free}[/]" : $"[red]{free}[/]")}")
                    ))
                    .Header("[bold green]Details[/]")
                    .BorderColor(Color.Green)
                    .Border(BoxBorder.Rounded)
                    .Expand();

                AnsiConsole.Write(info);
                AnsiConsole.WriteLine();

                AnsiConsole.Write(Meniuri.ConstruiesteTabelMeniu(matcherie));
                AnsiConsole.WriteLine();

                if (!AnsiConsole.Confirm("View another matchery?"))
                    return;
            }
        }

        // -------------------------
        // ADD PRODUCT
        // -------------------------
        private static void AdaugaProdusInMatcherie(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            var matcherie = AlegeMatcherie(sistem, "Select matchery to add a product:");
            if (matcherie == null) return;

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Add product[/] [grey]→[/] {Markup.Escape(matcherie.Nume)}").RuleStyle("grey"));

            string nume = CereTextObligatoriu("Product name:");
            string descriere = AnsiConsole.Ask<string>("Description:");

            decimal pret = CereDecimal("Price (RON):", min: 0.01m, max: 99999m);
            int cant = CereInt("Stock quantity:", min: 0, max: 100000);
            int kcal = CereInt("Calories (kcal):", min: 0, max: 100000);

            var produs = new Matcha(nume, descriere, pret, cant, kcal);

            var coord = LocatieServicii.Get<CoordonatorSistem>();
            bool ok = coord.TryAdaugaProdus(matcherie, produs, out string mesaj);

            if (!ok)
            {
                AnsiConsole.MarkupLine($"[red]{Markup.Escape(mesaj)}[/]");
                UIComun.Pauza();
                return;
            }

            UIComun.SalvareSistem(sistem);
            AnsiConsole.MarkupLine($"[green]{Markup.Escape(mesaj)}[/]");
            UIComun.Pauza();
        }

        // -------------------------
        // EDIT MATCHERY
        // -------------------------
        private static void EditeazaMatcherie(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            var matcherie = AlegeMatcherie(sistem, "Select matchery to edit:");
            if (matcherie == null) return;

            bool back = false;
            while (!back)
            {
                AnsiConsole.Clear();

                var panel = new Panel(new Rows(
                        new Markup($"[bold]Matchery:[/] [green]{Markup.Escape(matcherie.Nume)}[/]"),
                        new Markup($"[bold]Schedule:[/] [grey]{Markup.Escape(matcherie.Program)}[/]"),
                        new Markup($"[bold]Capacity:[/] {matcherie.Capacitate}")
                    ))
                    .Header("[bold green]Edit matchery[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green)
                    .Expand();

                AnsiConsole.Write(panel);
                AnsiConsole.WriteLine();

                string opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose what to edit:")
                        .AddChoices(new[]
                        {
                            "Change schedule",
                            "Change capacity",
                            "Back"
                        }));

                switch (opt)
                {
                    case "Change schedule":
                        {
                            string nou = CereTextObligatoriu("New schedule (e.g., 09:00-22:00):");
                            matcherie.SetProgram(nou);
                            UIComun.SalvareSistem(sistem);
                            AnsiConsole.MarkupLine("[green]Schedule updated.[/]");
                            UIComun.Pauza();
                            break;
                        }
                    case "Change capacity":
                        {
                            int cap = CereInt("New capacity:", min: 1, max: 10000);
                            matcherie.SetCapacitate(cap);
                            UIComun.SalvareSistem(sistem);
                            AnsiConsole.MarkupLine("[green]Capacity updated.[/]");
                            UIComun.Pauza();
                            break;
                        }
                    case "Back":
                        back = true;
                        break;
                }
            }
        }

        // -------------------------
        // RESERVATION TYPES
        // -------------------------
        private static void GestioneazaTipuriRezervare(SistemMatcha sistem)
        {
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            bool back = false;
            while (!back)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[green]Reservation types[/]").RuleStyle("grey"));
                AnsiConsole.WriteLine();

                AnsiConsole.Write(ConstruiesteTabelTipuriRezervare(sistem));
                AnsiConsole.WriteLine();

                string opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Choose action[/]")
                        .AddChoices(new[]
                        {
                            "Add type",
                            "Edit type",
                            "Delete type",
                            "Back"
                        }));

                switch (opt)
                {
                    case "Add type":
                        AdaugaTipRezervare(sistem);
                        break;

                    case "Edit type":
                        EditeazaTipRezervare(sistem);
                        break;

                    case "Delete type":
                        StergeTipRezervare(sistem);
                        break;

                    case "Back":
                        back = true;
                        break;
                }
            }
        }

        private static Table ConstruiesteTabelTipuriRezervare(SistemMatcha sistem)
        {
            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Title("[bold orange1]Reservation types[/]");

            table.AddColumn("[bold]Name[/]");
            table.AddColumn(new TableColumn("[bold]Price[/]").RightAligned());
            table.AddColumn("[bold]Limitations[/]");
            table.AddColumn("[bold]Benefits[/]");

            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                table.AddRow("[grey]—[/]", "-", "[grey]No types available[/]", "[grey]—[/]");
                return table;
            }

            foreach (var t in sistem.TipuriRezervari.OrderBy(x => x.Nume))
            {
                table.AddRow(
                    Markup.Escape(t.Nume),
                    $"{t.Pret} RON",
                    Markup.Escape(t.Limitari ?? ""),
                    Markup.Escape(t.Beneficii ?? "")
                );
            }

            return table;
        }

        private static void AdaugaTipRezervare(SistemMatcha sistem)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[green]Add reservation type[/]").RuleStyle("grey"));

            string nume = CereTextObligatoriu("Type name:");
            if (sistem.TipuriRezervari.Any(x => string.Equals(x.Nume, nume, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]A type with this name already exists.[/]");
                UIComun.Pauza();
                return;
            }

            decimal pret = CereDecimal("Price (RON):", min: 0.01m, max: 99999m);
            string limitari = AnsiConsole.Ask<string>("Limitations:");
            string beneficii = AnsiConsole.Ask<string>("Benefits:");

            sistem.TipuriRezervari.Add(new TipRezervare(nume, pret, limitari, beneficii));
            UIComun.SalvareSistem(sistem);

            AnsiConsole.MarkupLine("[green]Reservation type added.[/]");
            UIComun.Pauza();
        }

        private static void EditeazaTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No reservation types to edit.[/]");
                UIComun.Pauza();
                return;
            }

            var tip = AlegeTipRezervare(sistem, "Select a type to edit:");
            if (tip == null) return;

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Edit type[/] [grey]→[/] {Markup.Escape(tip.Nume)}").RuleStyle("grey"));

            string numeNou = CereTextObligatoriu($"New name [grey](current: {Markup.Escape(tip.Nume)})[/]:");

            if (!string.Equals(numeNou, tip.Nume, StringComparison.OrdinalIgnoreCase) &&
                sistem.TipuriRezervari.Any(x => string.Equals(x.Nume, numeNou, StringComparison.OrdinalIgnoreCase)))
            {
                AnsiConsole.MarkupLine("[red]Another type already uses this name.[/]");
                UIComun.Pauza();
                return;
            }

            decimal pretNou = CereDecimal($"New price (RON) [grey](current: {tip.Pret})[/]:", min: 0.01m, max: 99999m);
            string limNou = AnsiConsole.Ask<string>($"New limitations [grey](press Enter to keep current is not supported here)[/]:");
            string benNou = AnsiConsole.Ask<string>($"New benefits [grey](press Enter to keep current is not supported here)[/]:");

            tip.Nume = numeNou;
            tip.Pret = pretNou;
            tip.Limitari = limNou ?? "";
            tip.Beneficii = benNou ?? "";

            UIComun.SalvareSistem(sistem);
            AnsiConsole.MarkupLine("[green]Reservation type updated.[/]");
            UIComun.Pauza();
        }

        private static void StergeTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No reservation types to delete.[/]");
                UIComun.Pauza();
                return;
            }

            var tip = AlegeTipRezervare(sistem, "Select a type to delete:");
            if (tip == null) return;

            if (!AnsiConsole.Confirm($"Delete [red]{Markup.Escape(tip.Nume)}[/]?"))
            {
                UIComun.Pauza();
                return;
            }

            // Nota: Nu stergem rezervarile existente; doar tipul din lista.
            sistem.TipuriRezervari.Remove(tip);
            UIComun.SalvareSistem(sistem);

            AnsiConsole.MarkupLine("[green]Reservation type deleted.[/]");
            UIComun.Pauza();
        }

        // -------------------------
        // VIEW RESERVATIONS
        // -------------------------
        private static void VizualizeazaRezervari(SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            var matcherie = AlegeMatcherie(sistem, "Select matchery to view reservations:");
            if (matcherie == null) return;

            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule($"[green]Reservations[/] [grey]→[/] {Markup.Escape(matcherie.Nume)}").RuleStyle("grey"));
            AnsiConsole.WriteLine();

            var table = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Title("[bold orange1]Reservations[/]");

            table.AddColumn("[bold]No.[/]");
            table.AddColumn("[bold]Client[/]");
            table.AddColumn("[bold]Type[/]");
            table.AddColumn(new TableColumn("[bold]Price[/]").RightAligned());
            table.AddColumn("[bold]Benefits[/]");

            var list = matcherie.Rezervari ?? new List<Rezervare>();

            if (list.Count == 0)
            {
                table.AddRow("—", "[grey]No reservations[/]", "-", "-", "-");
            }
            else
            {
                int i = 1;
                foreach (var r in list)
                {
                    table.AddRow(
                        i.ToString(),
                        Markup.Escape(r.ClientID ?? "N/A"),
                        Markup.Escape(r.Tip ?? "N/A"),
                        $"{r.Pret} RON",
                        Markup.Escape(r.Beneficii ?? "")
                    );
                    i++;
                }
            }

            int rez = list.Count;
            int cap = matcherie.Capacitate <= 0 ? 0 : matcherie.Capacitate;
            int free = cap <= 0 ? 0 : Math.Max(0, cap - rez);

            var info = new Panel(new Markup($"[bold]Capacity:[/] {cap}  [bold]Occupied:[/] {rez}  [bold]Free:[/] {(free > 0 ? $"[green]{free}[/]" : $"[red]{free}[/]")}"))
                .Border(BoxBorder.Rounded)
                .BorderColor(Color.Green)
                .Header("[bold green]Status[/]")
                .Expand();

            AnsiConsole.Write(info);
            AnsiConsole.WriteLine();
            AnsiConsole.Write(table);
            UIComun.Pauza();
        }

        // -------------------------
        // PICKERS
        // -------------------------
        private static Matcherie? AlegeMatcherie(SistemMatcha sistem, string titlu)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0) return null;

            var map = new Dictionary<string, Matcherie>();
            foreach (var m in sistem.Magazine.OrderBy(x => x.Nume))
            {
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int free = Math.Max(0, cap - rez);

                string keyBase = $"{m.Nume} | {m.Program} | {free}/{cap} free";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key))
                {
                    key = $"{keyBase} #{k}";
                    k++;
                }
                map[key] = m;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold green]{Markup.Escape(titlu)}[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            return map[ales];
        }

        private static TipRezervare? AlegeTipRezervare(SistemMatcha sistem, string titlu)
        {
            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0) return null;

            var map = new Dictionary<string, TipRezervare>();
            foreach (var t in sistem.TipuriRezervari.OrderBy(x => x.Nume))
            {
                string keyBase = $"{t.Nume} - {t.Pret} RON";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key))
                {
                    key = $"{keyBase} #{k}";
                    k++;
                }
                map[key] = t;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"[bold orange1]{Markup.Escape(titlu)}[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            return map[ales];
        }

        // -------------------------
        // INPUT HELPERS
        // -------------------------
        private static string CereTextObligatoriu(string prompt)
        {
            while (true)
            {
                string x = AnsiConsole.Ask<string>(prompt);
                if (!string.IsNullOrWhiteSpace(x))
                    return x;
                AnsiConsole.MarkupLine("[red]Value cannot be empty.[/]");
            }
        }

        private static int CereInt(string prompt, int min, int max)
        {
            while (true)
            {
                string s = AnsiConsole.Ask<string>(prompt);
                if (int.TryParse(s, out int v) && v >= min && v <= max)
                    return v;

                AnsiConsole.MarkupLine($"[red]Enter a valid integer between {min} and {max}.[/]");
            }
        }

        private static decimal CereDecimal(string prompt, decimal min, decimal max)
        {
            while (true)
            {
                string s = AnsiConsole.Ask<string>(prompt);

                // Accept both "12.5" and "12,5" (RO keyboard)
                s = s.Replace(',', '.');

                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal v) && v >= min && v <= max)
                    return v;

                AnsiConsole.MarkupLine($"[red]Enter a valid number between {min} and {max}.[/]");
            }
        }
    }
}
