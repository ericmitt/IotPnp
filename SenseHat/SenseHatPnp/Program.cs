using System;
using System.Threading;
using Iot.Device.Common;
using Iot.Device.SenseHat;
using UnitsNet;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Azure.Devices.Client;
using TransportType = Microsoft.Azure.Devices.Client.TransportType;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using PnpHelpers;
using System.Runtime.InteropServices;
using System.Reflection;

namespace SenseHatPnP
{
    class Program
    {
        private static string deviceConnectionString = "";
        private static string ModelId = "dtmi:com:example:SenseHat;1";
        public const string ContentApplicationJson = "application/json";

        private static DeviceClient deviceClient = null;
        private static double pt_temperature = 0d;
        private static double pt_humidity = 0d;
        private static double min_temperature = 0d;
        private static double max_temperature = 0d;
        private static double sumTemperature = 0d;
        private static long numbermeasure = 0L;
        private static DateTime startTime = DateTime.Now;
        private static double min_humidity = 0d;
        private static double max_humidity = 0d;
        private static double sumhumidity = 0d;
        private static float induc = 0F;
        private static double acceleration = 0D;
        private static double angularrate = 0D;
        private static double workingsetsize = 0D;

        static void Main(string[] args)
        {
            ConnectToHub();

            SenseHatLedMatrixSysFs matrix = new();

            UpdateProperty("serialNumber", "1234-56-789");

            var d = new Dictionary<string, object>
                {
                    { "manufacturer", "Raspberry" },
                    { "model", Environment.Version.Major },
                    { "swVersion", "1.0.0" },
                    { "osName",   "Raspbian" },
                    { "processorArchitecture", "ARM"},
                    { "processorManufacturer", "" },
                    { "totalStorage", 256 },
                    { "totalMemory", 1024 },
                };
            PatchProperty("deviceInformation", d);

            //Send Telemetry
            while (true)
            {
                numbermeasure++;

                //workingSet

                workingsetsize = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024;
                SendTelemetry(workingsetsize, "workingSet", "DEFAULT_COMPONENT");

                // Temperature
                SenseHatPressureAndTemperature pt = new();
                pt_temperature = pt.Temperature.DegreesCelsius;
                Console.WriteLine("Temp = " + pt_temperature);
                SendTelemetry(pt_temperature, "temperature", "thermostat1");

                //Humidity
                SenseHatTemperatureAndHumidity ph = new();
                pt_humidity = ph.Humidity.Percent;
                Console.WriteLine("Humidity = " + pt_humidity);
                SendTelemetry(pt_humidity, "humidity", "humidity1");

                if (pt_temperature > max_temperature)
                {
                    max_temperature = pt_temperature;

                    UpdateReadOnlyProperties("maxTempSinceLastReboot", "thermostat1");
                }
                if (pt_temperature < min_temperature) min_temperature = pt_temperature;

                sumTemperature += pt_temperature;

                if (pt_humidity > max_humidity)
                {
                    max_humidity = pt_humidity;

                    UpdateReadOnlyProperties("maxHumiditySinceLastReboot", "humidity1");
                }
                if (pt_humidity < min_humidity) min_humidity = pt_humidity;

                sumhumidity += pt_humidity;

                //Magnetometer
                SenseHatMagnetometer mg = new();
                induc = mg.MagneticInduction.Length();
                Console.WriteLine("Induction " + induc);
                SendTelemetry(induc, "inductance", "magneto1");

                var XYZ = new
                {
                    X = mg.MagneticInduction.X,
                    Y = mg.MagneticInduction.Y,
                    Z = mg.MagneticInduction.Z,
                };

                SendTelemetry2(XYZ, "inductancexyz", "magneto1");

                //accelerometer
                SenseHatAccelerometerAndGyroscope ag = new();
                acceleration = ag.Acceleration.Length();
                SendTelemetry(acceleration, "acceleration", "accelerometer1");
                Console.WriteLine("Acceleration " + acceleration);
                var XYZacc = new
                {
                    X = ag.Acceleration.X,
                    Y = ag.Acceleration.Y,
                    Z = ag.Acceleration.Z,
                };
                SendTelemetry2(XYZacc, "accelerationxyz", "accelerometer1");


                // Gyroscope
                angularrate = ag.AngularRate.Length();
                SendTelemetry(angularrate, "angularrate", "gyroscope1");
                Console.WriteLine("AngularRate " + angularrate);
                var XYZgyro = new
                {
                    X = ag.AngularRate.X,
                    Y = ag.AngularRate.Y,
                    Z = ag.AngularRate.Z,
                };
                SendTelemetry2(XYZgyro, "angularratexyz", "gyroscope1");

                //LED animation
                AnimateLedMatrix(matrix);

                Thread.Sleep(3000);
            }
        }

