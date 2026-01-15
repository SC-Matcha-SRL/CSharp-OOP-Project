using Spectre.Console;

namespace ConsoleApp5
{
    /// <summary>
    /// UI helpers reutilizabile în mai multe meniuri.
    /// </summary>
    public static class CommonUI
    {
        /// <summary>
        /// Pauză standard (așteaptă o tastă).
        /// </summary>
        public static void Pauza()
        {
            AnsiConsole.MarkupLine("\n[grey]Apasă orice tastă pentru a continua...[/]");
            Console.ReadKey(true);
        }

        /// <summary>
        /// Salvează sistemul în JSON cu feedback vizual.
        /// </summary>
        public static void SalvareSistem(SistemMatcha sistem)
        {
            AnsiConsole.Status().Start("Se salvează datele...", ctx =>
            {
                GestiuneDate.SalveazaTot(sistem);
                Thread.Sleep(800);
            });
        }
    }
}