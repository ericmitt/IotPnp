# Tutorial: How to convert a PnP Device to a PnP Module (C#)

---

title: Convert an IoT Plug and Play device to a Generic Module | Microsoft Docs
description: Use C# PnP device code and convert it to a module.
author: ericmitt
ms.author: ericmitt
ms.date: 9/17/2020
ms.topic: quickstart
ms.service: iot-pnp
services: iot-pnp
ms.custom: mvc
---

In this article we are going to migrate an Azure IoT PnP Device to an Azure IoT PnP Module.

As described in the PnP developer guide, a device become a PnP device once it publishes its DTDL ModelId and implements the properties and methods described in this DTDL Model. The same apply for modules.

We are going to transform the **Thermostat** C# device sample into a generic module.

## Prerequisites

[!INCLUDE [iot-pnp-prerequisites](../../includes/iot-pnp-prerequisites.md)]

## getting started

- Clone the CSharp SDK Repo

```cmd
git clone https://github.com/Azure/azure-iot-sdk-csharp.git
```

- Navigate to the pnp sample:  <yourclone>\azure-iot-sdk-csharp\iothub\device\samples\PnpDeviceSamples\Thermostat

- Open the  **Thermostat** project in Visual Studio.

## Change the code to communicate the modelId at the connection time

Open **Program.cs**:

  1. Change the deviceclient type to a ModuleClient type (Ideally the variable 's_deviceClient' should also be renamed for clarity but we'll leave this outside of this sample to keep changes to a minimum):

  ```csharp
  private static ModuleClient s_deviceClient;
  ```

  2. In the **InitializeDeviceClientAsync** method change the connection instruction to include the modelId as option to the connection:

  ```csharp
  //s_deviceClient = DeviceClient.CreateFromConnectionString(s_deviceConnectionString, TransportType.Mqtt, options);

  s_deviceClient = ModuleClient.CreateFromConnectionString(s_deviceConnectionString, TransportType.Mqtt, options);
  ```

Voila, your PnP module code is ready! We need now to configure the environment to run the sample.

## Running the PnP Module

Go in IoTExplorer, open the Hub and Device you want your module be hosted by. You can add a module to any device you created before (Of course on real devices you need to have enough resources, like memory to be able to create module)

Create a module identity for the device, name it, select the add hoc security settings. (Symetric key work well for this sample)

Open the module just created, copy the Primary connection string. Create the env variable **IOTHUB_MODULE_CONN_STRING** with the module connection string just copied.

Look at the Module Twin tab, who display the Json for the twin. Note the absence of modelId.

Switch back to Visual Studio, and run the project.

Look in IoT Explorer for:

- The module twin json now contains the modelId declared
- Telemetry passing at the device level (not at the pnp component level)
- Module twin property updates triggering PnP notifications

You can also send commands to your module.

## Interacting with a device module, from your solution

With the Service SDK, you can retrieve the modelId of a PnP device. It is the same for a module.
For example you can run the sample created for Service SDK [thermostat](link)...


## Make this a PnP IoT Edge Module

To make this a PnP IoT Edge module, we only need to containerize this application. The code does not need to be changed. The connection string environment variable will be injected by the IoT Edge runtime on startup.

To containerize your module, the easiest is to start from an empty C# IoT Edge module template by following [this tutorial](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-visual-studio-develop-module) and update the `Program.cs` code to the one created above.

Once your module has been containerized, deploy it on an IoT Edge device by following [this tutorial to simulate an IoT Edge device in Azure](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-ubuntuvm) or [this tutorial if you have a physical device](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux).

You can now again look in IoT Explorer to see:
- The module twin json of your IoT Edge device now contains the modelId declared
- Telemetry passing at the device level
- IoT Edge module twin property updates triggering PnP notifications
- The IoT Edge module reacting to your PnP commands

