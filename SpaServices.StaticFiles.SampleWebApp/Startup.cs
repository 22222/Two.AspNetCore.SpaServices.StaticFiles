using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Two.AspNetCore.SpaServices.StaticFiles;

#pragma warning disable CA1822 // Mark members as static

namespace SpaServices.StaticFiles.SampleWebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            // This if/else block is just for demo purposes.  In a real app you should just pick one configuration.
            if (Configuration.GetValue<bool?>("SuppressUrlRewrite") == true)
            {
                // In production, the React files will be served from this directory
                services.AddSpaStaticFiles(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                });
            }
            else if (Configuration.GetValue<bool?>("UseVerboseConfiguration") == true)
            {
                services.AddSpaStaticFilesWithUrlRewrite(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                    configuration.SourcePathBase = "./";
                    configuration.SourcePathBaseSelector = (httpRequest) => httpRequest?.Path.StartsWithSegments("test", StringComparison.OrdinalIgnoreCase) == true ? "./test" : "./";
                    configuration.TargetPathBaseSelector = (httpRequest) => (httpRequest?.PathBase ?? string.Empty) + '/';
                    configuration.MaxFileLengthForRewrite = 10000;
                    configuration.UpdateBaseElementHrefOnly = true;
                    configuration.Rewriters = new ISpaStaticFilesUrlRewriter[]
                    {
                        new HtmlUrlRewriter(),
                        new ServiceWorkerJsUrlRewriter(),
                    };
                });
            }
            else
            {
                services.AddSpaStaticFilesWithUrlRewrite(configuration =>
                {
                    configuration.RootPath = "ClientApp/build";
                });
            }
        }

        public void Configure(IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            IWebHostEnvironment env = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }
    }
}
