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

namespace SenseThermostat
{
    class Program
    {
        private static string deviceConnectionString = "";
        private static string ModelId = "dtmi:com:example:Thermostat;1";

        private static DeviceClient deviceClient = null;
        private static double pt_temperature = 0d;
        private static double min_temperature = 0d;
        private static double max_temperature = 0d;
        private static double sumTemperature = 0d;
        private static long numbermeasure = 0l;
        private static DateTime startTime = DateTime.Now;
        static void Main(string[] args)
        {
            ConnectToHub();

            while (true)
            {
                SenseHatPressureAndTemperature pt = new();
                pt_temperature = pt.Temperature.DegreesCelsius;
                Console.WriteLine("Temp = " + pt_temperature);
                SendTelemetry(pt_temperature);

                if (pt_temperature > max_temperature)
                {
                    max_temperature = pt_temperature;

                    UpdateReadOnlyProperties();
                }
                if (pt_temperature < min_temperature) min_temperature = pt_temperature;

                sumTemperature += pt_temperature;
                numbermeasure++;

                Thread.Sleep(3000);
            }
        }

        private static async Task UpdateReadOnlyProperties()
        {
            const string propertyName = "maxTempSinceLastReboot";
            var reportedProperties = new TwinCollection();
            reportedProperties[propertyName] = max_temperature;
            await deviceClient.UpdateReportedPropertiesAsync(reportedProperties);
        }

        private async static void SendTelemetry(double temperature)
        {
            string telemetryName = "temperature";
            string telemetryPayload = $"{{ \"{telemetryName}\": {pt_temperature} }}";
            using var message = new Message(Encoding.UTF8.GetBytes(telemetryPayload))
            {
                ContentEncoding = "utf-8",
                ContentType = "application/json",
            };

            await deviceClient.SendEventAsync(message);
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

            await deviceClient.SetMethodHandlerAsync("getMaxMinReport", HandleMaxMinReportCommand, deviceClient, cancellationToke);


        }

        private static Task<MethodResponse> HandleMaxMinReportCommand(MethodRequest request, object userContext)
        {
            Console.WriteLine("MaxMinReport Command received");

            var report = new
            {
                maxTemp = max_temperature,
                minTemp = min_temperature,
                avgTemp = sumTemperature / numbermeasure,
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
