using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcPingServer.Services;



class PingService : GrpcPingServer.PingService.PingServiceBase
{

    private readonly ILogger<PingService> _logger;

    public PingService(ILogger<PingService> logger)
    {
        _logger = logger;
    }

    public override Task<Response> Ping(Empty request, ServerCallContext callContext)
    {
        _logger.LogInformation($"Peer: \"{callContext.Peer}\", Method: \"{callContext.Method}\"");
        return Task.FromResult(new Response
        {
            Message = $"PONG to {callContext.Peer}"
        });
    }
}