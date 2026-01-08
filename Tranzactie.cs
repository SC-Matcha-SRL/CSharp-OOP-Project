namespace ConsoleApp5;
using Spectre.Console;
using System.Text.Json.Serialization;
public class Tranzactie
{
    public string Id { get; private set; }
    public DateTime Data { get; private set; }
    public decimal suma  { get; private set; }
    public Matcherie Matcherie { get; private set; }
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
    public string Tip { get; set; }
    public decimal Pret { get; set; }
    public string Limitari { get; set; }
    public string Beneficii { get; set; }
    public string ClientID { get; set; }
    public Matcherie Matcherie { get; set; }
    [JsonConstructor]
    public Rezervare(string tip, decimal pret, string limitari, string beneficii, string clientID, Matcherie matcherie)
    {
        Tip = tip;
        Pret = pret;
        Limitari = limitari;
        Beneficii = beneficii;
        ClientID = clientID;
        Matcherie = matcherie;
    }
    
    
    public void CreazaInteractiv()
    {
        AnsiConsole.Write(new Rule("[yellow]Configurare Rezervare Nouă[/]"));

        this.Tip = AnsiConsole.Ask<string>("Introduceți tipul rezervării (ex: Masa 2 pers):");
        this.Pret = AnsiConsole.Ask<decimal>("Prețul rezervării:");
        this.Limitari = AnsiConsole.Ask<string>("Introduceți limitările (sau 'Niciuna'):");
        this.Beneficii = AnsiConsole.Ask<string>("Introduceți beneficiile:");
        
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
        Limitari = noileLimitari;
    }

    public void SetBeneficii(string noileBeneficii)
    {
        Beneficii = noileBeneficii;
    }

    public void SetClientID(string clid)
    {
        ClientID = clid;
    }
    
}