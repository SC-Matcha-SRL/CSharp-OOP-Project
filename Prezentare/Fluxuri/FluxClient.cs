using System;
using System.Collections.Generic;
using System.Linq;
using Spectre.Console;

namespace ConsoleApp5
{
    public static class FluxClient
    {
        public static void Run(SistemMatcha sistem)
        {
            var client = ServiciiCont.AlegeClientDinSistem(sistem);
            if (client == null) return;

            client = ServiciiCont.ReparaClientDacaENecesat(sistem, client);

            bool inapoi = false;
            while (!inapoi)
            {
                Meniuri.AfiseazaDashboardClient(client, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]What do you want to do?[/]")
                        .AddChoices(new[]
                        {
                            "Order Matcha",
                            "Book a Table",
                            "Cancel a Reservation",
                            "Transactions History",
                            "View Reservations",
                            "Logout"
                        }));

                switch (optiune)
                {
                    case "Order Matcha":
                        ComandaMatcha(client, sistem);
                        break;

                    case "Book a Table":
                        RezervaMasa(client, sistem);
                        break;

                    case "Cancel a Reservation":
                        AnuleazaRezervare(client, sistem);
                        break;

                    case "Transactions History":
                        AfiseazaIstoric(client);
                        break;

                    case "View Reservations":
                        AfiseazaRezervari(client);
                        break;

                    case "Logout":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void ComandaMatcha(ContClient client, SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            var magazin = AlegeMatcherieDinLista(sistem.Magazine);
            if (magazin == null) return;

            if (magazin.Meniu == null || magazin.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]This matchery has an empty menu.[/]");
                UIComun.Pauza();
                return;
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(Meniuri.ConstruiesteTabelMeniu(magazin));
            AnsiConsole.WriteLine();

            var map = new Dictionary<string, Matcha>();
            foreach (var p in magazin.Meniu)
            {
                string keyBase = $"{p.Nume} - {p.Pret} RON (stock {p.Cantitate})";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key)) { key = $"{keyBase} #{k}"; k++; }
                map[key] = p;
            }

            string produsKey = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold yellow]Select product[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys));

            var produs = map[produsKey];

            if (produs.Cantitate <= 0)
            {
                AnsiConsole.MarkupLine("[red]Product unavailable (stock 0).[/]");
                UIComun.Pauza();
                return;
            }

            int qty;
            while (true)
            {
                qty = AnsiConsole.Ask<int>($"Quantity [grey](1 - {produs.Cantitate})[/]:");
                if (qty >= 1 && qty <= produs.Cantitate) break;
                AnsiConsole.MarkupLine("[red]Invalid quantity.[/]");
            }

            decimal total = produs.Pret * qty;

            if (!AnsiConsole.Confirm($"Confirm order: [green]{qty}x {Markup.Escape(produs.Nume)}[/] = [yellow]{total} RON[/]?"))
            {
                AnsiConsole.MarkupLine("[grey]Order canceled.[/]");
                UIComun.Pauza();
                return;
            }

