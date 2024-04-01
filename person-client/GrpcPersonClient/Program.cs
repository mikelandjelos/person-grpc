using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using GrpcPersonClient;
using GrpcPingClient;

using var channel = GrpcChannel.ForAddress("https://localhost:7100");
var pingClient = new PingService.PingServiceClient(channel);

var pingResponse = await pingClient.PingAsync(new Empty());

Console.WriteLine($"Ping response: \"{pingResponse}\"");
Console.WriteLine($"Press any key to continue...");
Console.ReadKey();

var personClient = new PersonService.PersonServiceClient(channel);

