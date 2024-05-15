using System;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices;
using IOT_Project_Device;


string conString = File.ReadAllText(@"ConnectionString.txt");
Console.WriteLine("Connection String loaded");

using var deviceClient = DeviceClient.CreateFromConnectionString(conString);
await deviceClient.OpenAsync();
var device = new MyDevice(deviceClient);
Console.WriteLine("Connection with Device Established");
await device.InitializerHandlers();
await device.UpdateTwinAsync();
Console.WriteLine("Ready to WORK =)");
await device.SendMessages(2, 1000);
var periodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(10));
while (await periodicTimer.WaitForNextTickAsync())
{
    await device.TimerSendingMessages();
}