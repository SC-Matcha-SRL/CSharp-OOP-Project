using Spectre.Console;
using Spectre.Console.Rendering;

namespace ConsoleApp5
{
    /// <summary>
    /// Flow-ul complet pentru Admin: login + dashboard + toate submeniurile.
    /// </summary>
    public static class AdminFlow
    {
        public static void Run(SistemMatcha sistem)
        {
            var admin = LoginAdmin(sistem);
            if (admin == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AfiseazaDashboardAdmin(admin, sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]PANOU ADMIN[/] - Salut, [white]{Markup.Escape(admin.Nume)}[/]")
                        .AddChoices(new[]
                        {
                            "1) Administrare Matcherii (CRUD)",
                            "2) Tipuri Rezervări (CRUD)",
                            "3) Tranzacții (creare/modificare/asociere client)",
                            "4) Monitorizare activitate",
                            "5) Creează Administrator (cont nou)",
                            "Deconectare"
                        }));

                switch (optiune)
                {
                    case "1) Administrare Matcherii (CRUD)":
                        SubmeniuMatcherii(admin, sistem);
                        break;

                    case "2) Tipuri Rezervări (CRUD)":
                        SubmeniuTipuriRezervari(sistem);
                        break;

                    case "3) Tranzacții (creare/modificare/asociere client)":
                        SubmeniuTranzactii(sistem);
                        break;

                    case "4) Monitorizare activitate":
                        AfiseazaMonitorizare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "5) Creează Administrator (cont nou)":
                        AccountService.CreeazaAdminNou(sistem);
                        break;

                    case "Deconectare":
                        inapoi = true;
                        break;
                }
            }
        }

        // -------------------- LOGIN ADMIN --------------------

        private static AdministratorMatcha? LoginAdmin(SistemMatcha sistem)
        {
            if (sistem.Administratori == null || sistem.Administratori.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există administratori în sistem.[/]");
                CommonUI.Pauza();
                return null;
            }

            for (int incercari = 0; incercari < 3; incercari++)
            {
                Console.Clear();
                AnsiConsole.Write(new Rule("[red]Autentificare Administrator[/]").RuleStyle("grey"));

                string id = AnsiConsole.Ask<string>("Admin ID:");
                string parola = AnsiConsole.Prompt(new TextPrompt<string>("Parola:").Secret());

                foreach (var a in sistem.Administratori)
                {
                    if (a.AdminId == id && a.Parola == parola)
                    {
                        AnsiConsole.MarkupLine("[green]Autentificare reușită![/]");
                        Thread.Sleep(250);

                        // Rebind: Admin trebuie să “vadă” lista globală de matcherii
                        var adminLegatDeSistem = new AdministratorMatcha(a.Nume, a.AdminId, a.Parola, sistem.Magazine);

                        int idx = sistem.Administratori.IndexOf(a);
                        if (idx >= 0) sistem.Administratori[idx] = adminLegatDeSistem;

                        return adminLegatDeSistem;
                    }
                }

                AnsiConsole.MarkupLine("[red]Date invalide. Mai încearcă.[/]");
                Thread.Sleep(600);
            }

            AnsiConsole.MarkupLine("[red]Prea multe încercări. Revenire la meniu.[/]");
            CommonUI.Pauza();
            return null;
        }

        // -------------------- DASHBOARD ADMIN --------------------

