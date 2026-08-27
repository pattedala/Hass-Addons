# MQTT Gateway Client

![Supports aarch64 Architecture](https://img.shields.io/badge/aarch64-yes-green.svg)
![Supports amd64 Architecture](https://img.shields.io/badge/amd64-yes-green.svg)

A Home Assistant add-on that acts as a bridge between MQTT and Azure IoT Hub.
The add-on listens for messages on a configured MQTT topic and forwards the received telemetry to an Azure IoT Hub device using the Azure IoT Hub device connection string.

## Mosquitto MQTT Broker required
If you don't have an MQTT broker yet; in Home Assistant go to Settings → Apps → App store and install the Mosquitto broker app, then start it.

## Data flow
```text
Home Assistant
      │
      │ MQTT
      ▼
Mosquitto
      │
      ▼
MQTT Gateway Client
      │
      │ 
      ▼
Azure IoT Hub
```

## Configuration

The following settings can be configured through the Home Assistant add-on UI:

MQTT Host – MQTT broker hostname
MQTT Port – MQTT broker port
MQTT Topic – Topic to subscribe to
MQTT Username / Password – MQTT authentication
IoT Hub Connection String – Azure IoT Hub device connection string

![MQTT configuration in the Home Assistant frontend][config]

Then hit 'Add' button to type topic to subscribe on and the connection string of yours,
you can add multiple topics/connections strings to listen for

![MQTT multiple configuration in the Home Assistant frontend][multiple]

The add-on runs as a background service and does not provide its own web interface.

[config]: https://raw.githubusercontent.com/pattedala/Hass-Addons/main/MQTTGatewayClient-addon/images/mqtt_config.png
[multiple]: https://raw.githubusercontent.com/pattedala/Hass-Addons/main/MQTTGatewayClient-addon/images/multi_connection.png
