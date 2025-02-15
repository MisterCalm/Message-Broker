using System.Reflection;
using System.Text.Json;
using SharedLibrary;

namespace Consumer;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Consumer App Started...");

        var consumers = ConsumerLoader.LoadConsumers();

        HttpClient httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("http://localhost:5148");

        while (true)
        {
            foreach (var consumer in consumers)
            {
                var method = consumer.Method;
                var subscribeAttr = method.GetCustomAttribute<SubscribeIdAttribute>();

                if (subscribeAttr != null)
                {
                    uint producerId = (uint)subscribeAttr.producerId;

                    var response = await httpClient.GetAsync($"/api/message?producerId={producerId}");
                    if (response.IsSuccessStatusCode)
                    {
                        var messageString = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Message Received: {messageString}");

                        var envelope = JsonSerializer.Deserialize<MessageEnvelope<JsonElement>>(messageString);
                        Console.WriteLine("Envelope received!");

                        if (envelope != null && envelope.ProducerId == producerId)
                        {
                            var messageContent = envelope.MessageContent;
                            Console.WriteLine($"Invoking Consumer for Producer ID: {producerId}");


                            var rateLimitAttr = method.GetCustomAttribute<ConsumeRateLimitAttribute>();
                            if (rateLimitAttr != null)
                            {
                                if (!RateLimiter.CanConsume(rateLimitAttr.rate))
                                {
                                    Console.WriteLine("Rate limit reached, skipping...");
                                    continue;
                                }
                            }
                            Console.WriteLine("message content sent to dll for process");
                            method.Invoke(consumer.Instance, new object[] { messageContent });
                        }
                    }
                    else
                    {
                        Console.WriteLine($"No messages for Producer ID: {producerId}");
                    }
                }
            }

            await Task.Delay(1000);
        }
    }
}
