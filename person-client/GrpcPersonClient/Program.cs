using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using GrpcPersonClient;
using GrpcPingClient;

using var channel = GrpcChannel.ForAddress("https://localhost:7100");
var pingClient = new PingService.PingServiceClient(channel);
var personClient = new PersonService.PersonServiceClient(channel);


// Ping testing

Console.WriteLine("========================================================");
Console.WriteLine("Test 1: Ping");
Console.WriteLine("========================================================");

var pingResponse = await pingClient.PingAsync(new Empty());
Console.WriteLine($"Ping response: \"{pingResponse}\"");

// Create person testing

Console.WriteLine("========================================================");
Console.WriteLine("Test 2: Create Person");
Console.WriteLine("========================================================");

Person personToCreate = new Person
{
    Name = "Mihajlo Madic",
    PhoneNumbers = {
        new PhoneNumber{Type=PhoneType.Home, Number="123-456-789"},
        new PhoneNumber{Type=PhoneType.Mobile, Number="456-456-456"}
    },
    Email = "mihajlo.madic@gmail.com"
};

var createPersonResponse = await personClient.CreatePersonAsync(new CreatePersonRequest { Person = personToCreate });
Console.WriteLine($"Created person with Id: {createPersonResponse.CreatedId}");

// Get person

Console.WriteLine("========================================================");
Console.WriteLine("Test 3: Get Person");
Console.WriteLine("========================================================");

var getPersonResponse = await personClient.GetPersonAsync(new GetPersonRequest { Id = createPersonResponse.CreatedId });
Console.WriteLine($"Retreived person: {getPersonResponse.RetreivedPerson}");

// Get people

Console.WriteLine("========================================================");
Console.WriteLine("Test 4: Get People");
Console.WriteLine("========================================================");

var getPeopleResponse = await personClient.GetPeopleAsync(new GetPeopleRequest { EndingId = -1 }); // Get all

Console.WriteLine("Retreived people:");
foreach (var person in getPeopleResponse.RetreivedPeople)
    Console.WriteLine($"{person.Id}. {person.Name} - {person.Email}");

// Update person

Console.WriteLine("========================================================");
Console.WriteLine("Test 5: Update Person");
Console.WriteLine("========================================================");

var updatePersonResponse = await personClient.UpdatePersonAsync(new UpdatePersonRequest { Email = "mihajlo.madic@elfak.rs" });
Console.WriteLine($"Updated email: \"{updatePersonResponse.UpdatedPerson.Email}\"");

// Delete person

Console.WriteLine("========================================================");
Console.WriteLine("Test 6: Delete Person");
Console.WriteLine("========================================================");

var deletePersonResponse = await personClient.DeletePersonAsync(new DeletePersonRequest { Id = getPersonResponse.RetreivedPerson.Id });
Console.WriteLine($"Deleted person with id: {deletePersonResponse.DeletedId}");