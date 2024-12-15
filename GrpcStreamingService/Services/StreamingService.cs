using Grpc.Core;

namespace GrpcStreamingService.Services;

public class StreamingService : Streaming.StreamingBase
{
    // Unary Call - Fetch User Info
    public override Task<UnaryResponse> UnaryCall(UnaryRequest request, ServerCallContext context)
    {
        return Task.FromResult(new UnaryResponse
        {
            Name = "Prashant Odhavani",
            Age = 26,
            Email = "prashant@example.com"
        });
    }

    // Server Streaming - Send Notifications Stream
    public override async Task ServerStreamingCall(StreamingRequest request, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
    {
        var notifications = new List<string>
            {
                "Welcome to the platform!",
                "You have a new message.",
                "Your profile has been updated.",
                "Your order has been shipped."
            };

        foreach (var notification in notifications)
        {
            await responseStream.WriteAsync(new StreamingResponse
            {
                Message = notification,
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            await Task.Delay(5000); 
        }
    }

    // Client Streaming - Receive User Activities
    public override async Task<StreamingResponse> ClientStreamingCall(IAsyncStreamReader<StreamingRequest> requestStream, ServerCallContext context)
    {
        var activities = new List<string>();
        await foreach (var request in requestStream.ReadAllAsync())
        {
            activities.Add(request.Activity);
            Console.WriteLine($"Received activity: {request.Activity}");
        }

        return new StreamingResponse
        {
            Summary = $"Received and stored {activities.Count} activities."
        };
    }

    // Bidirectional Streaming - Real-Time Chat
    public override async Task BidirectionalStreamingCall(IAsyncStreamReader<StreamingRequest> requestStream, IServerStreamWriter<StreamingResponse> responseStream, ServerCallContext context)
    {
        await foreach (var request in requestStream.ReadAllAsync())
        {
            Console.WriteLine($"Received: {request.ChatMessage}");

            await responseStream.WriteAsync(new StreamingResponse
            {
                ChatMessage = $"Server Echo: {request.ChatMessage}",
                Timestamp = DateTime.UtcNow.ToString("o")
            });

            await Task.Delay(5000);
        }
    }
}
