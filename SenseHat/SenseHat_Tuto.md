# End to end experience with .NET, Azure IoT and Raspberry PI + SenseHat

In this experience, we are going to:

Implement in .net core 3.1, a PnP device who publish temperature to Azure IoT Hub from temperature sensor (SenseHat on RAspberry Pi 3)

## On the Device

### Setup and pre requisite

1. Setup System on your Raspberry Pi3, as usual following this steps : [link to raspberry pages] 
1. Plug your SenseHat on your Raspberry PI, an start it with screen, mouse and keyboard connected
1. Enable, in Raspberry Pi Configuration: SSH, I2C
1. create your dev/test environment (create a folder "SenseHat" taht will receive copy of your work)
1. note the IP address of your PI.

## On your Dev Machine (assuming it is Windows 10, but Linux or Mac welcome)

1. Install VSTudio (with .NET and .NET Core)
1. Install VSCode
1. Clone this tutorial repo: [link to this repo]
1. THe solution use 2 nuget package 
    1. Iot.Device.Bindings
    1. azure iot sdk  
1. Clone .net IOT Repo (optional, but you'll need their dll: Iot.DEvice.Bindings.dll)
1. Clone Azure IoT C# SDK (optional, but you'll need to install the  latest version of the device [SDK](https://www.nuget.org/packages/Microsoft.Azure.Devices/)
    ```dotnetcli
    Install-Package Microsoft.Azure.Devices -Version 1.30.0
    ```
## On the Azure portal
Prepare a Hub with a new device
(For example IoTHub42, SenseThermostat)

Note the following:
1. cnx string for Iot Hub
1. cnx string for the Device


Add and cnx Option for DPS (so we can use the device with IoT Central)


Show/explain the code
    .net iot usage
    design a pnp model
    azure iot sdk hub usage


## Run and Test
How to run the code produced?
How to debug this code?
