
# IOT Agent - Instrukcja

# 1. Uruhomienie aplikacji
  
  - Otworzyć plik IOT_Projekt.sln
  - Kliknąć "Rozpocznij"

# 2. Konfiguracja
 
 Edycja w sekcji device: 

 - Konfigurację połączenia z serwerem OPC UA edytujemy w pliku "OpcUaConnectionString.txt"
 - Nazwę urządzenia monitorowanego przez OPC Ua edytujemy w pliku "OpcUaDeviceName.txt"
 - Klucz połączenia do Azure IOT Hub edytujemy w pliku "AzureConString.txt"

 Edycja w sekcji kontrolera: 

 - Konfigurację połączenia z IOT Hub edytujemy w pliku "AzureConString.txt"

# 3. Działanie agenta 
Agent monitoruje jeden device w serwerze OPC UA. Dane dotyczące urządzenia są pobierane stale do cazsu wyłączenia agenta w odstępie czasowym  10 sekund.

# 4. D2C Messages
Wiadomości D2C są wysyłane do chmury Azure co 10 sekund. w formacie: 

```
{
  "body": {
    "ProductionStatus": 1,
    "WorkorderId": "32af9212-03f9-408d-b387-edb9d8971f6c",
    "Temperature": 25,
    "GoodCount": 0,
    "BadCount": 0,
    "ProductionRate": 0
  },
  "enqueuedTime": "Wed May 15 2024 16:59:55 GMT+0200 (czas środkowoeuropejski letni)",
  "properties": {}
}
```
# 5. Device Twin
```
{
	"deviceId": "Device_1",
	"etag": "AAAAAAAAAAQ=",
	"deviceEtag": "NjI2NTkyMzg4",
	"status": "enabled",
	"statusUpdateTime": "0001-01-01T00:00:00Z",
	"connectionState": "Connected",
	"lastActivityTime": "2024-05-15T15:00:05.2446811Z",
	"cloudToDeviceMessageCount": 0,
	"authenticationType": "sas",
	"x509Thumbprint": {
		"primaryThumbprint": null,
		"secondaryThumbprint": null
	},
	"modelId": "",
	"version": 36,
	"properties": {
		"desired": {
			"69": "69",
			"ProductionRate": 1096722042,
			"$metadata": {
				"69": {
					"$lastUpdated": "2024-05-15T15:00:53.8408487Z",
					"$lastUpdatedVersion": 4
				},
				"$lastUpdated": "2024-05-15T15:00:53.8408487Z",
				"$lastUpdatedVersion": 4,
				"ProductionRate": {
					"$lastUpdated": "2024-05-15T15:00:53.8408487Z",
					"$lastUpdatedVersion": 4
				}
			},
			"$version": 4
		},
		"reported": {
			"DateTimeLastAppLaunch": "2024-05-15T16:59:41.9872007+02:00",
			"ErrorStatus": "",
			"ProductionRate": 100,
			"$metadata": {
				"$lastUpdated": "2024-05-15T15:00:55.0893144Z",
				"DateTimeLastAppLaunch": {
					"$lastUpdated": "2024-05-15T14:59:43.476607Z"
				},
				"ErrorStatus": {
					"$lastUpdated": "2024-05-15T14:59:43.476607Z"
				},
				"ProductionRate": {
					"$lastUpdated": "2024-05-15T15:00:55.0893144Z"
				}
			},
			"$version": 32
		}
	},
	"capabilities": {
		"iotEdge": false
	}
}
```
# 5. Direct Methods
1. UpdateProductionRateup - Zwiększenie Production Rate o 10
```
private async Task<MethodResponse> UpdateProductionRateup(MethodRequest methodRequest, object userContext)
{
    var client = new OpcClient(OPCstring);
    client.Connect();
    await Task.Delay(1000);
    var ProdRate = new OpcReadNode($"ns=2;s={DeviceName}/ProductionRate");
    var tempProdRateVal = client.ReadNode(ProdRate);
    int FinalProdRateChange = ((int)(tempProdRateVal.As<float>() + 10));
    client.WriteNode($"ns=2;s={DeviceName}/ProductionRate", FinalProdRateChange);

    client.Disconnect();
    return new MethodResponse(0);
}
```
2. UpdateProductionRatedown - Zmniejszenie Production Rate o 10
```
private async Task<MethodResponse> UpdateProductionRatedown(MethodRequest methodRequest, object userContext)
{
    var client = new OpcClient(OPCstring);
    client.Connect();
    await Task.Delay(1000);
    var ProdRate = new OpcReadNode($"ns=2;s={DeviceName}/ProductionRate");
    var tempProdRateVal = client.ReadNode(ProdRate);
    int FinalProdRateChange = ((tempProdRateVal.As<int>() - 10));
    client.WriteNode($"ns=2;s={DeviceName}/ProductionRate", FinalProdRateChange);

    client.Disconnect();
    return new MethodResponse(0);
}
```
3. PowerON  - Włączenie produckji
```
private async Task<MethodResponse> PowerON(MethodRequest methodRequest, object userContext)
{
    var client = new OpcClient(OPCstring);
    client.Connect();
    await Task.Delay(10);
    client.WriteNode($"ns=2;s=Device 1/ProductionStatus", 1);
    Console.WriteLine("[Agent] Device Run! By Controller");
    client.Disconnect();
    return new MethodResponse(0);
}
```
4. PowerOFF - Wyłączenie produkcji
```
var client = new OpcClient(OPCstring);
client.Connect();
await Task.Delay(10);
client.WriteNode($"ns=2;s=Device 1/ProductionStatus", 0);
Console.WriteLine(client.ReadNode("ns=2;s=Device 1/ProductionStatus"));

client.Disconnect();
Console.WriteLine("[Agent] Device Stop! By Controller");
return new MethodResponse(0);
```
5. EmergencyStop - Zatrzymanie urządzenia emergency
```
private async Task<MethodResponse> EmergencyStop(MethodRequest methodRequest, object userContext)
{
    var client = new OpcClient(OPCstring);
    client.Connect();
    await Task.Delay(1000);
    client.CallMethod($"ns=2;s={DeviceName}", $"ns=2;s={DeviceName}/EmergencyStop");
    //var test = new OpcCallMethod("Device1", "ns=2;s={DeviceName}/EmergencyStop");

    client.Disconnect();
    Console.WriteLine("[Agent] STOP!");
    return new MethodResponse(0);
}
```
5. ResetErrors - Resetowanie błędów
```
private async Task<MethodResponse> ResetErrors(MethodRequest methodRequest, object userContext)
{
    var client = new OpcClient(OPCstring);
    client.Connect();
    await Task.Delay(1000);
    client.CallMethod($"ns=2;s={DeviceName}", $"ns=2;s={DeviceName}/ResetErrorStatus");

    client.Disconnect();
    Console.WriteLine("[Agent] Errors Reseted");
    return new MethodResponse(0);
}
```
