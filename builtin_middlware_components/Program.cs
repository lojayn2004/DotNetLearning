using static System.Net.WebRequestMethods;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


#region wwwroot Folder in ASP.NET Core


// 1) Key Characteristics of the wwwroot Folder:
// - Default Location: The wwwroot folder sits at the root level of an ASP.NET Core project.
// - Direct Access to Files: Files within wwwroot are directly accessible by clients via relative URLs.
//   For instance, a file named styles.css in wwwroot/css can be accessed as:
//   http://<your-app-url>/css/styles.css
// - Static Files Middleware: To serve files from wwwroot, we must use the UseStaticFiles
//   middleware in the application's middleware pipeline.
// - Enhanced Security: Files located outside wwwroot aren't publicly accessible, which helps
//   safeguard application logic and sensitive files. Only files within wwwroot or subdirectories
//   are served to clients, enhancing security by preventing unauthorized access to server code.



app.UseStaticFiles();



// 2) To rename the wwwroot folder

/*
//Step1: Creating an Instance of WebApplicationOptions Class
WebApplicationOptions webApplicationOptions = new WebApplicationOptions
{
    WebRootPath = "MyWebRoot", //Setting the WebRootPath as MyWebRoot
    Args = args, //Setting the Command Line Arguments in Args
    EnvironmentName = "Production", //Changing to Production
};
//Step2: Pass WebApplicationOptions Instance to the CreateBuilder Method
var builder02 = WebApplication.CreateBuilder(webApplicationOptions);

var app02 = builder02.Build();
app02.MapGet("/", () => $"EnvironmentName: {app02.Environment.EnvironmentName} \n" +
$"ApplicationName: {app02.Environment.ApplicationName} \n" +
$"WebRootPath: {app02.Environment.WebRootPath} \n" +
$"ContentRootPath: {app02.Environment.ContentRootPath}");
//This will Run the Application
app02.Run();
*/

#endregion


#region Static Files Middleware in ASP.NET Core

// When a client requests a file, such as /logo.png, here’s how UseStaticFiles() processes the request:

// 1) Request Interception:
//             When the UseStaticFiles() middleware is registered, it intercepts requests for file paths.

// 2) Path Matching:
//             The middleware examines the request path. If the path corresponds to a file in the static file directory,
//             i.e., wwwroot (or the custom folder specified with WebRootPath), it prepares the file for delivery.

// 3) Serving the File:
//             If a matching file is found, the middleware sends it directly to the client.
//             The file is sent with appropriate headers, including Content-Type, based on its extension.

// 4) Skipping Application Logic:
//             By serving static files directly, the middleware bypasses the rest of the ASP.NET Core pipeline,
//             improving performance by reducing unnecessary processing for static assets.

// 5) Caching and Security:
//             To improve performance and control access, we can configure caching headers and set security rules for serving static files.


app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Cache static files for 30 days
        ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=2592000";
    }
});
#endregion


#region Configuring Default Page in ASP.NET Core

// ─────────────────────────────────────────────────────────────────────────────
// WHAT IT DOES
// UseDefaultFiles() is a URL REWRITER — it does NOT serve files.
// If the request URL points to a DIRECTORY (ends with "/"), it looks for a
// default file inside that directory and rewrites the path to point at it.
// The actual file is then served by UseStaticFiles(), which must come AFTER.
//
// Default file names it searches for (in order):
//   default.htm, default.html, index.htm, index.html
// ─────────────────────────────────────────────────────────────────────────────

// 1. Use Default Files (URL rewriter) — MUST be before UseStaticFiles
app.UseDefaultFiles();

// ─────────────────────────────────────────────────────────────────────────────
// BEHAVIOR NOTES
// • Applies to ANY directory, not just the root. Examples (given wwwroot/):
//       GET /              → rewritten to /index.html        
//       GET /docs/         → rewritten to /docs/index.html   
//       GET /docs/api/     → rewritten to /docs/api/index.html 
//       GET /images/       → no default file there → 404     
//
// • TRAILING SLASH MATTERS:
//       /docs/   → treated as a directory  → rewrite works   
//       /docs    → NOT treated as directory → 404           
//
// • Default file names are GLOBAL, not per-folder. You cannot say
//   "root uses index.html but docs/ uses home.html".
//
// • Only operates INSIDE the web root (wwwroot, or your custom WebRootPath).
//   Files outside wwwroot are never served as default files.
// ─────────────────────────────────────────────────────────────────────────────

// Optional: customize the global list of default file names (applies to ALL folders)
// var defaultFilesOptions = new DefaultFilesOptions();
// defaultFilesOptions.DefaultFileNames.Clear();
// defaultFilesOptions.DefaultFileNames.Add("myindex.html");
// app.UseDefaultFiles(defaultFilesOptions);   // use this instead of the plain call above

// 2. Static Files Middleware (actually serves the file at the rewritten path)
app.UseStaticFiles();

// ─────────────────────────────────────────────────────────────────────────────
// SHORTCUT
// UseFileServer() == UseDefaultFiles() + UseStaticFiles() (in the correct order),
// plus optional directory browsing. Prefer this to avoid ordering mistakes:
//
//     app.UseFileServer();
//
// (Directory browsing is OFF by default; enable with:
//     app.UseFileServer(enableDirectoryBrowsing: true);
//  but never enable it in production — it exposes your folder structure.)
// ─────────────────────────────────────────────────────────────────────────────
// app.UseFileServer();

#endregion


#region Configuring Developer Exception Page Middleware

// Get the current environment information (usually injected via builder.Environment or app.Environment)
// In Minimal API, you can directly use app.Environment
if (app.Environment.IsDevelopment())
{
    // Enable Developer Exception Page (only for development environment)
    // It will catch exceptions from subsequent middleware and display detailed error information
    app.UseDeveloperExceptionPage();
}
else
{
    // Use friendly error page in production environment
    app.UseExceptionHandler("/Error");
    // It is also recommended to add HSTS here
    // app.UseHsts();
}

// Note: Exception handling middleware should be placed at the very beginning of the pipeline
// For example, before UseStaticFiles, UseRouting, etc.

#endregion

app.Run();