        private static void AfiseazaDashboardAdmin(AdministratorMatcha admin, SistemMatcha sistem)
        {
            int nrMagazine = sistem.Magazine?.Count ?? 0;
            int nrClienti = sistem.Clienti?.Count ?? 0;

            int rezervariActive = 0;
            if (sistem.Magazine != null)
                foreach (var m in sistem.Magazine)
                    rezervariActive += (m.Rezervari?.Count ?? 0);

            int tranzactii = 0;
            if (sistem.Clienti != null)
                foreach (var c in sistem.Clienti)
                    tranzactii += (c.Istoric?.Count ?? 0);

            // Panel stânga: info
            var info = new Panel(
                new Rows(
                    new Markup($"[bold]Admin:[/] {Markup.Escape(admin.Nume)} ([grey]{Markup.Escape(admin.AdminId)}[/])"),
                    new Markup($"[bold]Magazine:[/] {nrMagazine}"),
                    new Markup($"[bold]Clienți:[/] {nrClienti}"),
                    new Markup($"[bold]Rezervări active:[/] {rezervariActive}"),
                    new Markup($"[bold]Tranzacții totale:[/] {tranzactii}")
                ))
                .Header("[bold red]📌 DASHBOARD ADMIN[/]")
                .BorderColor(Color.Red)
                .Expand();

            // Grafic: vânzări ultimele 7 zile (din Tranzactie.Data)
            var chart = new BarChart()
                .Label("[green]Vânzări (ultimele 7 zile)[/]")
                .CenterLabel();

            int width = Math.Max(30, Math.Min(60, AnsiConsole.Profile.Width / 2 - 10));
            chart.Width(width);

            DateTime azi = DateTime.Today;
            for (int i = 6; i >= 0; i--)
            {
                DateTime zi = azi.AddDays(-i);
                int countZi = 0;

                if (sistem.Clienti != null)
                {
                    foreach (var c in sistem.Clienti)
                    {
                        if (c.Istoric == null) continue;
                        foreach (var t in c.Istoric)
                            if (t.Data.Date == zi.Date) countZi++;
                    }
                }

                chart.AddItem(zi.ToString("dd/MM"), countZi, Color.Green);
            }

            var chartPanel = new Panel(chart)
                .Header("[bold green]📈 Trend[/]")
                .BorderColor(Color.Green)
                .Expand();

            // Layout: 2 coloane dacă există spațiu
            int w = AnsiConsole.Profile.Width;
            if (w >= 120)
            {
                var grid = new Grid();
                grid.AddColumn(new GridColumn());
                grid.AddColumn(new GridColumn());
                grid.AddRow(info, chartPanel);
                AnsiConsole.Write(grid);
            }
            else
            {
                AnsiConsole.Write(info);
                AnsiConsole.WriteLine();
                AnsiConsole.Write(chartPanel);
            }

            AnsiConsole.WriteLine();
        }

        // -------------------- MATCHERII CRUD --------------------

