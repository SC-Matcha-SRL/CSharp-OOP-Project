using Spectre.Console;

namespace ConsoleApp5
{
    public static class Aplicatie
    {
        public static void Run()
        {
            var sistem = GestiuneDate.IncarcaTot();
            sistem.AsiguraColectii();

            // Seed demo daca nu exista admini
            if (sistem.Administratori.Count == 0)
            {
                SeedDateTest.IncarcaDateTest(sistem);
                UIComun.SalvareSistem(sistem);
            }

            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("X Matcha").Color(Color.Green));
            AnsiConsole.WriteLine();

            bool ruleaza = true;
            while (ruleaza)
            {
                string actiune = EcranStart.AfiseazaEcranStartSiAlegeRol(sistem);

                switch (actiune)
                {
                    case "Client":
                        FluxClient.Run(sistem);
                        break;

                    case "Administrator":
                        FluxAdmin.Run(sistem);
                        break;

                    case "CreareCont":
                        ServiciiCont.CreeazaContClient(sistem);
                        break;

                    case "Iesire":
                        UIComun.SalvareSistem(sistem);
                        ruleaza = false;
                        break;
                }
            }
        }
    }
}