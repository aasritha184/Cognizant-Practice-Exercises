using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Step 1 Requirement: Configure Swagger Generation with metadata
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Swagger Demo",
        Version = "v1",
        Description = "ASP.NET Core Web API with Swagger and Postman Demonstration",
        TermsOfService = new Uri("http://www.example.com/terms"),
        Contact = new OpenApiContact()
        {
            Name = "John Doe",
            Email = "john@xyzmail.com",
            Url = new Uri("http://www.example.com")
        },
        License = new OpenApiLicense()
        {
            Name = "License Terms",
            Url = new Uri("http://www.example.com")
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Step 1 Requirement: Enable Swagger & Swagger UI middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Specifying the Swagger JSON endpoint.
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger Demo");
    c.RoutePrefix = "swagger";
});

app.UseAuthorization();

app.MapControllers();

app.Run();