        private static void SubmeniuMatcherii(AdministratorMatcha admin, SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Administrare Matcherii[/]")
                        .AddChoices(new[]
                        {
                            "Vezi raport matcherii",
                            "Creează matcherie",
                            "Modifică matcherie (program/capacitate)",
                            "Șterge matcherie",
                            "Meniu produse (CRUD)",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Meniu produse (CRUD)":
                        SubmeniuMeniuProduse(admin);
                        break;

                    case "Vezi raport matcherii":
                        Console.Clear();
                        admin.informatii();
                        CommonUI.Pauza();
                        break;

                    case "Creează matcherie":
                        CreeazaMatcherie(admin, sistem);
                        CommonUI.Pauza();
                        break;

                    case "Modifică matcherie (program/capacitate)":
                    {
                        string nume = AnsiConsole.Ask<string>("Numele matcheriei:");
                        admin.modificaMatcherie(nume);
                        CommonUI.Pauza();
                        break;
                    }

                    case "Șterge matcherie":
                        StergeMatcherie(admin, sistem);
                        CommonUI.Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void CreeazaMatcherie(AdministratorMatcha admin, SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Nume matcherie:");
            string program = AnsiConsole.Ask<string>("Program (ex: 08:00-22:00):");
            int capacitate = AnsiConsole.Ask<int>("Capacitate:");

            var m = new Matcherie(nume, program, capacitate, new List<Matcha>(), new List<Rezervare>());
            admin.creazaMatcherie(m, sistem.Magazine);
        }

        private static void StergeMatcherie(AdministratorMatcha admin, SistemMatcha sistem)
        {
            if (admin.Matcherii == null || admin.Matcherii.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in admin.Matcherii) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege matcheria de șters:")
                    .AddChoices(numeList));

            Matcherie? target = null;
            foreach (var m in admin.Matcherii)
                if (m.Nume == ales) { target = m; break; }

            if (target != null) admin.stergeMatcherie(target, sistem.Magazine);
        }

        // -------------------- PRODUSE CRUD --------------------

        private static void SubmeniuMeniuProduse(AdministratorMatcha admin)
        {
            if (admin.Matcherii == null || admin.Matcherii.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii. Creează una mai întâi.[/]");
                CommonUI.Pauza();
                return;
            }

            Matcherie matcherie = AlegeMatcherieDinAdmin(admin);
            if (matcherie == null) return;

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();
                AnsiConsole.MarkupLine($"[bold green]Meniu produse[/] pentru: [white]{Markup.Escape(matcherie.Nume)}[/]");
                AnsiConsole.WriteLine();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Alege acțiunea:")
                        .AddChoices(new[]
                        {
                            "Vezi meniul",
                            "Adaugă produs",
                            "Modifică produs",
                            "Șterge produs",
                            "Schimbă matcheria",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Vezi meniul":
                        Console.Clear();
                        AfiseazaMeniuSafe(matcherie);
                        CommonUI.Pauza();
                        break;

                    case "Adaugă produs":
                        AdaugaProdus(matcherie);
                        CommonUI.Pauza();
                        break;

                    case "Modifică produs":
                        ModificaProdus(matcherie);
                        CommonUI.Pauza();
                        break;

                    case "Șterge produs":
                        StergeProdus(matcherie);
                        CommonUI.Pauza();
                        break;

                    case "Schimbă matcheria":
                        matcherie = AlegeMatcherieDinAdmin(admin);
                        if (matcherie == null) return;
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        private static Matcherie AlegeMatcherieDinAdmin(AdministratorMatcha admin)
        {
            var numeList = new List<string>();
            foreach (var m in admin.Matcherii) numeList.Add(m.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege matcheria:")
                    .AddChoices(numeList));

            foreach (var m in admin.Matcherii)
                if (m.Nume == ales) return m;

            return null;
        }

        private static void AfiseazaMeniuSafe(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Meniul este gol.[/]");
                return;
            }

            matcherie.AfiseazaMeniu();
        }

        private static void AdaugaProdus(Matcherie matcherie)
        {
            string nume = AnsiConsole.Ask<string>("Nume produs:");

            foreach (var p in matcherie.Meniu)
                if (p.nume == nume)
                {
                    AnsiConsole.MarkupLine("[red]Există deja un produs cu acest nume.[/]");
                    return;
                }

            string descriere = AnsiConsole.Ask<string>("Descriere:");
            decimal pret = AnsiConsole.Ask<decimal>("Preț (RON):");
            int cantitate = AnsiConsole.Ask<int>("Cantitate (stoc):");
            int calorii = AnsiConsole.Ask<int>("Calorii:");

            matcherie.Meniu.Add(new Matcha(nume, descriere, pret, cantitate, calorii));
            AnsiConsole.MarkupLine("[green]Produs adăugat în meniu.[/]");
        }

        private static void ModificaProdus(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există produse de modificat.[/]");
                return;
            }

            var numeProduse = new List<string>();
            foreach (var p in matcherie.Meniu) numeProduse.Add(p.nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege produsul de modificat:")
                    .AddChoices(numeProduse));

            Matcha produs = null;
            foreach (var p in matcherie.Meniu)
                if (p.nume == ales) { produs = p; break; }

            if (produs == null) return;

            var camp = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Ce dorești să modifici?")
                    .AddChoices(new[] { "Nume", "Descriere", "Preț", "Cantitate", "Calorii", "Anulează" }));

            if (camp == "Anulează") return;

            if (camp == "Nume")
            {
                string nou = AnsiConsole.Ask<string>($"Nume nou (curent: {produs.nume}):");
                foreach (var p in matcherie.Meniu)
                    if (p != produs && p.nume == nou)
                    {
                        AnsiConsole.MarkupLine("[red]Există deja un produs cu acest nume.[/]");
                        return;
                    }
                produs.nume = nou;
            }
            else if (camp == "Descriere") produs.descriere = AnsiConsole.Ask<string>($"Descriere nouă (curent: {produs.descriere}):");
            else if (camp == "Preț") produs.pret = AnsiConsole.Ask<decimal>($"Preț nou (curent: {produs.pret}):");
            else if (camp == "Cantitate") produs.cantitate = AnsiConsole.Ask<int>($"Cantitate nouă (curent: {produs.cantitate}):");
            else if (camp == "Calorii") produs.calorii = AnsiConsole.Ask<int>($"Calorii noi (curent: {produs.calorii}):");

            AnsiConsole.MarkupLine("[green]Produs modificat.[/]");
        }

        private static void StergeProdus(Matcherie matcherie)
        {
            if (matcherie.Meniu == null || matcherie.Meniu.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există produse de șters.[/]");
                return;
            }

            var numeProduse = new List<string>();
            foreach (var p in matcherie.Meniu) numeProduse.Add(p.nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege produsul de șters:")
                    .AddChoices(numeProduse));

            Matcha produs = null;
            foreach (var p in matcherie.Meniu)
                if (p.nume == ales) { produs = p; break; }

            if (produs == null) return;

            if (!AnsiConsole.Confirm($"Sigur vrei să ștergi [red]{Markup.Escape(produs.nume)}[/]?"))
                return;

            matcherie.Meniu.Remove(produs);
            AnsiConsole.MarkupLine("[green]Produs șters.[/]");
        }

        // -------------------- TIPURI REZERVARI CRUD --------------------

        private static void SubmeniuTipuriRezervari(SistemMatcha sistem)
        {
            sistem.TipuriRezervari ??= new List<TipRezervare>();

            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Tipuri Rezervări (admin)[/]")
                        .AddChoices(new[] { "Listă tipuri", "Adaugă tip", "Modifică tip", "Șterge tip", "Înapoi" }));

                switch (opt)
                {
                    case "Listă tipuri":
                        AfiseazaTipuriRezervari(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Adaugă tip":
                        AdaugaTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Modifică tip":
                        ModificaTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Șterge tip":
                        StergeTipRezervare(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaTipuriRezervari(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri definite.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Tipuri rezervări[/]");
            t.AddColumn("Nume");
            t.AddColumn(new TableColumn("Preț").RightAligned());
            t.AddColumn("Limitări");
            t.AddColumn("Beneficii");

            foreach (var tr in sistem.TipuriRezervari)
            {
                t.AddRow(
                    Markup.Escape(tr.Nume),
                    $"{tr.Pret} RON",
                    Markup.Escape(tr.Limitari),
                    Markup.Escape(tr.Beneficii)
                );
            }

            AnsiConsole.Write(t);
        }

        private static void AdaugaTipRezervare(SistemMatcha sistem)
        {
            string nume = AnsiConsole.Ask<string>("Nume tip (ex: Familie, Prieteni):");
            decimal pret = AnsiConsole.Ask<decimal>("Preț:");
            string lim = AnsiConsole.Ask<string>("Limitări:");
            string ben = AnsiConsole.Ask<string>("Beneficii:");

            foreach (var x in sistem.TipuriRezervari)
                if (x.Nume == nume)
                {
                    AnsiConsole.MarkupLine("[red]Există deja un tip cu acest nume.[/]");
                    return;
                }

            sistem.TipuriRezervari.Add(new TipRezervare(nume, pret, lim, ben));
            AnsiConsole.MarkupLine("[green]Tip adăugat.[/]");
        }

        private static void ModificaTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri de modificat.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in sistem.TipuriRezervari) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege tipul de modificat:")
                    .AddChoices(numeList));

            TipRezervare? tip = null;
            foreach (var x in sistem.TipuriRezervari)
                if (x.Nume == ales) { tip = x; break; }

            if (tip == null) return;

            tip.Pret = AnsiConsole.Ask<decimal>($"Preț nou (curent {tip.Pret}):");
            tip.Limitari = AnsiConsole.Ask<string>($"Limitări noi (curent: {tip.Limitari}):");
            tip.Beneficii = AnsiConsole.Ask<string>($"Beneficii noi (curent: {tip.Beneficii}):");

            AnsiConsole.MarkupLine("[green]Tip modificat.[/]");
        }

        private static void StergeTipRezervare(SistemMatcha sistem)
        {
            if (sistem.TipuriRezervari.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tipuri de șters.[/]");
                return;
            }

            var numeList = new List<string>();
            foreach (var x in sistem.TipuriRezervari) numeList.Add(x.Nume);

            string ales = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Alege tipul de șters:")
                    .AddChoices(numeList));

            TipRezervare? tip = null;
            foreach (var x in sistem.TipuriRezervari)
                if (x.Nume == ales) { tip = x; break; }

            if (tip != null)
            {
                sistem.TipuriRezervari.Remove(tip);
                AnsiConsole.MarkupLine("[green]Tip șters.[/]");
            }
        }

        // -------------------- TRANZACTII CRUD --------------------

        private static void SubmeniuTranzactii(SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                Console.Clear();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold]Tranzacții (admin)[/]")
                        .AddChoices(new[]
                        {
                            "Vezi toate tranzacțiile (din istoricul clienților)",
                            "Creează tranzacție pentru un client",
                            "Modifică o tranzacție (înlocuire)",
                            "Înapoi"
                        }));

