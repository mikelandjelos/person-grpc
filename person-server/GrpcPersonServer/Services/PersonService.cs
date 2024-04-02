using System.Net;
using Grpc.Core;

namespace GrpcPersonServer.Services;

public class PersonService : GrpcPersonServer.PersonService.PersonServiceBase
{

    private static int counter = 0;
    private static List<Person> people = new List<Person>();
    private readonly ILogger<PersonService> _logger;

    static PersonService()
    {
        var rand = new Random();

        for (int i = 0; i < rand.Next(20, 50); ++i)
        {
            var name = Faker.Name.FullName();
            var email = Faker.Internet.Email(name.Split(' ')[0]);
            var phones = new List<PhoneNumber>();

            for (int j = 0; j < rand.Next(1, 5); ++j)
            {
                phones.Add(new PhoneNumber
                {
                    Number = Faker.Phone.Number(),
                    Type = j == 0 ? PhoneType.Mobile : j == 1 ? PhoneType.Home : PhoneType.Work,
                });
            }

            people.Add(new Person
            {
                Id = counter++,
                Name = name,
                Email = email,
                PhoneNumbers = { phones },
            });
        }
    }

    public PersonService(ILogger<PersonService> logger)
    {
        _logger = logger;
    }

    public override Task<CreatePersonResponse> CreatePerson(CreatePersonRequest request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            var person = request.Person.Clone();
            person.Id = counter++;
            people.Add(person);
            return Task.FromResult(new CreatePersonResponse
            {
                CreatedId = person.Id,
                Metadata = new ResponseMetadata
                {
                    Message = $"Succesfully created person with id {person.Id}!",
                    Status = (int)HttpStatusCode.Created
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return Task.FromResult(new CreatePersonResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = "Internal server error occured!",
                    Status = (int)HttpStatusCode.InternalServerError,
                }
            });
        }

    }

    public override Task<GetPersonResponse> GetPerson(GetPersonRequest request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            var retreivedPerson = people.Where(person => person.Id == request.Id).FirstOrDefault();
            return Task.FromResult(new GetPersonResponse
            {
                RetreivedPerson = retreivedPerson,
                Metadata = new ResponseMetadata
                {
                    Message = retreivedPerson != null ? "Person found!" : "Person not found!",
                    Status = (int)HttpStatusCode.OK,
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return Task.FromResult(new GetPersonResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = $"Internal server error occured!",
                    Status = (int)HttpStatusCode.InternalServerError,
                }
            });
        }
    }

    public override Task<GetPeopleResponse> GetPeople(GetPeopleRequest request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            if (request.StartingId < 0 && request.EndingId > people.Count)
                return Task.FromResult(new GetPeopleResponse
                {
                    Metadata = new ResponseMetadata
                    {
                        Message = $"Bad call parameters, starting id must be greater than 0, and ending must be smaller than {people.Count}",
                        Status = (int)HttpStatusCode.BadRequest,
                    }
                });

            return Task.FromResult(new GetPeopleResponse
            {
                RetreivedPeople = { people.Slice(request.StartingId, request.EndingId == -1 ? people.Count : request.EndingId) },
                Metadata = new ResponseMetadata
                {
                    Message = $"Number of people found: {people.Count}!",
                    Status = (int)HttpStatusCode.OK,
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return Task.FromResult(new GetPeopleResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = $"Internal server error occured!",
                    Status = (int)HttpStatusCode.InternalServerError,
                }
            });
        }
    }

    public override Task<UpdatePersonResponse> UpdatePerson(UpdatePersonRequest request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            var personToUpdate = people.Where(person => person.Id == request.Id).FirstOrDefault();

            if (personToUpdate == null)
                return Task.FromResult(new UpdatePersonResponse
                {
                    Metadata = new ResponseMetadata
                    {
                        Message = "Person not found",
                        Status = (int)HttpStatusCode.NotFound,
                    }
                });

            if (request.HasName)
                personToUpdate.Name = request.Name;


            if (request.HasEmail)
                personToUpdate.Email = request.Email;

            if (request.Phones.Count > 0)
            {
                personToUpdate.PhoneNumbers.Clear();
                personToUpdate.PhoneNumbers.AddRange(request.Phones);
            }

            return Task.FromResult(new UpdatePersonResponse
            {
                UpdatedPerson = personToUpdate,
                Metadata = new ResponseMetadata
                {
                    Message = $"Updated person with id {personToUpdate.Id}!",
                    Status = (int)HttpStatusCode.OK,
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return Task.FromResult(new UpdatePersonResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = $"Internal server error occured!",
                    Status = (int)HttpStatusCode.InternalServerError,
                }
            });
        }
    }

    public override Task<DeletePersonResponse> DeletePerson(DeletePersonRequest request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            var personToDelete = people.Find(person => person.Id == request.Id);

            if (personToDelete == null)
                return Task.FromResult(new DeletePersonResponse
                {
                    Metadata = new ResponseMetadata
                    {
                        Message = $"Person with ID {request.Id} not found!",
                        Status = (int)HttpStatusCode.OK,
                    },
                });

            people.Remove(personToDelete);

            return Task.FromResult(new DeletePersonResponse
            {
                DeletedId = request.Id,
                Metadata = new ResponseMetadata
                {
                    Message = $"Deleted person with id {request.Id}! Current number of people: {people.Count}.",
                    Status = (int)HttpStatusCode.OK,
                },
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
            return Task.FromResult(new DeletePersonResponse
            {
                Metadata = new ResponseMetadata
                {
                    Message = $"Internal server error occured!",
                    Status = (int)HttpStatusCode.InternalServerError,
                }
            });
        }
    }

    public override async Task DeletePeople(IAsyncStreamReader<DeletePersonRequest> requestStream, IServerStreamWriter<DeletePersonResponse> responseStream, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");

        try
        {
            int deletedCount = 0;
            while (true)
            {
                var nextAvailable = await requestStream.MoveNext();
                
                if (!nextAvailable)
                    break;

                if (requestStream.Current.Id == -1)
                {

                    await responseStream.WriteAsync(new DeletePersonResponse
                    {
                        Metadata = new ResponseMetadata
                        {
                            Message = $"Finished deleting! Deleted count: {deletedCount}.",
                            Status = (int)HttpStatusCode.OK,
                        },
                    });
                    break;
                }

                var personToDelete = people.Find(person => person.Id == requestStream.Current.Id);

                if (personToDelete == null)
                {
                    await responseStream.WriteAsync(new DeletePersonResponse
                    {
                        Metadata = new ResponseMetadata
                        {
                            Message = $"Person with ID {requestStream.Current.Id} not found!",
                            Status = (int)HttpStatusCode.OK,
                        },
                    });
                    continue;
                }

                people.Remove(personToDelete);

                await responseStream.WriteAsync(new DeletePersonResponse
                {
                    DeletedId = requestStream.Current.Id,
                    Metadata = new ResponseMetadata
                    {
                        Message = $"Deleted person with id {requestStream.Current.Id}! Current number of people: {people.Count}.",
                        Status = (int)HttpStatusCode.OK,
                    },
                });
                deletedCount++;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.StackTrace);
        }
    }
}