            produs.Cantitate -= qty;
            client.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), DateTime.Now, total, magazin));

            UIComun.SalvareSistem(sistem);

            var receipt = new Panel(new Rows(
                    new Markup("[bold green]Order confirmed![/]"),
                    new Markup($"[grey]Client:[/] {Markup.Escape(client.Nume)}"),
                    new Markup($"[grey]Matchery:[/] {Markup.Escape(magazin.Nume)}"),
                    new Markup($"[grey]Product:[/] {Markup.Escape(produs.Nume)} x {qty}"),
                    new Markup($"[grey]Total:[/] [yellow]{total} RON[/]"),
                    new Markup($"[grey]Remaining stock:[/] {produs.Cantitate}")
                ))
                .Header("[bold]🧾 Receipt[/]")
                .BorderColor(Color.Green)
                .Expand();

            AnsiConsole.Write(receipt);
            UIComun.Pauza();
        }

        private static void RezervaMasa(ContClient client, SistemMatcha sistem)
        {
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No matcheries available.[/]");
                UIComun.Pauza();
                return;
            }

            var magazin = AlegeMatcherieDinLista(sistem.Magazine);
            if (magazin == null) return;

            if (sistem.TipuriRezervari == null || sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No reservation types available. Admin must add them first.[/]");
                UIComun.Pauza();
                return;
            }

            int cap = magazin.Capacitate <= 0 ? 0 : magazin.Capacitate;
            int ocupate = magazin.Rezervari?.Count ?? 0;
            if (cap == 0 || ocupate >= cap)
            {
                AnsiConsole.MarkupLine("[red]Sorry, the matchery is full.[/]");
                UIComun.Pauza();
                return;
            }

            var tipAles = AnsiConsole.Prompt(
                new SelectionPrompt<TipRezervare>()
                    .Title($"Select reservation type for [green]{Markup.Escape(magazin.Nume)}[/]:")
                    .PageSize(10)
                    .AddChoices(sistem.TipuriRezervari)
                    .UseConverter(t => $"{t.Nume} - {t.Pret} RON")
            );

            var detalii = new Panel(new Rows(
                    new Markup($"[bold]{Markup.Escape(tipAles.Nume)}[/]   [green]{tipAles.Pret} RON[/]"),
                    new Markup($"[grey]Limitations:[/] {Markup.Escape(tipAles.Limitari ?? "")}"),
                    new Markup($"[grey]Benefits:[/] {Markup.Escape(tipAles.Beneficii ?? "")}"),
                    new Markup($"[grey]Seats available now:[/] [white]{(cap - ocupate)}[/] / [white]{cap}[/]")
                ))
                .Header("[bold yellow]Reservation details[/]")
                .BorderColor(Color.Orange1)
                .Expand();

            AnsiConsole.Clear();
            AnsiConsole.Write(detalii);

            if (!AnsiConsole.Confirm("Confirm this reservation?"))
            {
                AnsiConsole.MarkupLine("[grey]Reservation canceled.[/]");
                UIComun.Pauza();
                return;
            }

            var coord = LocatieServicii.Get<CoordonatorSistem>();
            bool ok = coord.TryCreeazaRezervare(client, magazin, tipAles, out _, out string mesaj);

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

        private static void AfiseazaRezervari(ContClient client)
        {
            AnsiConsole.Clear();

            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You have no active reservations.[/]");
                UIComun.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Title("[bold orange1]📌 YOUR RESERVATIONS[/]")
                .Caption($"[grey]Total active reservations: {client.Rezervari.Count}[/]");

            tabel.AddColumn("[bold]No.[/]");
            tabel.AddColumn("[bold]Matchery[/]");
            tabel.AddColumn("[bold]Type[/]");
            tabel.AddColumn("[bold]Benefits[/]");
            tabel.AddColumn(new TableColumn("[bold]Price[/]").RightAligned());

            for (int i = 0; i < client.Rezervari.Count; i++)
            {
                var r = client.Rezervari[i];
                tabel.AddRow(
                    (i + 1).ToString(),
                    $"[cyan]{Markup.Escape(r.MatcherieNume ?? "N/A")}[/]",
                    Markup.Escape(r.Tip ?? "N/A"),
                    $"[grey]{Markup.Escape(r.Beneficii ?? "")}[/]",
                    $"[green]{r.Pret} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            UIComun.Pauza();
        }

        private static void AnuleazaRezervare(ContClient client, SistemMatcha sistem)
        {
            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No reservations to cancel.[/]");
                UIComun.Pauza();
                return;
            }

            var rezervare = AnsiConsole.Prompt(
                new SelectionPrompt<Rezervare>()
                    .Title("Select reservation to [red]cancel[/]:")
                    .PageSize(10)
                    .AddChoices(client.Rezervari)
                    .UseConverter(r => $"[{Markup.Escape(r.MatcherieNume ?? "N/A")}] {Markup.Escape(r.Tip ?? "Reservation")} - {r.Pret} RON")
            );

            if (!AnsiConsole.Confirm($"Cancel [yellow]{Markup.Escape(rezervare.Tip)}[/]?"))
            {
                UIComun.Pauza();
                return;
            }

            var coord = LocatieServicii.Get<CoordonatorSistem>();
            bool ok = coord.TryAnuleazaRezervare(client, rezervare, out string mesaj);

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

        private static void AfiseazaIstoric(ContClient client)
        {
            AnsiConsole.Clear();

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No transactions found.[/]");
                UIComun.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.DoubleEdge)
                .BorderColor(Color.Magenta1)
                .Title("[bold magenta]🧾 TRANSACTIONS HISTORY[/]");

            tabel.AddColumn("Date");
            tabel.AddColumn("Matchery");
            tabel.AddColumn(new TableColumn("Amount").RightAligned());

            foreach (var t in client.Istoric)
            {
                tabel.AddRow(
                    t.Data.ToString("dd/MM/yyyy HH:mm"),
                    Markup.Escape(t.MatcherieNume ?? "N/A"),
                    $"[green]{t.Suma} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            UIComun.Pauza();
        }

        private static Matcherie? AlegeMatcherieDinLista(List<Matcherie> magazine)
        {
            if (magazine == null || magazine.Count == 0) return null;

            var map = new Dictionary<string, Matcherie>();

            foreach (var m in magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int free = Math.Max(0, cap - rez);

                string keyBase = $"{m.Nume} | {m.Program} | {free}/{cap} free";
                string key = keyBase;
                int k = 2;
                while (map.ContainsKey(key)) { key = $"{keyBase} #{k}"; k++; }

                map[key] = m;
            }

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold green]Select matchery[/]")
                    .PageSize(10)
                    .AddChoices(map.Keys)
            );

            return map[ales];
        }
    }
}
