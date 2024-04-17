using Npgsql;
using PublicApi.Services.Interface;
using PublicApi.Services;
using System.Data;
using PublicApi.Utils;
using PublicApi.Repositories.Interface;
using PublicApi.Models.PaymentProcessor;
using Microsoft.OpenApi.Models;
using PublicApi.Utils.RabbitMQ.Interface;
using PublicApi.Utils.RabbitMQ;
using dotenv.net;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// Services
builder.Services.AddSingleton<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddSingleton<IExternalPaymentProcessor, ExternalPaymentProcessor>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRabbitMQConsumer>(provider =>
{
    var rabbitMQService = provider.GetService<IRabbitMQService>();
    var paymentRepository = provider.GetService<IPaymentRepository>();
    return new RabbitMQConsumer("payment_confirmation", paymentRepository);
});

builder.Services.AddHttpClient();

var paymentProcessorConfig = new PaymentProcessorConfig();
builder.Configuration.GetSection("PaymentProcessorUri").Bind(paymentProcessorConfig);
builder.Services.AddSingleton(paymentProcessorConfig);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IDbConnection>((sp) => new NpgsqlConnection(connectionString));

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
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
