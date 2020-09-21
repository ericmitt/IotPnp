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

As described in the PnP developer guide, A device become a PnP device once ih publish its ModelId. (Of course the device should implement the properties and methods described  in this DTDL Model). The same apply for modules.

We are going to transform the **Thermostat** C# device sample into a generic module.

## Prerequisites

[!INCLUDE [iot-pnp-prerequisites](../../includes/iot-pnp-prerequisites.md)]

## getting started

Clone the CSharp SDK Repo

```cmd
git clone https://github.com/Azure/azure-iot-sdk-csharp.git
```

Navigate to the pnp sample:  <yourclone>\azure-iot-sdk-csharp\iothub\device\samples\PnpDeviceSamples\Thermostat

Open the  **Thermostat** project in Visual Studio.


## Change the code to communicate the modelId at the connection time

Open **Program.cs**:
1 change the deviceclient to ModuleClient as type (Ideally we should rename this variable, but let this sample short as possible):

```csharp
private static ModuleClient s_deviceClient;
```

1) in the **InitializeDeviceClientAsync** method change the connection instruction to include the modelId as option to the connection:

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

Look in Iot Explorer now for:

- Now the device twin json should contains the modelId we declared
- You can see telemetry passing at the device level (not at the pnp component level)

## Interacting with a device module, from your solution

With the Service SDK, you can retrieve the modelId of a PnP device. It is the same for a module.
For example you can run the sample created for Service SDK [thermostat](link)...


## Make this generic Module a PnP Edge Module
