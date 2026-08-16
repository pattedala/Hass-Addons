using System.Text.Json;
using MQTTGatewayClient.Configuration;
using MQTTGatewayClient.Services;

var builder = Host.CreateApplicationBuilder(args);

var optionsJson =
    await File.ReadAllTextAsync("/data/options.json");

var options =
    JsonSerializer.Deserialize<GatewayOptions>(optionsJson)
    ?? throw new InvalidOperationException(
        "Unable to read Home Assistant add-on configuration.");

if (string.IsNullOrWhiteSpace(options.IoTHubConnectionString))
{
    throw new InvalidOperationException(
        "IoTHubConnectionString is not configured.");
}

builder.Services.AddSingleton(options);

builder.Services.AddSingleton<IoTHubService>();

builder.Services.AddHostedService<MqttService>();

var host = builder.Build();

await host.RunAsync();