using Grpc.Core;
using Grpc.Net.Client;
using GrpcStreamingService;

var channel = GrpcChannel.ForAddress("https://localhost:7139");
var client = new Streaming.StreamingClient(channel);

// Unary Call - Example: Fetch User Info
Console.WriteLine("Unary Call - Fetch User Info:");
var unaryResponse = await client.UnaryCallAsync(new UnaryRequest { UserId = "12345" });
Console.WriteLine($"User Info: Name = {unaryResponse.Name}, Age = {unaryResponse.Age}, Email = {unaryResponse.Email}");

// Server Streaming - Example: Get Notifications Stream
Console.WriteLine("\nServer Streaming - Notifications Stream:");
using (var streamingCall = client.ServerStreamingCall(new StreamingRequest { UserId = "12345" }))
{
    await foreach (var notification in streamingCall.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Notification: {notification.Message} (Time: {notification.Timestamp})");
    }
}

// Client Streaming - Example: Upload User Activity
Console.WriteLine("\nClient Streaming - Upload User Activity:");
using (var clientStreamingCall = client.ClientStreamingCall())
{
    var activities = new[] { "Login", "Viewed Dashboard", "Updated Profile", "Logged Out" };

    foreach (var activity in activities)
    {
        await clientStreamingCall.RequestStream.WriteAsync(new StreamingRequest
        {
            UserId = "12345",
            Activity = activity,
            Timestamp = DateTime.UtcNow.ToString("o")
        });
        Console.WriteLine($"Sent activity: {activity}");
    }
    await clientStreamingCall.RequestStream.CompleteAsync();

    var activityResponse = await clientStreamingCall;
    Console.WriteLine($"Activity Upload Summary: {activityResponse.Summary}");
}

// Bidirectional Streaming - Example: Real-Time Chat
Console.WriteLine("\nBidirectional Streaming - Real-Time Chat:");
using (var bidirectionalCall = client.BidirectionalStreamingCall())
{
    var sendTask = Task.Run(async () =>
    {
        var messages = new[] { "Hello!", "How are you?", "What's the status of my order?", "Thanks, bye!" };

        foreach (var message in messages)
        {
            await bidirectionalCall.RequestStream.WriteAsync(new StreamingRequest
            {
                UserId = "12345",
                ChatMessage = message,
                Timestamp = DateTime.UtcNow.ToString("o")
            });
            Console.WriteLine($"Sent: {message}");
            await Task.Delay(500);
        }
        await bidirectionalCall.RequestStream.CompleteAsync();
    });

    await foreach (var chatResponse in bidirectionalCall.ResponseStream.ReadAllAsync())
    {
        Console.WriteLine($"Received: {chatResponse.ChatMessage} (Time: {chatResponse.Timestamp})");
    }

    await sendTask;
}