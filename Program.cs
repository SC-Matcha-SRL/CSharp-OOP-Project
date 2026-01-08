using System.Runtime.CompilerServices;
using ConsoleApp5;
using Spectre.Console;
namespace ConsoleApp5
{
    class Program
    {
        static void Main(string[] args)
        {
            SistemMatcha sistem = GestiuneDate.IncarcaTot();//INIȚIALIZARE
            if (sistem.Administratori.Count == 0)
            {
                IncarcaDateTest(sistem);
            }

            AnsiConsole.Write(new FigletText("Matcha System").Color(Color.Green));

            // --- 2. BUCAL PRINCIPALĂ ---
            bool ruleazaProgramul = true;
            while (ruleazaProgramul)
            {
                Console.Clear();
                var rol = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold white]Cine folosește aplicația?[/]")
                        .AddChoices(new[] { "Administrator", "Client", "Ieșire" }));

                switch (rol)
                {
                    case "Administrator":
                        RulareMeniuAdmin(sistem);
                        break;
                    case "Client":
                        RulareMeniuClient(sistem);
                        break;
                    case "Ieșire":
                        SalvareSistem(sistem);
                        ruleazaProgramul = false;
                        break;
                }
            }
        }

        // --- 3. METODELE DE LOGICĂ (STATICE) ---

        static void RulareMeniuAdmin(SistemMatcha sistem)
        {
            // Login simplificat
            var admin = sistem.Administratori[0]; 
            bool inapoi = false;
            
            while (!inapoi)
            {
                Console.Clear();
                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"[bold red]PANOU ADMIN[/] - Salut, {admin.Nume}")
                        .AddChoices(new[] { "Vezi Raport Magazine", "Modifică Magazin", "Înapoi" }));

                if (optiune == "Vezi Raport Magazine") 
                    admin.informatii();
                else if (optiune == "Modifică Magazin") 
                {
                    var nume = AnsiConsole.Ask<string>("Numele magazinului:");
                    admin.modificaMatcherie(nume);
                }
                else inapoi = true;

                if (!inapoi) { Console.WriteLine("\nApasă o tastă..."); Console.ReadKey(); }
            }
        }

        static void RulareMeniuClient(SistemMatcha sistem)
        {
            bool inapoi = false;
            while (!inapoi)
            {
                // Apelăm noua funcție de UI
                Meniuri.AfiseazaDashboardClient(sistem.Clienti[0], sistem);

                var optiune = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Ce dorești să faci?[/]")
                        .AddChoices(new[] { 
                            "Comandă Matcha", 
                            "Rezervă Masă", 
                            "Anuleaza Rezervare Masă",
                            "Istoric Tranzacții",
                            "Vizulaizează Rezervări",
                            "Deconectare" 
                        }));
                var NumeMagazinAles = "";
                switch (optiune)
                {
                    case "Comandă Matcha":
                         NumeMagazinAles = sistem.Clienti[0].AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        sistem.Clienti[0].Comanda(NumeMagazinAles, sistem.Magazine, sistem.Administratori[0]);
                        break;
                    case "Vizulaizează Rezervări":
                        Console.Clear(); // Curățăm ecranul pentru a vedea doar tabelul
    
                        if (sistem.Clienti[0].Rezervari == null || sistem.Clienti[0].Rezervari.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă momentan.[/]");
                        }
                        else
                        {
                            // Creăm tabelul
                            var tabel = new Table()
                                .Border(TableBorder.Rounded)
                                .BorderColor(Color.Orange1)
                                .Title("[bold orange1]📅 REZERVĂRILE TALE[/]")
                                .Caption("[grey]Total rezervări active: " + sistem.Clienti[0].Rezervari.Count + "[/]");

                            tabel.AddColumn("[bold]Nr.[/]");
                            tabel.AddColumn("[bold]Locație[/]");
                            tabel.AddColumn("[bold]Tip Rezervare[/]");
                            tabel.AddColumn("[bold]Beneficii[/]");
                            tabel.AddColumn(new TableColumn("[bold]Pret[/]").Centered());

                            for (int i = 0; i < sistem.Clienti[0].Rezervari.Count; i++)
                            {
                                var rez = sistem.Clienti[0].Rezervari[i];
                                tabel.AddRow(
                                    (i + 1).ToString(),
                                    $"[cyan]{rez.Matcherie?.Nume ?? "Nespecificat"}[/]",
                                    rez.Tip,
                                    $"[italic grey]{rez.Beneficii}[/]",
                                    $"[green]{rez.Pret} RON[/]"
                                );
                            }
                            AnsiConsole.Write(tabel);
                        }
                        // --- ELEMENTUL CRUCIAL ---
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[grey]Apasă orice tastă pentru a reveni la meniu...[/]");
                        Console.ReadKey(true); // Oprește execuția până la apăsarea unei taste
                        break;
                    case "Rezervă Masă":
                        NumeMagazinAles = sistem.Clienti[0].AfiseazaRestauranteSiAlegeUnul(sistem.Magazine);
                        if (string.IsNullOrEmpty(NumeMagazinAles)) break;
                        foreach (var magazin in sistem.Magazine)
                        {
                            if (magazin.Nume==NumeMagazinAles)
                            {
                                /*
                                // Verificăm dacă lista Rezervari nu este null înainte de Add
                                if (sistem.Clienti[0].Rezervari == null) 
                                    sistem.Clienti[0].Rezervari = new List<Rezervare>();
                                */
                                var nouaRezervare = sistem.Clienti[0].rezervaMasa(magazin, sistem.Administratori[0]);
            
                                // Adăugăm în listă DOAR dacă metoda returnează obiectul și nu l-a adăugat deja intern
                                if (nouaRezervare != null)
                                {
                                    sistem.Clienti[0].Rezervari.Add(nouaRezervare);
                                    AnsiConsole.MarkupLine("[green]Rezervare adăugată cu succes![/]");
                                }
                            }
                        }
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[grey]Apasă orice tastă pentru a reveni la meniu...[/]");
                        Console.ReadKey(true); // Oprește execuția până la apăsarea unei taste
                        break;
                    case "Anuleaza Rezervare Masă":
                        if (sistem.Clienti[0].Rezervari == null || sistem.Clienti[0].Rezervari.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio rezervare activă de anulat.[/]");
                            Console.ReadKey(true);
                            break;
                        }

                        var rezervareDeAnulat = AnsiConsole.Prompt(
                            new SelectionPrompt<Rezervare>()
                                .Title("Selectează rezervarea pe care dorești să o [red]anulezi[/]:")
                                .PageSize(10)
                                .AddChoices(sistem.Clienti[0].Rezervari)
                                .UseConverter(r => {
                                    // ESCAPARE: Protejăm textul împotriva interpretării ca stil/culoare
                                    string numeEscapat = Markup.Escape(r.Matcherie?.Nume ?? "Nespecificat");
                                    string tipEscapat = Markup.Escape(r.Tip ?? "Rezervare");
                
                                    return $"[[{numeEscapat}]] {tipEscapat} - [green]{r.Pret} RON[/]";
                                }));

                        if (AnsiConsole.Confirm($"Sigur dorești să anulezi rezervarea [yellow]{Markup.Escape(rezervareDeAnulat.Tip)}[/]?"))
                        {
                            // Ștergem din ambele liste
                            rezervareDeAnulat.Matcherie?.Rezervari.Remove(rezervareDeAnulat);//?-null conditional operator
                            sistem.Clienti[0].Rezervari.Remove(rezervareDeAnulat);

                            AnsiConsole.MarkupLine("[bold green]Rezervarea a fost anulată cu succes![/]");
                        }
    
                        Console.ReadKey(true);
                        break;
                    case "Istoric Tranzacții":
                        Console.Clear();
                        if (sistem.Clienti[0].Istoric == null || sistem.Clienti[0].Istoric.Count == 0)
                        {
                            AnsiConsole.MarkupLine("[yellow]Nu ai nicio tranzacție înregistrată.[/]");
                        }
                        else
                        {
                            var tabel = new Table()
                                .Border(TableBorder.DoubleEdge)
                                .Title("[bold magenta]🧾 ISTORIC CUMPĂRĂTURI[/]")
                                .BorderColor(Color.Magenta1);

                            tabel.AddColumn("Dată");
                            tabel.AddColumn("Magazin");
                            tabel.AddColumn(new TableColumn("Preț").RightAligned());

                            foreach (var t in sistem.Clienti[0].Istoric)
                            {
                                tabel.AddRow(
                                    t.Data.ToString("dd/MM/yyyy HH:mm"),
                                    Markup.Escape(t.Matcherie.Nume),
                                    $"[green]{t.suma} RON[/]"
                                );
                            }
                            AnsiConsole.Write(tabel);
                        }

                        AnsiConsole.WriteLine("\nApasă orice tastă pentru a reveni...");
                        Console.ReadKey(true);
                        break;
                        
                        break;
                    case "Deconectare":
                        inapoi = true;
                        break;
                }
            }
        }

        static void SalvareSistem(SistemMatcha sistem)
        {
            AnsiConsole.Status().Start("Se salvează datele...", ctx => {
                GestiuneDate.SalveazaTot(sistem);
                Thread.Sleep(800);
            });
        }

        static void IncarcaDateTest(SistemMatcha sistem)
        {
            var meniu = new List<Matcha> { new Matcha("Matcha Latte", "Clasic", 22.5m, 100, 120) };
            var m1 = new Matcherie("Matcha Zen", "08-22", 20, meniu, new List<Rezervare>());
            var meniu2 = new List<Matcha> { new Matcha("Matcha Latte vf. Odaia", "Clasic amar", 22.5m, 100, 120) };
            var m3 = new Matcherie("Matcha urzica sanicolau nou", "08-22", 20, meniu2, new List<Rezervare>());
            sistem.Magazine.Add(m1);
            sistem.Magazine.Add(m3);
            sistem.Administratori.Add(new AdministratorMatcha("Admin", "ADM01", "1234", sistem.Magazine));
            sistem.Administratori.Add(new AdministratorMatcha("Admin22", "ADM02", "1234", sistem.Magazine));
            sistem.Clienti.Add(new Client("Andrei", "andrei@email.com", new List<Tranzactie>(), new List<Rezervare>()));
        }
    }
}