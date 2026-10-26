# ASP.NET Core Built-in Middleware Components

This project demonstrates serving static files, choosing a web root, configuring a default page, and handling exceptions with the Developer Exception Page. The examples are in [Program.cs](./Program.cs).

## Where should static files be stored?

Put public files such as CSS, JavaScript, images, and client-side libraries in the application's web root. By default, ASP.NET Core uses the `wwwroot` folder at the project root. For example:

```text
wwwroot/
  css/site.css
  images/logo.png
  index.html
```

Files outside the web root are not served by the static-files middleware by default. Do not put secrets, private uploads, or server-side source files in a publicly served web root.

## What is the `wwwroot` folder?

`wwwroot` is the default public web root: the base directory from which static-file requests are served. For example, `wwwroot/css/site.css` is requested at `/css/site.css` (the `wwwroot` directory name is not part of the URL).

Files are served only when static-file middleware is registered. Serving a file this way bypasses later middleware in the request pipeline.

## How do I configure Static Files Middleware?

Register `UseStaticFiles` on the application pipeline:

```csharp
app.UseStaticFiles();
```

Place it before endpoint mappings that should not handle static-file requests. You can customize response headers and other file-serving behavior with `StaticFileOptions`; the existing `Program.cs` example sets a 30-day cache lifetime:

```csharp
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = context =>
        context.Context.Response.Headers["Cache-Control"] = "public,max-age=2592000"
});
```

For a normal application, register static-file middleware once. If you use default-file rewriting, put `UseDefaultFiles` before `UseStaticFiles`, as shown below.

## How do I use my own web root folder?

Set `WebRootPath` before building the application. The path is relative to the content root (normally the project directory). Create the folder in the project and place the public assets there.

```csharp
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "MyWebRoot"
});

var app = builder.Build();
app.UseStaticFiles();
```

With this configuration, `MyWebRoot/images/logo.png` is served at `/images/logo.png`. The default `wwwroot` folder is no longer the configured web root. The commented example in `Program.cs` also shows `WebRootPath` and the resulting environment paths.

## How do I configure a default page?

`UseDefaultFiles` checks a directory request for a default document and rewrites the request path to that file. It does not serve the file itself, so follow it with `UseStaticFiles`:

```csharp
app.UseDefaultFiles();
app.UseStaticFiles();
```

The default names are `default.htm`, `default.html`, `index.htm`, and `index.html`. For example, a request for `/` can resolve to `wwwroot/index.html`. To use a different name:

```csharp
var options = new DefaultFilesOptions();
options.DefaultFileNames.Clear();
options.DefaultFileNames.Add("home.html");

app.UseDefaultFiles(options);
app.UseStaticFiles();
```

The default-file middleware only searches the web root (or configured custom web root). `UseFileServer()` is a convenience option that combines default-file handling and static-file serving; directory browsing remains disabled unless explicitly enabled.

## What is Developer Exception Page Middleware?

The Developer Exception Page (`UseDeveloperExceptionPage`) catches unhandled exceptions from middleware that runs after it and displays diagnostic information useful during development. The details can include exception messages, stack traces, request headers, and source context, so it must not be exposed to production users.

## How do I use the Developer Exception Page?

Enable it only in the Development environment. For other environments, use a production-safe error handler:

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
```

`/Error` should be an appropriate error endpoint or page in the application. ASP.NET Core templates and hosting configurations may already enable the Developer Exception Page in Development; register it explicitly when your app's setup requires it, and avoid adding it more than once.

## How can I customize `UseDeveloperExceptionPage`?

Pass `DeveloperExceptionPageOptions` to adjust the amount of source context shown around an error:

```csharp
app.UseDeveloperExceptionPage(new DeveloperExceptionPageOptions
{
    SourceCodeLineCount = 10
});
```

Customization is intended for local development diagnostics, not for exposing detailed exception output to clients in production.

## Where should `UseDeveloperExceptionPage` be configured?

Configure exception-handling middleware near the beginning of the pipeline, before middleware that may throw (such as static-file, routing, or endpoint middleware). Exception-handling middleware can only catch exceptions from components that run after it.

In the current `Program.cs`, the Development/Production exception-handling branch appears after the static-file and default-file middleware registrations. Move that branch above them if it should catch exceptions from the entire pipeline. In production, configure `UseExceptionHandler` (and typically `UseHsts`) in the corresponding non-Development branch instead.
