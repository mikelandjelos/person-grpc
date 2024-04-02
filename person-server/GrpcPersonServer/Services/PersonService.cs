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

    #region  Basic CRUD - Unary RPCs

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
            if (request.Id >= people.Count || request.Id < 0)
                return Task.FromResult(new DeletePersonResponse
                {
                    Metadata = new ResponseMetadata
                    {
                        Message = $"Id of {request.Id} is not valid! Id must be between 0 and {people.Count}!",
                        Status = (int)HttpStatusCode.BadRequest,
                    },
                });

            people.RemoveAt(request.Id);

            return Task.FromResult(new DeletePersonResponse
            {
                DeletedId = request.Id,
                Metadata = new ResponseMetadata
                {
                    Message = $"Deleted person with id {request.Id}!",
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

    #endregion

    #region Streaming RPCs

    #endregion
}
