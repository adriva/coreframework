
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adriva.Extensions.Caching.Abstractions;
using Xunit;

namespace Adriva.Extensions.Caching.Memory.Test
{
    public class MemoryCache_Test
    {
        private IServiceProvider ServiceProvider;

        public MemoryCache_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
            });
            services.AddInMemoryCache();
            this.ServiceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task CheckCacheExpiresAsync()
        {
            var cache = this.ServiceProvider.GetService<ICache>();

            var item = await cache.GetOrCreateAsync<object>("test", async item =>
            {
                await Task.CompletedTask;
                item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return new object();
            });

            Assert.True(null != item); // make sure item is cached

            var newItem = await cache.GetOrCreateAsync<object>("test", async item =>
            {
                await Task.CompletedTask;
                item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return new object();
            });

            Assert.Same(item, newItem); // make sure item is not created again

            await Task.Delay(6000);

            newItem = await cache.GetOrCreateAsync<object>("test", async item =>
            {
                await Task.CompletedTask;
                item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return new object();
            });

            Assert.NotSame(item, newItem); // make sure item has expired
        }


        [Fact]
        public async Task CheckDependencyExpirationAsync()
        {
            var cache = this.ServiceProvider.GetService<ICache>();

            var item = await cache.GetOrCreateAsync<object>("test", async item =>
            {
                await Task.CompletedTask;
                item.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return new object();
            }, "Dependency");

            cache.NotifyChanged("Dependency");

            var newItem = await cache.GetOrCreateAsync<object>("test", async item =>
            {
                await Task.CompletedTask;
                item.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return new object();
            }, "Dependency");

            Assert.NotSame(item, newItem);
        }
    }
}
