# How to Run the Consumer-Producer Messaging App

## Overview
This application is designed as a messaging system where consumers subscribe to specific producers and receive messages asynchronously. The system uses an ASP.NET Core Web API as the message broker and dynamically loads consumer plugins as DLLs. The consumers process messages based on a producer ID using attributes to define subscriptions and rate limits.

## Prerequisites
- .NET SDK (6.0 or later)
- Visual Studio or Visual Studio Code
- Postman or any API testing tool (optional, for testing the API directly)

## Project Structure
```
Root Directory
│
├── Broker           # ASP.NET Core Web API for message queue
│   
│   
│       
│
├── Consumer      # Console App for consuming messages
│   
│   
│
├── SharedLibrary # Shared    models and interfaces
│   
│
└── AddOns                      # Place for dynamic consumer DLLs

## other folders are examples of creating libraries for dll's!
```

## Interfaces and Attributes
- **IConsumer<T>**: Interface implemented by each consumer. It ensures consistency in processing messages.
- **[SubscribeId]**: Attribute for specifying which producer ID the consumer subscribes to.
- **[ConsumeRateLimit]**: Attribute for rate limiting message consumption.

### Example Usage
```csharp
public class MessageConsumer : IConsumer<JsonElement>
{
    [ConsumeRateLimit(3)]
    [SubscribeId(0)]
    public void Consume(JsonElement messageContent)
    {
        var message = JsonSerializer.Deserialize<Message>(messageContent.ToString());
        Console.WriteLine("Consumed: " + message.MessageContent);
    }
}
```

## Initializer Parameters
- **BaseAddress**: The base URL for the API (default: `http://localhost:5148`)
if after run api uses other ports make sure to change this in producer and consumer!
*in the next update I will automate thi*
- **TimeInterval**: a time interval to call MessageLogCleaner to clean acknowledged messages from MessageLog.txt file

 - **MaxFreeId's**:the total number of messages that we can hold on our disk , for preventing possible crashes I sat this to "50" , you can change it in the main program of webAPI app by passing it into the Initializer constructor. **the default value is 3000 messages**

## Setup and Configuration
1. **Build the Solution**:
    ```sh
    dotnet build
    ```

2. **Run the API**:
    Navigate to the `Api` folder and run:
    ```sh
    dotnet run
    ```
    This will start the API on `http://localhost:5148`.

3. **Place Consumer DLLs**:
    - Build the consumer class as a separate project.
    - Copy the generated DLL to the following path:
      ```
      RootDirectory/AddOns/ConsumerDll
      ```

4. **Run the Consumer App**:
    Navigate to the `Consumer` folder and run:
    ```sh
    dotnet run
    ```

## Running and Testing the Application
- Send POST requests to the API to enqueue messages. Example request:
    ```json
    POST http://localhost:5148/api/message
    {
        "producerId": 0,
        "messageContent": "Test message"
    }
    ```
- The Consumer App should pick up the message if the `[SubscribeId(0)]` attribute is used.

## Example DLLs
- Examples are provided in the `ProductionLibrary`and'ConsumptionLibrary' folders.

- place the compiled DLLs in the `AddOns` folder.

## Troubleshooting
- Make sure the API and Consumer apps are using the same base address.
- Check console logs for detailed error messages.

## Notes
- Modify rate limits using the `[ConsumeRateLimit]` attribute.

