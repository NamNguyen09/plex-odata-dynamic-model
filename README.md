# ASP.NET Core OData 8.x with dynamic model builder
## 1. Introduction
This is a server side library depending on `Microsoft.AspNetCore.OData` nuget package that help to dynamically create an EDM model, and bind it into Web API OData pipeline.
## 2. Usage
In the ASP.NET Core Web Application project, update your `Program.cs` as below:  
```  
var builder = WebApplication.CreateBuilder(new WebApplicationOptions 
{ 
   ApplicationName = typeof(Program).Assembly.GetName().Name,
   ContentRootPath = Directory.GetCurrentDirectory(),
   WebRootPath = "wwwroot"
});
builder.Services.AddControllers().AddOData(); 
builder.Services.Configure<PlexODataOptions>(options =>
{
    options.RoutePrefix = "myodata";
    options.DynamicControllerName = "HandleAll";
    options.MaxPageSize = 200;
});

builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IApplicationModelProvider, PlexODataRoutingApplicationModelProvider>());
builder.Services.TryAddSingleton<IDataSource, MyDataSource>();
builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<MatcherPolicy, PlexODataRoutingMatcherPolicy>());

var app = builder.Build();
app.UseRouting();
app.MapControllers();
app.Logger.LogInformation("The application started");
app.Run();
```
      
That's it.

## 3. References:
- [ODataRoutingSample](https://github.com/OData/AspNetCoreOData/tree/main/sample/ODataRoutingSample): ASP.NET Core OData sample project in this repo.
  