        private static async Task UpdateProperty(string propertyName, string v)
        {
            TwinCollection reportedProperties = PnpConvention.CreatePropertyPatch(propertyName, v);
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }

        private static async Task UpdateProperty(string componentName, string propertyName, string v)
        {
            TwinCollection reportedProperties = PnpConvention.CreateComponentPropertyPatch(componentName,propertyName, v);
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

        }

        private static async Task PatchProperty (string componentName, Dictionary<string, object> dictio)
        {
            TwinCollection deviceInfoTc = PnpConvention.CreateComponentPropertyPatch( componentName, dictio);

            await deviceClient.UpdateReportedPropertiesAsync(deviceInfoTc);
        }

        private static void AnimateLedMatrix(SenseHatLedMatrixSysFs matrix)
        {
           
            if (pt_temperature >= 27.5D)
            {
                matrix.Fill(System.Drawing.Color.OrangeRed);
            }
            else
                matrix.Fill(System.Drawing.Color.Yellow);
            if (pt_temperature <= 26.5D)
                matrix.Fill(System.Drawing.Color.Green);

                        
            for (int i = 0; i < 8; i++)
            {
                matrix.SetPixel(i, 0, System.Drawing.Color.Aquamarine);
                matrix.SetPixel(0, i, System.Drawing.Color.Aquamarine);
                matrix.SetPixel(i, 7, System.Drawing.Color.Aquamarine);
                matrix.SetPixel(7, i, System.Drawing.Color.Aquamarine);
                matrix.SetPixel(i, i, System.Drawing.Color.Aquamarine);
            }
        }

        private static async void SendTelemetry2(object v1, string telemetryName, string componentName)
        {
            var message =  PnpConvention.CreateMessage(telemetryName, v1, componentName);
            await deviceClient.SendEventAsync(message);
        }

        private static async Task UpdateReadOnlyProperties(string propertyName, string componentName)
        {
            if (propertyName == "maxTempSinceLastReboot")
            {
                //var reportedProperties = new TwinCollection();
                //reportedProperties[propertyName] = max_temperature;
                //await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

                TwinCollection reportedProperties = PnpConvention.CreateComponentPropertyPatch(componentName, propertyName, max_temperature);
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);

            }
            if (propertyName == "maxHumiditySinceLastReboot")
            {
                var reportedProperties = new TwinCollection();
                reportedProperties[propertyName] = max_humidity;
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
            }

        }
        
