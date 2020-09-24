---
title: Convert an IoT Plug and Play device to a Generic Module | Microsoft Docs
description: Use C# PnP device code and convert it to a module.
author: ericmitt
ms.author: ericmitt
ms.date: 9/17/2020
ms.topic: tutorial
ms.service: iot-pnp
services: iot-pnp
---
# Tutorial: How to convert a PnP Device to a PnP Module (C#)

In this article we are going to migrate an Azure IoT PnP Device to an Azure IoT PnP Module.

As described in the PnP developer guide, a device become a PnP device once it publishes its DTDL ModelId and implements the properties and methods described in this DTDL Model. The same apply for modules.

We are going to transform the **Thermostat** C# device sample into a generic module.

## Prerequisites

[!INCLUDE [iot-pnp-prerequisites](../../includes/iot-pnp-prerequisites.md)]

## getting started

- Clone the CSharp SDK Repo

```cmd
git clone https://github.com/Azure/azure-iot-samples-csharp.git
```

- Navigate to the pnp sample:  <yourclone>\azure-iot-samples-csharp\iot-hub\Samples\service\PnpServiceSamples\Thermostat

- Open the  **Thermostat** project in Visual Studio.

## Change the code to communicate the modelId at the connection time

make the sample running without DPS, just with the connection string:

```bash
IOTHUB_DEVICE_SECURITY_TYPE = "connectionString"
```

Open **Parameter.cs**:

- Change the env variable to be used for the module:

```csharp
public string PrimaryConnectionString { get; set; } = Environment.GetEnvironmentVariable("IOTHUB_MODULE_CONN_STRING");
```
Open **Program.cs**

- change every occurrence of **DeviceClient** by **ModuleClient** in ThermostatSample.cs 
- change every occurrence of **DeviceClient** by **ModuleClient** in Program.cs **except this one** (line 102 in **ProvisionDeviceAsync** method):

 ```csharp
var pdc = ProvisioningDeviceClient.Create....
``` 
>Tip, use the VS2019 replace all feature for the current project, then try to build, the error will point you to **ProvisionDeviceAsync** method.

Ideally we should have renamed these variable, but let this sample short as possible and use the replace function in Visual Studio.

Note now:

- in ThermostatSample.cs, that our variable is now a ModuleClient:

```csharp
private readonly ModuleClient _deviceClient;
```

- in the program.cs the **SetupModuleClientAsync** method change the connection instruction to include the modelId as option to the connection:

```csharp
case "connectionstring":
    s_logger.LogDebug($"Initializing via IoT Hub connection string");
    deviceClient = InitializeModuleClient(parameters.PrimaryConnectionString);
    break;
```

Voila, your PnP module code is ready! We need now to configure the environment to run the sample now.

## Running the PnP Module

Go in IoTExplorer, open the Hub and Device you want your module be hosted by. You can add a module to any device you created before (Of course on real devices you need to have enough resources, like memory to be able to create module)

Create a module identity for the device, name it, select the add hoc security settings. (Symetric key works well for this sample)

Open the module just created, copy the Primary connection string and create the env variable **IOTHUB_MODULE_CONN_STRING** with it.

Look at the Module Twin tab, who display the Json for the twin. Note the absence of modelId.

Switch back to Visual Studio, and run the project.

Look in IoT Explorer for:

- The module twin json now contains the modelId declared
- Telemetry passing at the device level (not at the pnp component level)
- Module twin property updates triggering PnP notifications

## Interacting with a device module, from your solution

With the Service SDK, you can retrieve the modelId of a PnP device. It is the same for a PnP module.
For example you can run the sample created for Service SDK [thermostat](quickstart-service-csharp.md)...

Open the <yourclone>\azure-iot-samples-csharp\iot-hub\Samples\service\PnpServiceSamples\Thermostat

Open the **Program.cs**

Change the invoke method adding the moduleId as second parameter:

```csharp
CloudToDeviceMethodResult result = await s_serviceClient.InvokeDeviceMethodAsync(s_deviceId,"<ModuleName>" ,commandInvocation);
```

Let the device code running and sending telemetry, run this modified service sample from Visual Studio.
Note that the command in called in the device console:

```bash
[09/24/2020 13:08:13]dbug: Thermostat.ThermostatSample[0]
      Property: Update - { "maxTempSinceLastReboot": 33.9°C } is Completed.
[09/24/2020 13:08:18]dbug: Thermostat.ThermostatSample[0]
      Telemetry: Sent - { "temperature": 33.9°C }.
[09/24/2020 13:08:23]dbug: Thermostat.ThermostatSample[0]
      Telemetry: Sent - { "temperature": 33.9°C }.
[09/24/2020 13:08:24]dbug: Thermostat.ThermostatSample[0]
      Command: Received - Generating max, min and avg temperature report since 9/24/2020 1:08:22 PM.
[09/24/2020 13:08:24]dbug: Thermostat.ThermostatSample[0]
      Command: MaxMinReport since 9/24/2020 1:08:22 PM: maxTemp=33.9, minTemp=33.9, avgTemp=33.9, startTime=9/24/2020 1:08:23 PM, endTime=9/24/2020 1:08:23 PM
[09/24/2020 13:08:28]dbug: Thermostat.ThermostatSample[0]
      Telemetry: Sent - { "temperature": 33.9°C }.
[09/24/2020 13:08:33]dbug: Thermostat.ThermostatSample[0]
      Telemetry: Sent - { "temperature": 33.9°C }.
```

