using System.IO;
using PharmacyTracker.Api.Middleware;
using PharmacyTracker.Api.Models;
using PharmacyTracker.Api.Repositories;
using PharmacyTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// File Paths configuration
var contentRoot = builder.Environment.ContentRootPath;
var dataDir = Path.Combine(contentRoot, "Data");
var medicinesPath = builder.Configuration["FileStorage:MedicinesPath"] ?? Path.Combine(dataDir, "medicines.json");
var salesPath = builder.Configuration["FileStorage:SalesPath"] ?? Path.Combine(dataDir, "sales.json");

// Register repositories as Singletons to maintain SemaphoreSlim locking across concurrent requests.
builder.Services.AddSingleton<IJsonRepository<Medicine>>(sp => new JsonRepository<Medicine>(medicinesPath));
builder.Services.AddSingleton<IJsonRepository<SaleRecord>>(sp => new JsonRepository<SaleRecord>(salesPath));

// Services
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<ISaleService, SaleService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Serve SPA files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

// Fallback to SPA index.html for client side routing
app.MapFallbackToFile("index.html");

app.Run();
