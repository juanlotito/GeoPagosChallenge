using PublicApi.Services.Interface;
using PublicApi.Services;
using PublicApi.Utils;
using PublicApi.Repositories.Interface;
using PublicApi.Models.PaymentProcessor;
using Microsoft.OpenApi.Models;
using PublicApi.Utils.RabbitMQ.Interface;
using PublicApi.Utils.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "GeoPagos Challenge: PublicAPI", Version = "v1" });
});

//SERVICIOS
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
builder.Services.AddScoped<IExternalPaymentProcessor, ExternalPaymentProcessor>();
builder.Services.AddSingleton<IPaymentRepository, PaymentRepository>();
builder.Services.AddTransient<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRabbitMQConsumer>(provider =>
{
    var rabbitMQService = provider.GetService<IRabbitMQService>();
    var paymentRepository = provider.GetService<IPaymentRepository>();
    return new RabbitMQConsumer("payment_confirmation", paymentRepository);
});
builder.Services.AddHttpClient();

// CONFIGS
var paymentProcessorConfig = new PaymentProcessorConfig();
builder.Configuration.GetSection("PaymentProcessorUri").Bind(paymentProcessorConfig);
builder.Services.AddSingleton(paymentProcessorConfig);

//QUEUE SERVICE
builder.Services.AddHostedService<QueuedHostedService>();

//KESTREL CONFIG PARA DOCKER
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
});

var app = builder.Build();

//SWAGGER SIN PREFIJO, SOLO EN DEVELOPMENT
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
