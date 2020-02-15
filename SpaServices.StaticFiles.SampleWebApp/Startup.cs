using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Two.AspNetCore.SpaServices.StaticFiles;

namespace SpaServices.StaticFiles.SampleWebApp
{
    public class Startup : IStartup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        /// <inheritdoc />
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

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
                    configuration.SourcePathBaseSelector = (httpRequest) => httpRequest.Path.StartsWithSegments("test", StringComparison.OrdinalIgnoreCase) ? "./test" : "./";
                    configuration.TargetPathBaseSelector = (httpRequest) => httpRequest.PathBase + '/';
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

            return services.BuildServiceProvider();
        }

        /// <inheritdoc />
        public void Configure(IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            IHostingEnvironment env = app.ApplicationServices.GetRequiredService<IHostingEnvironment>();
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            app.UseMvc();
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