                switch (opt)
                {
                    case "Vezi toate tranzacțiile (din istoricul clienților)":
                        AfiseazaToateTranzactiile(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Creează tranzacție pentru un client":
                        CreeazaTranzactiePentruClient(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Modifică o tranzacție (înlocuire)":
                        ModificaTranzactieInlocuire(sistem);
                        CommonUI.Pauza();
                        break;

                    case "Înapoi":
                        inapoi = true;
                        break;
                }
            }
        }

        private static void AfiseazaToateTranzactiile(SistemMatcha sistem)
        {
            Console.Clear();

            var table = new Table().Border(TableBorder.Rounded).Title("[bold]Toate tranzacțiile[/]");
            table.AddColumn("Client");
            table.AddColumn("Dată");
            table.AddColumn("Magazin");
            table.AddColumn(new TableColumn("Sumă").RightAligned());

            int count = 0;

            if (sistem.Clienti != null)
            {
                foreach (var c in sistem.Clienti)
                {
                    if (c.Istoric == null) continue;

                    foreach (var t in c.Istoric)
                    {
                        count++;
                        table.AddRow(
                            Markup.Escape(c.Nume),
                            t.Data.ToString("dd/MM/yyyy HH:mm"),
                            Markup.Escape(t.Matcherie?.Nume ?? "N/A"),
                            $"{t.suma} RON"
                        );
                    }
                }
            }

            if (count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există tranzacții.[/]");
                return;
            }

            AnsiConsole.Write(table);
        }

