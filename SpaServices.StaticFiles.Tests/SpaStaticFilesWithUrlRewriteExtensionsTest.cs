using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Two.AspNetCore.SpaServices.StaticFiles
{
    public class SpaStaticFilesWithUrlRewriteExtensionsTest
    {
        [Fact]
        public void UseSpa_NoExceptionThrown()
        {
            var services = GetServiceCollection();
            services.AddSpaStaticFilesWithUrlRewrite(configuration =>
            {
                configuration.RootPath = "Root";
            });

            var serviceProvider = services.BuildServiceProvider();
            var applicationbuilder = GetApplicationBuilder(serviceProvider);
            applicationbuilder.UseSpa(x => { });
        }

        [Fact]
        public void UseSpa_ThrowsInvalidOperationException_IfRootPathNotSet()
        {
            var services = GetServiceCollection();
            services.AddSpaStaticFilesWithUrlRewrite(_ => { });

            var serviceProvider = services.BuildServiceProvider();
            var applicationbuilder = GetApplicationBuilder(serviceProvider);
            var exception = Assert.Throws<InvalidOperationException>(
                () => applicationbuilder.UseSpa(_ => { }));
            Assert.Contains(nameof(UrlRewriteSpaStaticFilesOptions.RootPath), exception.Message);
        }

        private IApplicationBuilder GetApplicationBuilder(IServiceProvider serviceProvider)
        {
            var applicationbuilderMock = new Mock<IApplicationBuilder>();
            applicationbuilderMock
                .Setup(s => s.ApplicationServices)
                .Returns(serviceProvider);
            return applicationbuilderMock.Object;
        }

        private ServiceCollection GetServiceCollection()
        {
            var hostingEnvironmentMock = new Mock<Microsoft.AspNetCore.Hosting.IHostingEnvironment>();
            hostingEnvironmentMock
                .Setup(s => s.ContentRootPath)
                .Returns("ContentRoot");
            var services = new ServiceCollection();
            services.AddLogging();
            services.TryAddSingleton(hostingEnvironmentMock.Object);
            return services;
        }
    }
}
