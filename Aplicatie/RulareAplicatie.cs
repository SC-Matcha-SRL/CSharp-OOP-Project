using System;
using Microsoft.Extensions.Logging;

namespace ConsoleApp5
{
    public sealed class RulareAplicatie
    {
        private readonly ILogger<RulareAplicatie> _logger;

        public RulareAplicatie(ILogger<RulareAplicatie> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("Application starting...");

            try
            {
                Aplicatie.Run();
                _logger.LogInformation("Application exited normally.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fatal error in application.");
                throw;
            }
        }
    }
}