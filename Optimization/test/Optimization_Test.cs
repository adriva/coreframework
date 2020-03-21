using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Adriva.Extensions.Optimization.Abstractions;
using Xunit;
using System.IO;
using System.Linq;

namespace Adriva.Extensions.Optimization.Test
{
    public class Optimization_Test
    {
        private IServiceProvider ServiceProvider;

        public Optimization_Test()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddConsole();
            });
            services.AddOptimization(options =>
            {

            })
            .AddTransformationChain("js", typeof(MergeContentTransform));
            this.ServiceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public void EnsureNoDuplicates()
        {
            var manager = this.ServiceProvider.GetService<IOptimizationManager>();
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IOptimizationContext>();

                Assert.True(null != manager);
                Assert.True(null != context);

                context.AddAsset("https://code.jquery.com/jquery-3.4.1.js");
                context.AddAsset("https://code.jquery.com/jquery-3.4.1.js");

                Assert.True(1 == context.Assets.Count); //make sure added once
            }
        }

        [Fact]
        public async Task RunMergeTransformOnJsAsyn()
        {
            var manager = this.ServiceProvider.GetService<IOptimizationManager>();
            using (var scope = this.ServiceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<IOptimizationContext>();

                Assert.True(null != manager);
                Assert.True(null != context);

                context.AddAsset("https://code.jquery.com/jquery-3.4.1.js");
                context.AddAsset("https://maxcdn.bootstrapcdn.com/bootstrap/3.4.1/js/bootstrap.js");

                var outputs = await manager.OptimizeAsync(context, "js");

                Assert.True(null != outputs && 1 == outputs.Count());
            }
        }
    }
}