using ServiceContracts;
using Services;
using Microsoft.EntityFrameworkCore;
using Entities;
using RepositoryContracts;
using Repositories;
using Serilog;
using CRUDDemmo.Filters.ActionFilters;
using CRUDDemmo;
using CRUDDemmo.Middleware;

var builder = WebApplication.CreateBuilder(args);


//Serilog
builder.Host.UseSerilog((HostBuilderContext context, IServiceProvider services, LoggerConfiguration loggerConfiguration) => {
    loggerConfiguration
    .ReadFrom.Configuration(context.Configuration) //read configuration settings from built-in IConfiguration
    .ReadFrom.Services(services); //read out current app's services and make them available to serilog
});

builder.Services.ConfigureServices(builder.Configuration);

var app = builder.Build();

app.UseSerilogRequestLogging();

//create application pipeline
if (builder.Environment.IsDevelopment()) {
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseExceptionHandlingMiddleware();
}
app.UseHsts();
app.UseHttpsRedirection();

if (builder.Environment.IsEnvironment("Test")==false)
Rotativa.AspNetCore.RotativaConfiguration.Setup("wwwroot", wkhtmltopdfRelativePath: "Rotativa");

app.UseHttpLogging();
app.UseStaticFiles();
app.UseRouting(); //Identifying action methods based rout
app.UseAuthentication(); //Reading Identity cookie
app.UseAuthorization(); // Validates access permissions of the user
app.MapControllers(); //Excute the filter pipline (action+filters)

app.UseEndpoints(endpoints => {
    endpoints.MapControllerRoute(
     name: "areas",
     pattern: "{area:exists}/{controller=Home}/{action=Index}");

    //Admin/Home/Index
    //Admin
    endpoints.MapControllerRoute(
     name: "default",
     pattern: "{controller}/{action}/{id?}"
     );
});

app.Run();

public partial class Program { }
