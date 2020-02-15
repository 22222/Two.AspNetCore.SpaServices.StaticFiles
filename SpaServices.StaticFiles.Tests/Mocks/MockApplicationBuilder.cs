using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Text;

namespace Two.AspNetCore.SpaServices.StaticFiles.Mocks
{
    public class MockApplicationBuilder : IApplicationBuilder
    {
        public MockApplicationBuilder(IServiceProvider applicationServices)
        {
            ApplicationServices = applicationServices ?? throw new ArgumentNullException(nameof(applicationServices));
        }

        /// <inheritdoc />
        public IServiceProvider ApplicationServices { get; set; }

        /// <inheritdoc />
        public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();

        /// <inheritdoc />
        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        /// <inheritdoc />
        public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
        {
            return this;
        }

        /// <inheritdoc />
        public RequestDelegate Build()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IApplicationBuilder New()
        {
            throw new NotImplementedException();
        }
    }
}
