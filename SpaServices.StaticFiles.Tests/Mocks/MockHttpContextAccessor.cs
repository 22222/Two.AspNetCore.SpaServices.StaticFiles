using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Two.AspNetCore.SpaServices.StaticFiles.Mocks
{
    public class MockHttpContextAccessor : IHttpContextAccessor
    {
        public HttpContext HttpContext { get; set; } = new DefaultHttpContext();

        public string PathBase
        {
            get => HttpContext.Request.PathBase;
            set => HttpContext.Request.PathBase = value;
        }
    }
}
