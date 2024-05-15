using System;
using Microsoft.Azure.Devices;
string conString = File.ReadAllText("AzureConString.txt");
Console.WriteLine("============================================");
Console.WriteLine("[Controller] Azure Conntecting String Loaded");

using var serviceClient = ServiceClient.CreateFromConnectionString(conString);
using var registryManager = RegistryManager.CreateFromConnectionString(conString);

var manager = new IOT_Projekt.IotHubManager(serviceClient, registryManager);

int input;
do
{
    FeatureSelector.PrintMenu(); 
    input = FeatureSelector.ReadInput(); 
    await FeatureSelector.Execute(input, manager); 
} while (input != 0);

Console.WriteLine("[Controller] Controller End Working");
Console.WriteLine("============================================");