        private static void CreeazaTranzactiePentruClient(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există clienți.[/]");
                return;
            }
            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există matcherii.[/]");
                return;
            }

            // client
            var clientiNume = new List<string>();
            foreach (var c in sistem.Clienti) clientiNume.Add(c.Nume);

            string clientAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează clientul:")
                    .AddChoices(clientiNume));

            // magazin
            var magazineNume = new List<string>();
            foreach (var m in sistem.Magazine) magazineNume.Add(m.Nume);

            string magazinAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează matcheria:")
                    .AddChoices(magazineNume));

            decimal suma = AnsiConsole.Ask<decimal>("Sumă (RON):");

            Client? client = sistem.Clienti.FirstOrDefault(c => c.Nume == clientAles);
            Matcherie? magazin = sistem.Magazine.FirstOrDefault(m => m.Nume == magazinAles);

            if (client == null || magazin == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);
            client.Istoric.Add(new Tranzactie(Guid.NewGuid().ToString(), DateTime.Now, suma, magazin));

            AnsiConsole.MarkupLine("[green]Tranzacție adăugată și asociată clientului.[/]");
        }

        private static void ModificaTranzactieInlocuire(SistemMatcha sistem)
        {
            if (sistem.Clienti == null || sistem.Clienti.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există clienți.[/]");
                return;
            }

            var clientiNume = sistem.Clienti.Select(c => c.Nume).ToList();

            string clientAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează clientul:")
                    .AddChoices(clientiNume));

            Client? client = sistem.Clienti.FirstOrDefault(c => c.Nume == clientAles);
            if (client == null) return;

            client = AccountService.FixClientIfNeeded(sistem, client);

            if (client.Istoric == null || client.Istoric.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Clientul nu are tranzacții.[/]");
                return;
            }

            var tranzList = client.Istoric
                .Select(t => $"{t.Data:dd/MM HH:mm} - {t.Matcherie?.Nume ?? "N/A"} - {t.suma} RON")
                .ToList();

            string tranzAles = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Selectează tranzacția de modificat:")
                    .AddChoices(tranzList));

            int index = tranzList.IndexOf(tranzAles);
            if (index < 0) return;

            var tranzSelectata = client.Istoric[index];

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]Nu există matcherii.[/]");
                return;
            }

            var magazineNume = sistem.Magazine.Select(m => m.Nume).ToList();

            string magazinNouNume = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Matcherie nouă:")
                    .AddChoices(magazineNume));

            Matcherie? magazinNou = sistem.Magazine.FirstOrDefault(m => m.Nume == magazinNouNume);
            if (magazinNou == null) return;

            decimal sumaNoua = AnsiConsole.Ask<decimal>($"Sumă nouă (curent {tranzSelectata.suma}):");

            // Înlocuire (setteri private)
            client.Istoric[index] = new Tranzactie(tranzSelectata.Id, tranzSelectata.Data, sumaNoua, magazinNou);

            AnsiConsole.MarkupLine("[green]Tranzacție modificată (înlocuită).[/]");
        }

        // -------------------- MONITORIZARE --------------------

        private static void AfiseazaMonitorizare(SistemMatcha sistem)
        {
            Console.Clear();

            if (sistem.Magazine == null || sistem.Magazine.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]Nu există matcherii.[/]");
                return;
            }

            var t = new Table().Border(TableBorder.Rounded).Title("[bold]Monitorizare activitate[/]");
            t.AddColumn("Matcherie");
            t.AddColumn(new TableColumn("Rezervări active").RightAligned());
            t.AddColumn(new TableColumn("Capacitate").RightAligned());
            t.AddColumn(new TableColumn("Ocupare").RightAligned());

            foreach (var m in sistem.Magazine)
            {
                int rez = m.Rezervari?.Count ?? 0;
                int cap = m.Capacitate <= 0 ? 1 : m.Capacitate;
                int pct = (int)Math.Round(100.0 * rez / cap);

                t.AddRow(
                    Markup.Escape(m.Nume),
                    rez.ToString(),
                    m.Capacitate.ToString(),
                    $"{pct}%"
                );
            }

            AnsiConsole.Write(t);
        }
    }
}
