using ConsoleApp5;
using Spectre.Console;

// 1. Pregătirea datelor de test (Seed Data)
var meniuBucuresti = new List<Matcha>
{
    new Matcha("Matcha Latte", "Clasic cu lapte de ovăz", 22.50m, 300, 120),
    new Matcha("Ceremonial Matcha", "Pudră premium din Japonia", 35.00m, 100, 5),
    new Matcha("Iced Matcha Lemonade", "Răcoritor și energizant", 18.00m, 400, 85)
};

var matcherie1 = new Matcherie("Matcha Zen Bucuresti", "08:00 - 22:00", 20, meniuBucuresti, new List<Rezervare>());

// 2. Inițializare Admin și Client
var admin = new AdministratorMatcha("Admin Principal", "ADM01", "parola123", new List<Matcherie> { matcherie1 });
var client = new Client("Andrei", "andrei@email.com", new List<Tranzactie>(), new List<Rezervare>());

AnsiConsole.Write(new FigletText("Matcha App Demo").Color(Color.Green));

bool ruleaza = true;
while (ruleaza)
{
    var optiune = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[bold yellow]Ce dorești să testezi?[/]")
            .AddChoices(new[] {
                "Vezi Restaurante", 
                "Comandă Matcha", 
                "Rezervă o Masă", 
                "Anulează Rezervare", 
                "Info Admin (Raport)", 
                "Ieșire"
            }));

    switch (optiune)
    {
        case "Vezi Restaurante":
            client.AfiseazaRestauranteSiAlegeUnul(admin.Matcherii);
            break;

        case "Comandă Matcha":
            string magazinNume = client.AfiseazaRestauranteSiAlegeUnul(admin.Matcherii);
            if (magazinNume != null)
            {
                client.Comanda(magazinNume, admin.Matcherii, admin);
            }
            break;

        case "Rezervă o Masă":
            // Alegem primul magazin pentru simplitate in demo
            var magazinPtRezervare = admin.Matcherii[0];
            client.rezervaMasa(magazinPtRezervare, admin);
            break;

        case "Anulează Rezervare":
            client.anuleazaRezervare();
            break;

        case "Info Admin (Raport)":
            admin.informatii();
            break;

        case "Ieșire":
            ruleaza = false;
            break;
    }

    if (ruleaza)
    {
        AnsiConsole.MarkupLine("\n[grey]Apasă orice tastă pentru a reveni la meniu...[/]");
        Console.ReadKey(true);
        Console.Clear();
    }
}