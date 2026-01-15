using Spectre.Console;

namespace ConsoleApp5
{
    /// <summary>
    /// Flow-ul complet pentru client (selectare client + meniul client).
    /// </summary>
    public static class ClientFlow
    {
        public static void Run(SistemMatcha sistem)
        {
            var client = AccountService.AlegeClientDinSistem(sistem);
            if (client == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);

            bool inapoi = false;
            while (!inapoi)
            {
                Meniuri.AfiseazaDashboardClient(client, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Ce dorești să faci?[/]")
                        .AddChoices(new[]
                        {
                            "Comandă Matcha",
                            "Rezervă Masă",
                            "Anulează Rezervare Masă",
                            "Istoric Tranzacții",
                            "Vizualizează Rezervări",
                            "Deconectare"
                        }));

                switch (optiune)
                {
                    case "Comandă Matcha":
                    {
                        string? numeMagazinAles = client.AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        if (string.IsNullOrEmpty(numeMagazinAles)) break;

                        // păstrăm logica ta: admin[0] „încasează”
                        if (sistem.Administratori.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[red]Nu există administratori în sistem.[/]");
                            CommonUI.Pauza();
                            break;
                        }

                        client.Comanda(numeMagazinAles, sistem.Magazine, sistem.Administratori[0]);
                        CommonUI.Pauza();
                        break;
                    }

                    case "Rezervă Masă":
                    {
                        string? numeMagazinAles = client.AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        if (string.IsNullOrEmpty(numeMagazinAles)) break;

                        if (sistem.Administratori.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[red]Nu există administratori în sistem.[/]");
                            CommonUI.Pauza();
                            break;
                        }

                        foreach (var magazin in sistem.Magazine)
                        {
                            if (magazin.Nume == numeMagazinAles)
                            {
                                var nouaRezervare = client.rezervaMasa(magazin, sistem.Administratori[0]);

                                // păstrăm fix comportamentul tău: add în lista clientului aici
                                if (nouaRezervare != null)
                                {
                                    client.Rezervari.Add(nouaRezervare);
                                    AnsiConsole.MarkupLine("[green]Rezervare adăugată cu succes![/]");
                                }
                                break;
                            }
                        }

                        CommonUI.Pauza();
                        break;
                    }

                    case "Vizualizează Rezervări":
                        AfiseazaRezervari(client);
                        break;

                    case "Anulează Rezervare Masă":
                        AnuleazaRezervare(client);
                        break;

                    case "Istoric Tranzacții":
                        AfiseazaIstoric(client);
                        break;

                    case "Deconectare":
                        inapoi = true;
                        break;
                }
            }
        }

        /// <summary>
        /// Tabel: rezervările curente ale clientului.
        /// </summary>
        private static void AfiseazaRezervari(Client client)
        {
            Console.Clear();

            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă momentan.[/]");
                CommonUI.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.Rounded)
                .BorderColor(Color.Orange1)
                .Title("[bold orange1]📌 REZERVĂRILE TALE[/]")
                .Caption("[grey]Total rezervări active: " + client.Rezervari.Count + "[/]");

            tabel.AddColumn("[bold]Nr.[/]");
            tabel.AddColumn("[bold]Locație[/]");
            tabel.AddColumn("[bold]Tip Rezervare[/]");
            tabel.AddColumn("[bold]Beneficii[/]");
            tabel.AddColumn(new TableColumn("[bold]Preț[/]").Centered());

            for (int i = 0; i < client.Rezervari.Count; i++)
            {
                var rez = client.Rezervari[i];
                tabel.AddRow(
                    (i + 1).ToString(),
                    $"[cyan]{rez.Matcherie?.Nume ?? "Nespecificat"}[/]",
                    rez.Tip,
                    $"[italic grey]{rez.Beneficii}[/]",
                    $"[green]{rez.Pret} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            CommonUI.Pauza();
        }

        /// <summary>
        /// Select prompt + confirm + ștergere din Matcherie și din client.
        /// </summary>
        private static void AnuleazaRezervare(Client client)
        {
            if (client.Rezervari == null || client.Rezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă de anulat.[/]");
                CommonUI.Pauza();
                return;
            }

            var rezervareDeAnulat = AnsiConsole.Prompt(
                new SelectionPrompt<Rezervare>()
                    .Title("Selectează rezervarea pe care dorești să o [red]anulezi[/]:")
                    .PageSize(10)
                    .AddChoices(client.Rezervari)
                    .UseConverter(r =>
                    {
                        string numeEscapat = Markup.Escape(r.Matcherie?.Nume ?? "Nespecificat");
                        string tipEscapat = Markup.Escape(r.Tip ?? "Rezervare");
                        return $"[[{numeEscapat}]] {tipEscapat} - [green]{r.Pret} RON[/]";
                    }));

            if (AnsiConsole.Confirm($"Sigur dorești să anulezi rezervarea [yellow]{Markup.Escape(rezervareDeAnulat.Tip)}[/]?"))
            {
                rezervareDeAnulat.Matcherie?.Rezervari.Remove(rezervareDeAnulat);
                client.Rezervari.Remove(rezervareDeAnulat);

                AnsiConsole.MarkupLine("[bold green]Rezervarea a fost anulată cu succes![/]");
            }

            CommonUI.Pauza();
        }

        /// <summary>
        /// Tabel istoric tranzacții.
        /// </summary>
        private static void AfiseazaIstoric(Client client)
        {
            Console.Clear();

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu ai nicio tranzacție înregistrată.[/]");
                CommonUI.Pauza();
                return;
            }

            var tabel = new Table()
                .Border(TableBorder.DoubleEdge)
                .Title("[bold magenta]🧾 ISTORIC CUMPĂRĂTURI[/]")
                .BorderColor(Color.Magenta1);

            tabel.AddColumn("Dată");
            tabel.AddColumn("Magazin");
            tabel.AddColumn(new TableColumn("Preț").RightAligned());

            foreach (var t in client.Istoric)
            {
                tabel.AddRow(
                    t.Data.ToString("dd/MM/yyyy HH:mm"),
                    Markup.Escape(t.Matcherie?.Nume ?? "N/A"),
                    $"[green]{t.suma} RON[/]"
                );
            }

            AnsiConsole.Write(tabel);
            CommonUI.Pauza();
        }
    }
}
