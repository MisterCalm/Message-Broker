using System.Reflection;
using System.Text.Json;
using SharedLibrary;
namespace ProducerNameSpace;

public class Producer<T> : IProducer<T>
{
    private readonly string _pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"..","..", "..", "..", "AddOns", "ProducerDll");
    private readonly HttpClient _httpClient = new();
    private readonly Dictionary<IProducer<T>, uint> _producers = new();

    public Producer()
{
    Console.WriteLine("Inside Producer Constructor");
    _httpClient.BaseAddress = new Uri("http://localhost:5148");
    LoadProducers();  
    ProduceMessages();        
    Console.WriteLine("Producers loaded and started.");

}


    public void LoadProducers()
    {
        var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll");
        uint producerId = 0;

        foreach (var dll in dllFiles)
        {
            var assembly = Assembly.LoadFrom(dll);
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                if (typeof(IProducer<T>).IsAssignableFrom(type) && !type.IsInterface)
                {
                    var instance = (IProducer<T>)Activator.CreateInstance(type)!;
                    _producers.Add(instance, producerId);
                    producerId++;
                }
            }
        }
    }

    private T InvokeProduce(IProducer<T> producer)
    {
        var produceMethod = producer.GetType().GetMethod("Produce");
        T message = (T)produceMethod.Invoke(producer, null);

        return message;
    }

    public T Produce()
    {
        throw new NotImplementedException("This is just a placeholder. The reflective invocation is handled elsewhere.");
    }

    public void ProduceMessages()
    {
        foreach (var producerPair in _producers)
        {
            var producer = producerPair.Key;
            var producerId = producerPair.Value;

            var produceMethod = producer.GetType().GetMethod("Produce");

            if (produceMethod != null)
            {
                var rateLimitAttr = produceMethod.GetCustomAttribute<RateLimitAttribute>();
                var retryAttr = produceMethod.GetCustomAttribute<RetryAttribute>();

                int threadCount = rateLimitAttr?.ThreadCount ?? 1;
                int retryCount = retryAttr?.RetryCount ?? 3;

                for (int i = 0; i < threadCount; i++)
                {
                    Task.Run(() =>
                    {
                        while (true)
                        {
                            T message = InvokeProduce(producer);
                            Retry(message, retryCount, producerId);
                        }
                    });
                }
            }
        }
    }

    private async void Retry(T message, int maxAttempts, uint producerId)
    {
        int attempts = 0;

        var envelope = new MessageEnvelope<T>
        {
            ProducerId = producerId,
            MessageContent = message
        };

        while (attempts < maxAttempts)
        {
            try
            {
                string serialized = JsonSerializer.Serialize(envelope, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });
                await SendPostRequest(serialized);
                Console.WriteLine($"Message from Producer {producerId} sent successfully.");
                break;
            }
            catch (Exception ex)
            {
                attempts++;
                Console.WriteLine($"Error: {ex.Message}. Retry attempt: {attempts}");
                await Task.Delay(1000);
            }
        }

        if (attempts == maxAttempts)
        {
            Console.WriteLine($"Failed to send message after {maxAttempts} attempts.");
        }
    }

    private async Task SendPostRequest(string message)
    {
        var content = new StringContent(message, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/api/message", content);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception("Failed to send message");
        }
    }
}
