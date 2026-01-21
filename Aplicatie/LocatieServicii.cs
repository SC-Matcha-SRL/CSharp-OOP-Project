using System;
using Microsoft.Extensions.DependencyInjection;

namespace ConsoleApp5
{
    public static class LocatieServicii
    {
        public static IServiceProvider? Provider { get; set; }

        public static T Get<T>() where T : notnull
        {
            if (Provider == null)
                throw new InvalidOperationException("LocatieServicii is not initialized.");

            return Provider.GetRequiredService<T>();
        }
    }
}