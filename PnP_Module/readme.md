# PnP Module tutorial

This Module sample is based on Thermostat sample.
This sample will be merged after Bugbash into the sample repo.

## Create the module in IoTExplorer

Go in IoTExplorer, open the Hub and Device you want your module be hosted by. You can add a module to any device you created before (Of course on real devices you need to have enough resources, like memory to be able to create module)

Create a module identity for the device, name it, select the add hoc security settings. (Symetric key work well for this sample)

Open the module just created, copy the Primary connection string. 

Create the env variable **IOTHUB_MODULE_CONN_STRING** with the module connection string just copied.

Look at the Module Twin tab, who display the Json for the twin. Note the absence of modelId.

## Configure your environment

Clone this repo: 

```bash
https://github.com/ericmitt/IotPnp.git 

```

Create the following env variable:
IOTHUB_CONNECTION_STRING
DEVICE_ID

## Run the PnP Module sample

Open the ThermostatModule solution in Visual Studio.

Make the **ThermostatClient** project the startup project.
Run the ThermostatClient project (ctrl+F5), and let the sample running. It push telemetry message at regular interval.

Switch to IoTEXplorer a look at the json of the module, do you see the update? the modelId?

Make the **ThermostatService** project the startup project.
Run the ThermostatService project (ctrl+F5, or debug it F5)
You should see trace of method invocation:

```bash
09/23/2020 21:17:34]dbug: Thermostat.Program[0]
      Initialize the service client.
[09/23/2020 21:17:34]dbug: Thermostat.Program[0]
      Get Twin model Id and Update Twin
[09/23/2020 21:17:35]dbug: Thermostat.Program[0]
      Model Id of this Twin is:
[09/23/2020 21:17:35]dbug: Thermostat.Program[0]
      Invoke a command
[09/23/2020 21:17:35]dbug: Thermostat.Program[0]
      Command getMaxMinReport invocation result status is: 404
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