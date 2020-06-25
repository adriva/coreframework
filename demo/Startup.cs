using System;
using Adriva.Storage.Abstractions;
using Adriva.Storage.Azure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo
{
    public class Startup
    {
        private bool DisableAnalytics = true;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });

            services
                .AddControllersWithViews()
                .AddRazorRuntimeCompilation();

            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.AddOptimization(options =>
            {
                options.BundleStylesheets = true;
                options.MinifyStylesheets = true;
                options.BundleJavascripts = true;
                options.MinifyJavascripts = true;
                options.MinifyHtml = false;
                //options.AssetRootUrl = "https://localhost:5001/assets/";
            })
            .ConfigureStyleSheetMinification(cssOptions =>
            {
                cssOptions.Substitutions.Add("deneme", "ahahahaha");
            });

            services
                .AddStorage(builder =>
                {
                    builder.AddAzureBlob(ServiceLifetime.Singleton, (b) =>
                    {
                        b.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=adriva;AccountKey=nQTYr6G1G00k+mUR370Ar7J0Spv+gbPWRCAyeTILHMF8KdHElRmy/xhiik8Uz1CQ2vohOzP6DsJUzGylFiTDlw==";
                        b.ContainerName = "jarrtcontent";
                    });

                    builder.AddQueueClient<AzureQueueClient>(ServiceLifetime.Singleton)
                        .Configure<AzureQueueConfiguration>(q =>
                        {
                            q.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=adriva;AccountKey=nQTYr6G1G00k+mUR370Ar7J0Spv+gbPWRCAyeTILHMF8KdHElRmy/xhiik8Uz1CQ2vohOzP6DsJUzGylFiTDlw==";
                            q.DefaultTimeToLive = TimeSpan.FromMinutes(1);
                            q.QueueName = "deneme";
                            q.UseSerializer<DefaultQueueMessageSerializer>();
                        });

                    builder.AddAzureTable(ServiceLifetime.Singleton, t =>
                    {
                        t.ConnectionString = "DefaultEndpointsProtocol=https;AccountName=adriva;AccountKey=nQTYr6G1G00k+mUR370Ar7J0Spv+gbPWRCAyeTILHMF8KdHElRmy/xhiik8Uz1CQ2vohOzP6DsJUzGylFiTDlw==";
                        t.TableName = "JarrtDomainInfo";
                    });
                });
            ;

            services.AddWebControls().AddRenderer<Adriva.Web.Controls.Abstractions.NullControlRenderer>("nullrenderer");

            if (!DisableAnalytics)
            {
                services.AddAppInsightsWebAnalytics(options =>
                {
                    options.InstrumentationKey = "Deneme";
                    options.IsDeveloperMode = true;
                    options.Capacity = 10;
                    options.EndPointAddress = "https://localhost:5001/analytics/track";
                });

                services.AddAppInsightsAnalyticsServer(builder =>
                {
                    builder
                        .SetProcessorThreadCount(1)
                        .SetBufferCapacity(10)
                        .UseInMemoryRepository()
                   ;
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            if (!DisableAnalytics) app.UseAnalyticsServer("/analytics");

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseOptimization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
