using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Application.Services;
using NPVCalculator.Application.Validators;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<INpvCalculatorService, NpvCalculatorService>();

// Add services to the container.  
builder.Services.AddControllers();

// Enables in-memory caching in .NET app. Think of it like adding
// a mini 'fast storage' inside app's RAM. Once added, code can use
// .NET's built-in IMemoryCache to store and reuse results instead
// of recalculating them every time.
builder.Services.AddMemoryCache();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<NpvRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NPV Calculator API",
        Version = "v1"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});


var app = builder.Build();

// Configure the HTTP request pipeline.  
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/swagger");
        return;
    }
    await next();
});

app.Run();


public partial class Program { }