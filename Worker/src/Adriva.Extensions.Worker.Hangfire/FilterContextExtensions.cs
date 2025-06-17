using System;
using Hangfire.Client;

namespace Adriva.Extensions.Worker.Hangfire
{
    public static class FilterContextExtensions
    {
        public static TContext AddServiceProvider<TContext>(this TContext createContext, IServiceProvider serviceProvider) where TContext : CreateContext
        {
            createContext.Items[HangfireDefaults.ServiceProviderContextKey] = serviceProvider;
            return createContext;
        }

        public static IServiceProvider GetServiceProvider(this CreateContext createContext)
        {
            if (!createContext.Items.TryGetValue(HangfireDefaults.ServiceProviderContextKey, out object boxedProvider))
            {
                return null;
            }

            return boxedProvider as IServiceProvider;
        }
    }
}
