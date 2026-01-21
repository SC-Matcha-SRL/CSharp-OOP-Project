using System;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ConsoleApp5
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TryEnableUtf8();

            using IHost host = CreateHost(args);

            LocatieServicii.Provider = host.Services;

            host.Services.GetRequiredService<RulareAplicatie>().Run();
        }

        private static void TryEnableUtf8()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch { }
        }

        private static IHost CreateHost(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddSimpleConsole(o =>
                    {
                        o.SingleLine = true;
                        o.TimestampFormat = "HH:mm:ss ";
                    });
                    logging.SetMinimumLevel(LogLevel.Information);
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<RulareAplicatie>();
                    services.AddSingleton<CoordonatorSistem>();
                })
                .Build();
        }
    }
}