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
            // UTF-8 "safe": dacă mediul nu permite, nu crăpăm aplicația
            TryEnableUtf8();

            using IHost host = CreateHost(args);

            // pentru cazurile în care folosești ServiceLocator.Get<T>()
            ServiceLocator.Provider = host.Services;

            // rulează aplicația prin runner (cu logger)
            host.Services.GetRequiredService<AppRunner>().Run();
        }

        private static void TryEnableUtf8()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
                Console.InputEncoding = Encoding.UTF8;
            }
            catch
            {
                // unele console/IDE-uri nu permit setarea => continuăm cu default
            }
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
                    services.AddSingleton<AppRunner>();
                    services.AddSingleton<SistemCoordinator>();
                })
                .Build();
        }
    }
}