namespace ConsoleApp5;
using Spectre.Console;

public class Tranzactie
{
    private string Id { get; set; }
    private DateTime Data { get; set; }
    private decimal suma  { get; set; }
    private Matcherie Matcherie { get; set; }
    public Tranzactie(string id, DateTime data, decimal suma,  Matcherie matcherie)
    {
        Id = id;
        Data = data;
        this.suma = suma;
        Matcherie = matcherie;
    }
}

public class Rezervare
{
    public string Tip { get; private set; }//masa 2 persoane ex
    private string ClientID { get; set; }
    public decimal Pret { get; private set; }
    public string limitari { get; private set; }
    public string beneficii { get; private set; }

    public Rezervare(string tip, decimal pret, string limitari, string beneficii)
    {
        Tip = tip;
        Pret = pret;
        this.limitari = limitari;
        this.beneficii = beneficii;
        this.ClientID = this.beneficii;
    }
    
    
    public void CreazaInteractiv()
    {
        AnsiConsole.Write(new Rule("[yellow]Configurare Rezervare Nouă[/]"));

        this.Tip = AnsiConsole.Ask<string>("Introduceți tipul rezervării (ex: Masa 2 pers):");
        this.Pret = AnsiConsole.Ask<decimal>("Prețul rezervării:");
        this.limitari = AnsiConsole.Ask<string>("Introduceți limitările (sau 'Niciuna'):");
        this.beneficii = AnsiConsole.Ask<string>("Introduceți beneficiile:");
        
        AnsiConsole.MarkupLine("[green] Datele rezervării au fost colectate cu succes![/]");
    }
    public void SetTip(string noulTip)
    {
        if (!string.IsNullOrWhiteSpace(noulTip))
            Tip = noulTip;
    }

    public void SetPret(decimal noulPret)
    {
        if (noulPret >= 0)
            Pret = noulPret;
    }

    public void SetLimitari(string noileLimitari)
    {
        limitari = noileLimitari;
    }

    public void SetBeneficii(string noileBeneficii)
    {
        beneficii = noileBeneficii;
    }

    public void SetClientID(string clid)
    {
        ClientID = clid;
    }
    
}