        private async static void SendTelemetry(double temperature, string telemetryName, string componentName)
        {
            Message message;
            if (telemetryName == "temperature")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_temperature} }}";
                message = PnpConvention.CreateMessage(telemetryName, pt_temperature, componentName);
                await deviceClient.SendEventAsync(message);
            }
            if (telemetryName == "humidity")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_humidity} }}";
                message = PnpConvention.CreateMessage(telemetryName, pt_humidity, componentName);
                await deviceClient.SendEventAsync(message);
            }

            if (telemetryName == "inductance")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_humidity} }}";
                message = PnpConvention.CreateMessage(telemetryName, induc, componentName);
                await deviceClient.SendEventAsync(message);
            }

            if (telemetryName == "acceleration")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_humidity} }}";
                message = PnpConvention.CreateMessage(telemetryName, acceleration, componentName);
                await deviceClient.SendEventAsync(message);
            }

            if (telemetryName == "angularrate")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_humidity} }}";
                message = PnpConvention.CreateMessage(telemetryName, angularrate, componentName);
                await deviceClient.SendEventAsync(message);
            }

            if (telemetryName == "workingSet")
            {
                //telemetryPayload = $"{{ \"{telemetryName}\": {pt_humidity} }}";
                message = PnpConvention.CreateMessage(telemetryName, workingsetsize);
                await deviceClient.SendEventAsync(message);
            }

        }

        private async static void ConnectToHub()
        {
            var options = new ClientOptions
            {
                ModelId = ModelId,
            };

            deviceClient = DeviceClient.CreateFromConnectionString(deviceConnectionString, TransportType.Mqtt, options);
            deviceClient.SetConnectionStatusChangesHandler((status, reason) =>
            {
                Console.WriteLine($"Connection status change registered - status={status}, reason={reason}.");
            });

            CancellationToken cancellationToke;
            await deviceClient.SetDesiredPropertyUpdateCallbackAsync(TargetTemperatureUpdateCallbackAsync, deviceClient, cancellationToke);

            await deviceClient.SetMethodHandlerAsync("thermostat1*getMaxMinReport", Thermostat1HandleMaxMinReportCommand, deviceClient, cancellationToke);
            await deviceClient.SetMethodHandlerAsync("humidity1*getMaxMinReport", Hunidity1HandleMaxMinReportCommand, deviceClient, cancellationToke);


        }
              
        private static Task<MethodResponse> Thermostat1HandleMaxMinReportCommand(MethodRequest request, object userContext)
        {
            Console.WriteLine("MaxMinReport Command received");

            var report = new
            {
                maxTemp = max_temperature,
                minTemp = min_temperature,
                avgTemp = sumTemperature/numbermeasure,
                startTime = startTime,
                endTime = DateTime.Now,
            };


            byte[] responsePayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(report));
            return Task.FromResult(new MethodResponse(responsePayload, (int)StatusCode.Completed));
        }

        private static Task<MethodResponse> Hunidity1HandleMaxMinReportCommand(MethodRequest request, object userContext)
        {
            Console.WriteLine("MaxMinReport Command received");

            var report = new
            {
                maxHumidity = max_humidity,
                minHumidity = min_humidity,
                avgHumidity = sumhumidity / numbermeasure,
                startTime = startTime,
                endTime = DateTime.Now,
            };


            byte[] responsePayload = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(report));
            return Task.FromResult(new MethodResponse(responsePayload, (int)StatusCode.Completed));
        }


        internal enum StatusCode
        {
            Completed = 200,
            InProgress = 202,
            NotFound = 404,
            BadRequest = 400
        }
        private static async Task TargetTemperatureUpdateCallbackAsync(TwinCollection desiredProperties, object userContext)
        {
            const string propertyName = "targetTemperature";

            if (true)
            {
                Console.WriteLine($"Property: Received - {{ \"{propertyName}\": {desiredProperties[propertyName]}°C }}.");

                string jsonPropertyPending = $"{{ \"{propertyName}\": {{ \"value\": {pt_temperature}, \"ac\": {(int)StatusCode.InProgress}, " +
                    $"\"av\": {desiredProperties.Version} }} }}";
                var reportedPropertyPending = new TwinCollection(jsonPropertyPending);
                await deviceClient.UpdateReportedPropertiesAsync(reportedPropertyPending);
                Console.WriteLine($"Property: Update - {{\"{propertyName}\": {desiredProperties[propertyName]}°C }} is {StatusCode.InProgress}.");

               

                string jsonProperty = $"{{ \"{propertyName}\": {{ \"value\": {pt_temperature}, \"ac\": {(int)StatusCode.Completed}, " +
                    $"\"av\": {desiredProperties.Version}, \"ad\": \"Successfully updated target temperature\" }} }}";
                var reportedProperty = new TwinCollection(jsonProperty);
                await deviceClient.UpdateReportedPropertiesAsync(reportedProperty);
                Console.WriteLine($"Property: Update - {{\"{propertyName}\": {pt_temperature}°C }} is {StatusCode.Completed}.");
            }
            else
            {
                Console.WriteLine($"Property: Received an unrecognized property update from service:\n{desiredProperties.ToJson()}");
            }
        }
    }
}
