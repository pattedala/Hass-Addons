using System.Text.Json;
using MQTTGatewayClient.Configuration;
using MQTTGatewayClient.Services;

var builder = Host.CreateApplicationBuilder(args);

const string optionsPath = "/data/options.json";

if(!File.Exists(optionsPath))
{
    throw new InvalidOperationException($"Home Assistant configuration file not found {optionsPath}");
}
var optionsJson =
    await File.ReadAllTextAsync(optionsPath);


var options =
    JsonSerializer.Deserialize<GatewayOptions>(optionsJson, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    })
    ?? throw new InvalidOperationException(
        "Unable to read Home Assistant add-on configuration.");

Console.WriteLine($"MQTT Host: {options.MqttHost}");
Console.WriteLine($"MQTT Port: {options.MqttPort}");
Console.WriteLine($"MQTT Username configured: {!string.IsNullOrWhiteSpace(options.MqttUsername)}");
Console.WriteLine($"MQTT Routes: {options.Routes.Count}");

if (string.IsNullOrWhiteSpace(options.MqttHost))
{
    throw new InvalidOperationException(
        "MQTT host is not configured.");
}

if (options.MqttPort <= 0)
{
    throw new InvalidOperationException(
        "MQTT port is not configured.");
}

if (string.IsNullOrWhiteSpace(options.MqttUsername))
{
    throw new InvalidOperationException(
        "MQTT username is not configured.");
}

if (string.IsNullOrWhiteSpace(options.MqttPassword))
{
    throw new InvalidOperationException(
        "MQTT password is not configured.");
}

if (options.Routes == null || options.Routes.Count == 0)
{
    throw new InvalidOperationException("No MQTT routes are configured");
}


foreach (var route in options.Routes)
{
    if (string.IsNullOrWhiteSpace(route.MqttTopicFilter))
    {
        throw new InvalidOperationException(
            "MQTT topic filter is not configured.");
    }

    if (string.IsNullOrWhiteSpace(route.AzureConnectionString))
    {
        throw new InvalidOperationException(
            $"Azure connection string is not configured for MQTT topic '{route.MqttTopicFilter}'.");
    }
}

builder.Services.AddSingleton(options);

builder.Services.AddSingleton<ConnectionService>();

builder.Services.AddHostedService<MqttService>();

var host = builder.Build();

await host.RunAsync();