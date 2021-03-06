An alternative implementation of the ASP.NET Core `ISpaStaticFileProvider` that supports URL rewriting.

[![Build status](https://ci.appveyor.com/api/projects/status/3qb029a77i8el0xq?svg=true)](https://ci.appveyor.com/project/22222/two-aspnetcore-spaservices-staticfiles)

Installation
============
You have a few options for installing this library:

* Install the [NuGet package](https://www.nuget.org/packages/Two.AspNetCore.SpaServices.StaticFiles/)
* Download the assembly from the [latest release](https://github.com/22222/Two.AspNetCore.SpaServices.StaticFiles/releases/latest) and reference it manually
* Copy the source code directly into your project

This project is available under either of two licenses: [MIT](LICENSE) or [The Unlicense](UNLICENSE).  The goal is to allow you to copy any of the source code from this library into your own project without having to worry about attribution or any other licensing complexity.


Getting Started
===============
This library works with ASP.NET Core web projects and the [Microsoft.AspNetCore.SpaServices.Extensions library](https://www.nuget.org/packages/Microsoft.AspNetCore.SpaServices.Extensions).

If you have a project like that, then there's probably some code like this in your `Startup.cs` file `ConfigureServices` method:

```c#
services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});
```

To use this library, just swap the `AddSpaStaticFiles` method for `AddSpaStaticFilesWithUrlRewrite`:

```c#
services.AddSpaStaticFilesWithUrlRewrite(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});
```

Or if you need to customize the URL rewrite process, there are more configuration options available:

```c#
using Two.AspNetCore.SpaServices.StaticFiles;

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
```

And that's it, everything else should be the same.  All of the normal `Configure` method code still applies, like:

```c#
app.UseSpaStaticFiles();
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";
    if (env.IsDevelopment())
    {
        spa.UseReactDevelopmentServer(npmScript: "start");
    }
});
```

For a full example, see the [sample project Startup.cs](SpaServices.StaticFiles.SampleWebApp/Startup.cs).
