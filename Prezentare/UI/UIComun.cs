using System.Threading;
using Spectre.Console;

namespace ConsoleApp5
{
    public static class UIComun
    {
        public static void Pauza()
        {
            AnsiConsole.MarkupLine($"\n[grey]{UiText.PressAnyKey}[/]");
            Console.ReadKey(true);
        }

        public static void SalvareSistem(SistemMatcha sistem)
        {
            AnsiConsole.Status().Start(UiText.SavingData, ctx =>
            {
                GestiuneDate.SalveazaTot(sistem);
                Thread.Sleep(250);
            });
        }
    }
}