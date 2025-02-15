namespace ConsumptionLibrary;

using System.Text.Json;
using SharedLibrary;
public class MessageConsumer : IConsumer<JsonElement>
{
    [ConsumeRateLimit(3)]
    [SubscribeId(0)]
    public void Consume(JsonElement messageContent)
    {
    
        if (messageContent.ValueKind == JsonValueKind.Null || messageContent.ValueKind == JsonValueKind.Undefined)
        {
            Console.WriteLine("an error in Parsing to target object...");
            Console.WriteLine(messageContent);
            return;
        }

        try
        {
            var message = JsonSerializer.Deserialize<Message>(messageContent.ToString());

            if (message == null)
            {
                Console.WriteLine("Failed to deserialize message content.");
                return;
            }

            Console.WriteLine("-------Consumed---------");
            Console.WriteLine($"Message ID: {message.MessageId}");
            Console.WriteLine($"Content: {message.MessageContent}");
            Console.WriteLine($"Timestamp: {message.MessageTime}");
            Console.WriteLine("Consumed message successfully!");
        }
        catch (JsonException ex)
        {

            Console.WriteLine($"Error deserializing message: {ex.Message}");
        }
    }
}




