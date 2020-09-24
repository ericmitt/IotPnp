---
title: Interact with an IoT Plug and Play device connected to your Azure IoT solution (Python) | Microsoft Docs
description: Use Python to connect to and interact with an IoT Plug and Play device that's connected to your Azure IoT solution.
author: elhorton
ms.author: elhorton
ms.date: 9/23/2020
ms.topic: quickstart
ms.service: iot-pnp
services: iot-pnp
ms.custom: mvc

# As a solution builder, I want to connect to and interact with an IoT Plug and Play device that's connected to my solution. For example, to collect telemetry from the device or to control the behavior of the device.
---

# Quickstart: Interact with an IoT Plug and Play device that's connected to your solution (Python)

[!INCLUDE [iot-pnp-quickstarts-service-selector.md](../../includes/iot-pnp-quickstarts-service-selector.md)]

IoT Plug and Play simplifies IoT by enabling you to interact with a device's model without knowledge of the underlying device implementation. This quickstart shows you how to use Python to connect to and control an IoT Plug and Play device that's connected to your solution.

## Prerequisites

[!INCLUDE [iot-pnp-prerequisites](../../includes/iot-pnp-prerequisites.md)]

To complete this quickstart, you need Python 3.7 on your development machine. You can download the latest recommended version for multiple platforms from [python.org](https://www.python.org/). You can check your Python version with the following command:  

```cmd/sh
python --version
```

The **azure-iot-device** package is published as a PIP.

In your local python environment install the package as follows:

```cmd/sh
pip install azure-iot-device
```

Install the **azure-iot-hub** package by running the following command:

```cmd/sh
pip install azure-iot-hub
```

## Run the sample device

[!INCLUDE [iot-pnp-environment](../../includes/iot-pnp-environment.md)]

To learn more about the sample configuration, see the [sample readme](https://github.com/Azure/azure-iot-sdk-python/blob/master/azure-iot-device/samples/pnp/README.md).

In this quickstart, you use a sample thermostat device, written in Python, as the IoT Plug and Play device. To run the sample device:

1. Open a terminal window in a folder of your choice. Run the following command to clone the [Azure IoT Python SDK](https://github.com/Azure/azure-iot-sdk-python) GitHub repository into this location:

    ```cmd/sh
    git clone https://github.com/Azure/azure-iot-sdk-python
    ```

1. This terminal window is used as your **device** terminal. Go to the folder of your cloned repository, and navigate to the */azure-iot-sdk-python/azure-iot-device/samples/pnp* folder.

1. Run the sample thermostat device with the following command:

    ```cmd/sh
    python simple_thermostat.py
    ```

1. You see messages saying that the device has sent some information and reported itself online. These messages indicate that the device has begun sending telemetry data to the hub, and is now ready to receive commands and property updates. Don't close this terminal, you need it to confirm the service sample is working.

## Run the sample solution

In this quickstart, you use a sample IoT solution in Python to interact with the sample device you just set up.

1. Open another terminal window to use as your **service** terminal. 

1. Navigate to the */azure-iot-sdk-python/azure-iot-hub/samples* folder of the cloned Python SDK repository.

1. In the samples folder, there are several samples showing how to interact with your devices. This quickstart will focus on using the IoTHubRegistryManager client to interact with IoT Plug and Play devices. It will specifically walk through *pnp_iothub_registry_manager_sample.py* as an example for using the IoTHubRegistryManager with an Azure IoT Plug and Play "thermostat" device. 

1. Configure your environment variables:

* **IOTHUB_CONNECTION_STRING**: the IoT hub connection string you made a note of previously.
* **IOTHUB_DEVICE_ID**: `"my-pnp-device"`
* **IOTHUB_METHOD_NAME**: `"getMaxMinReport"`
* **IOTHUB_METHOD_PAYLOAD**: `"hello world"`

```cmd/sh
set IOTHUB_CONNECTION_STRING=<your IoT Hub connection string>
set IOTHUB_DEVICE_ID=my-pnp-device
set IOTHUB_COMMAND_NAME="getMaxMinReport" # this is the relevant command for the thermostat sample
set IOTHUB_COMMAND_PAYLOAD="hello world" # this payload doesn't matter for this sample
```

Now you can use the following command in the **service** terminal to run this sample:

```cmd/sh
python pnp_iothub_registry_manager_sample.py
```

### Get device twin

The output shows the device's twin and prints its model ID:

```cmd/sh
The Model ID for this device is:
dtmi:com:example:Thermostat;1
```

The following snippet shows the sample code from *iothub_registry_manager_sample.py*:

```python
    # Create IoTHubRegistryManager
    iothub_registry_manager = IoTHubRegistryManager(iothub_connection_str)

    # Get device twin
    twin = iothub_registry_manager.get_twin(device_id)
    print("The device twin is: ")
    print("")
    print(twin)
    print("")
    
    # Print the device's model ID
    additional_props = twin.additional_properties
    if "modelId" in additional_props:
        print("The Model ID for this device is:")
        print(additional_props["modelId"])
        print("")
```

### Update a device twin

Within the *iothub_registry_manager_sample.py* sample, you will also see how a device twin is updated. The following code snippet shows you how to use a *patch* to update properties through your device's twin:

```python
    # Update twin
    twin_patch = Twin()
    twin_patch.properties = TwinProperties(desired={"targetTemperature": 42}) # this is relevant for the thermostat device example
    updated_twin = iothub_registry_manager.update_twin(device_id, twin_patch, twin.etag)
    print(updated_twin)
    print("The twin patch has been successfully applied")
```

You can verify that the update is applied in the **device** terminal that shows the following output:

```cmd/sh
the data in the desired properties patch was: {'targetTemperature': 42, '$version': 2}
```

The **service** terminal confirms that the patch was successful:

```cmd/sh
The twin patch has been successfully applied
```

### Invoke a method

Further down the sample, you will see the code to invoke a method on the device. Above, you set environment variables to define the relevant `IOTHUB_METHOD_NAME` and `IOTHUB_METHOD_PAYLOAD` for the simple thermostat device:

The **service** terminal shows a confirmation message when the method is invoked:

```cmd/sh
The device method has been successfully invoked
```

In the **device** terminal, you see the device receives the command:

```cmd/sh
Command request received with payload
hello world
Will return the max, min and average temperature from the specified time hello world to the current time
Done generating
{"avgTemp": 31.9, "endTime": "2020-09-23T23:53:45.029092", "maxTemp": 49, "minTemp": 10, "startTime": "2020-09-23T23:52:25.029092"}
Sent message
```

## Next steps

In this quickstart, you learned how to connect an IoT Plug and Play device to a IoT solution. To learn more about IoT Plug and Play device models, see:

> [!div class="nextstepaction"]
> [IoT Plug and Play modeling developer guide](concepts-developer-guide.md)
