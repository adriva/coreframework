using System;
using System.IO;
using Adriva.Storage.Abstractions;
using Adriva.Storage.Azure;
using Adriva.Web.Controls.Abstractions;
using demo.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace demo
{
    public class Startup
    {
        private bool DisableAnalytics = true;

        public IConfiguration Configuration { get; }
        public IHostEnvironment HostingEnvironment { get; }

        public Startup(IConfiguration configuration, IHostEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            this.HostingEnvironment = hostingEnvironment;
        }


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
                bool enableOpt = false;
                options.BundleStylesheets = enableOpt;
                options.MinifyStylesheets = enableOpt;
                options.BundleJavascripts = enableOpt;
                options.MinifyJavascripts = enableOpt;
                options.MinifyHtml = false;
                options.UseFileOrderer("assetorder.txt");
                // options.AssetRootUrl = "https://www.adriva.com";
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

            services.AddWebControls(options =>
            {
                options.OptimizationContextName = "WebControls";
            })
            .AddAssembly(typeof(Adriva.Web.Controls.Grid).Assembly.Location)
            .AddRenderer<Adriva.Web.Controls.Abstractions.NullControlRenderer>("nullrenderer")
            .AddRenderer<Adriva.Web.Controls.BootstrapGridRenderer>();

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

            services.AddDbContext<JarrtDbContext>(builder =>
            {
                builder.UseSqlServer("data source=kwzh9m06kp.database.windows.net;initial catalog=marketplace;persist security info=True;user id=asos_db@kwzh9m06kp;password=AdrivaAdriva1;MultipleActiveResultSets=True;App=EntityFramework", sqlBuilder =>
                {

                });
            });

            services.AddReporting(reportingBuilder =>
            {
                reportingBuilder.UseFileSystemRepository(options =>
                {
                    options.RootPath = Path.Combine(this.HostingEnvironment.ContentRootPath, "Reports");
                });
            });
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

            app.UseWebControls();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