In the service console the return code is OK: 

```bash
[09/24/2020 13:08:23]dbug: Thermostat.Program[0]
      Initialize the service client.
[09/24/2020 13:08:23]dbug: Thermostat.Program[0]
      Get Twin model Id and Update Twin
[09/24/2020 13:08:24]dbug: Thermostat.Program[0]
      Model Id of this Twin is:
[09/24/2020 13:08:24]dbug: Thermostat.Program[0]
      Invoke a command
[09/24/2020 13:08:24]dbug: Thermostat.Program[0]
      Command getMaxMinReport invocation result status is: 200
```

## Make this a PnP IoT Edge Module

To make this a PnP IoT Edge module, we only need to containerize this application. The code does not need to be changed. The connection string environment variable will be injected by the IoT Edge runtime on startup.

To containerize your module, the easiest is to start from an empty C# IoT Edge module template by following [this tutorial](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-module) and update the `Program.cs` code to the one created above.

Once your module has been containerized, deploy it on an IoT Edge device by following [this tutorial to simulate an IoT Edge device in Azure](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-ubuntuvm) or [this tutorial if you have a physical device](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux).

You can now again look in IoT Explorer to see:

- The module twin json of your IoT Edge device now contains the modelId declared
- Telemetry passing at the device level
- IoT Edge module twin property updates triggering PnP notifications
- The IoT Edge module reacting to your PnP commands

## Make the multiple component sample a PnP Module

Follow the exact same edits for the client code, change any DeviceClient by ModuleClient except for **ProvisioningDeviceClient** .

Add the env variable **IOTHUB_MODULE_CONN_STRING**, with a valid module connection string.

For the service sample, you need as previous add the env variable and change not one occurrence but multiple occurrence of **InvokeDeviceMethodAsync** corresponding to the different components, in this sample with have 3 components, the default, Thermostat1 and Thermostat2.

```csharp
CloudToDeviceMethodResult result = await s_serviceClient.InvokeDeviceMethodAsync(s_deviceId,"<ModuleName>" ,commandInvocation);
```

As the device client is still running, run this service sample, you'll should see:

```bash
[09/24/2020 13:30:14]dbug: TemperatureController.TemperatureControllerSample[0]
      Telemetry: Sent - component="thermostat1", { "temperature": 18.7°C }.
[09/24/2020 13:30:14]dbug: TemperatureController.TemperatureControllerSample[0]
      Telemetry: Sent - component="thermostat2", { "temperature": 44.9°C }.
[09/24/2020 13:30:17]dbug: TemperatureController.TemperatureControllerSample[0]
      Command: Received - component="thermostat1", generating max, min and avg temperature report since 9/24/2020 1:30:13 PM.
[09/24/2020 13:30:17]dbug: TemperatureController.TemperatureControllerSample[0]
      Command: component="thermostat1", MaxMinReport since 9/24/2020 1:30:13 PM: maxTemp=18.7, minTemp=18.7, avgTemp=18.7, startTime=9/24/2020 1:30:14 PM, endTime=9/24/2020 1:30:14 PM
[09/24/2020 13:30:17]dbug: TemperatureController.TemperatureControllerSample[0]
      Command: Received - Rebooting thermostat (resetting temperature reading to 0°C after 3 seconds).
[09/24/2020 13:30:19]dbug: TemperatureController.TemperatureControllerSample[0]
      Telemetry: Sent - component="thermostat1", { "temperature": 18.7°C }.
[09/24/2020 13:30:19]dbug: TemperatureController.TemperatureControllerSample[0]
      Telemetry: Sent - component="thermostat2", { "temperature": 44.9°C }.
```

And on Service side sample you'll see:

```bash
[09/24/2020 13:30:13]dbug: TemperatureController.Program[0]
      Initialize the service client.
[09/24/2020 13:30:14]dbug: TemperatureController.Program[0]
      Get model Id and Update Component Property
[09/24/2020 13:30:16]dbug: TemperatureController.Program[0]
      Model Id of this Twin is:
[09/24/2020 13:30:16]dbug: TemperatureController.Program[0]
      Invoke a command on a Component
[09/24/2020 13:30:17]dbug: TemperatureController.Program[0]
      Command getMaxMinReport invocation result status is: 200
[09/24/2020 13:30:17]dbug: TemperatureController.Program[0]
      Invoke a command on root interface
[09/24/2020 13:30:20]dbug: TemperatureController.Program[0]
      Command getMaxMinReport invocation result status is: 200
``` 