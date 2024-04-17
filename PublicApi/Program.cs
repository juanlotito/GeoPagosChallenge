using Npgsql;
using PublicApi.Services.Interface;
using PublicApi.Services;
using System.Data;
using PublicApi.Utils;
using PublicApi.Repositories.Interface;
using Microsoft.Extensions.Configuration;
using PublicApi.Models.PaymentProcessor;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// Queue
builder.Services.AddHostedService<QueuedHostedService>();

// Services
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IExternalPaymentProcessor, ExternalPaymentProcessor>();

builder.Services.AddHttpClient();

// Repositories
builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();

var paymentProcessorConfig = new PaymentProcessorConfig();
builder.Configuration.GetSection("PaymentProcessorUri").Bind(paymentProcessorConfig);
builder.Services.AddSingleton(paymentProcessorConfig);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddTransient<IDbConnection>((sp) => new NpgsqlConnection(connectionString));

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Listen(System.Net.IPAddress.Loopback, 5000);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();  
app.MapControllers();

if (args.Contains("--run-tests"))
{
    return;
}   

app.Run();
