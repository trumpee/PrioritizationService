using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Prioritize;
using Serilog;
using Serilog.Events;
using Trumpee.MassTransit;
using Trumpee.MassTransit.Configuration;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("MassTransit", LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var host = CreateHostBuilder(args).Build();
await host.RunAsync();

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureHostConfiguration(config =>
        {
            config.AddEnvironmentVariables();
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        })
        .UseSerilog()
        .ConfigureServices((host, services) =>
        {
            var rabbitTopologyBuilder = new RabbitMqTransportConfigurator();
            rabbitTopologyBuilder.AddExternalConfigurations(x =>
            {
                x.AddConsumer<PrioritizationConsumer>();
            });

            rabbitTopologyBuilder.UseExternalConfigurations((ctx, cfg) =>
            {
                cfg.ReceiveEndpoint("prioritize", e =>
                {
                    e.BindQueue = true;
                    e.PrefetchCount = 16;

                    e.UseConcurrencyLimit(4);
                    e.ConfigureConsumer<PrioritizationConsumer>(ctx);
                });
            }); 
            
            services.AddConfiguredMassTransit(host.Configuration, rabbitTopologyBuilder);
        });
}
