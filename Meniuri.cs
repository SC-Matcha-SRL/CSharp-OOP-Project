namespace ConsoleApp5;
using Spectre.Console;
public static class Meniuri
{
    public static void AfiseazaDashboardClient(Client client, SistemMatcha sistem)
    {
        Console.Clear();

        // 1. Creăm structura ecranului: Stânga (Meniu) și Dreapta (Profil + Info)
        var layout = new Layout("Root")
            .SplitColumns(
                new Layout("Meniu").Ratio(2), // Ocupă 2/3 din ecran
                new Layout("Profil").Ratio(1)  // Ocupă 1/3 din ecran
            );

        // 2. Construim tabelul cu toate produsele din toate magazinele (sau primul magazin)
        var tabelProduse = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Green)
            .Title("[bold green]🍵 MENIU MATCHA[/]")
            .AddColumn("Magazin")
            .AddColumn("Produs")
            .AddColumn(new TableColumn("Preț").Centered());

        foreach (var magazin in sistem.Magazine)
        {
            foreach (var produs in magazin.Meniu)
            {
                tabelProduse.AddRow(
                    magazin.Nume, 
                    produs.nume, 
                    $"[yellow]{produs.pret} RON[/]"
                );
            }
        }

        // 3. Construim panoul de profil pentru client
        var profilContent = new Rows(
            new Markup($"[bold]Utilizator:[/] {client.Nume}"),
            new Markup($"[bold]Email:[/] [blue]{client.Email}[/]"),
            new Rule("[yellow]Activitate[/]"),
            new Markup($"[bold]Rezervări:[/] {client.Rezervari.Count}"),
            new Markup($"[bold]Comenzi efectuate:[/] {client.Istoric.Count}"),
            new Rule(),
            new Markup("[grey]Folosește meniul de mai jos pentru acțiuni[/]")
        );

        var panouProfil = new Panel(profilContent)
            .Header("[bold cyan]👤 PROFILUL TĂU[/]")
            .Expand();

        // 4. Actualizăm secțiunile layout-ului cu obiectele create
        layout["Meniu"].Update(new Panel(tabelProduse).Expand());
        layout["Profil"].Update(panouProfil);

        // 5. Afișăm totul pe ecran
        AnsiConsole.Write(layout);
        AnsiConsole.WriteLine();
    }
}