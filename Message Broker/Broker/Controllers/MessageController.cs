using Microsoft.AspNetCore.Mvc;
using Broker;
using System.Text.Json;
using System.Collections.Concurrent;
using System.Formats.Asn1;

namespace BrokerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        ConcurrentQueue<string> _messageQueue;
        ConcurrentDictionary<uint,byte> freeId;
        string _filePath;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly object _GetIdLock = new object();


        public MessageController(Initializer initializer)
        {
            _messageQueue = initializer.GetMessageQueue();
            _filePath = initializer.GetFilePath();
            freeId = initializer.GetFreeId();
        }

    // POST api/messages - Receive message from Producer
    [HttpPost]
    public IActionResult PostMessage([FromBody] JsonElement json)
    {
        try
        {
            var message = json.ToString();

            if (string.IsNullOrWhiteSpace(message))
            {
                return BadRequest("Message content cannot be empty.");
            }

            using (JsonDocument doc = JsonDocument.Parse(message))
            {
                if (doc.RootElement.TryGetProperty("producerId", out JsonElement producerIdElement))
                {
                    if (producerIdElement.TryGetUInt32(out uint producerId))
                    {
                        uint MessageId = GetMessageId(freeId,message,producerId);
                        _messageQueue.Enqueue($"{MessageId}|{producerId}|{message}");
                        return Ok("Message received.");
                    }
                    else
                    {
                        return BadRequest("Invalid ProducerId format.");
                    }
                }
                else
                {
                    return BadRequest("ProducerId not found in the message.");
                }
            }
        }
        catch (JsonException ex)
        {
            return BadRequest($"Failed to process the message: {ex.Message}");
        }
    }

    // GET api/messages - Send message to Consumer
[HttpGet]
public async Task<IActionResult> GetMessage([FromQuery] uint producerId)
{
    while (true)
    {
        foreach (var message in _messageQueue)
        {
            var parts = message.Split("|");
            if (parts.Length > 2 && 
                uint.TryParse(parts[1], out uint msgProducerId) && 
                msgProducerId == producerId)
            {
                if (_messageQueue.TryPeek(out string peekedMessage) && peekedMessage == message)
                {
                    if (_messageQueue.TryDequeue(out string dequeuedMessage))
                    {
                        if (uint.TryParse(parts[0], out uint messageId))
                        {
                            FreeMessageId(dequeuedMessage, messageId);
                            Console.WriteLine($"Message sent to Consumer for Producer ID: {producerId}");
                            return Ok(parts[2]);
                        }
                        else
                        {
                            Console.WriteLine("Invalid message ID format.");
                        }
                    }
                }
            }
        }

        await Task.Delay(100);
    }
}



        /////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////My Additional Methods/////////////////////////////////////

        private uint GetMessageId(ConcurrentDictionary<uint,byte> freeId,string message,uint producerId)
        {
            uint id;
            
            if(freeId.Count>0)
            {
                id = GetId();
                System.IO.File.AppendAllText(_filePath, $"PENDING|{id}|{producerId}|{message}{Environment.NewLine}");
                return id;
            }
            else
            {
                
                waitTillfreeId().Wait();
                id = GetId();
                System.IO.File.AppendAllText(_filePath, $"PENDING|{id}|{producerId}|{message}{Environment.NewLine}");
                return id;
            }
            
        }
        private async Task waitTillfreeId()
        {
            while(freeId.Count==0)
            {
                await Task.Delay(100);
            }
        }
        private uint GetId()
        {
            uint id;

            lock(_GetIdLock)
            {
                id = freeId.Keys.FirstOrDefault();
                byte firstValue  = freeId.Values.FirstOrDefault();
                freeId.TryRemove(id,out firstValue);
            }
                return id;
        }

        private async void SendMessage()
        {
            
        }

        private void FreeMessageId(string message,uint messageId)
        {
            freeId.TryAdd(messageId,0);
            System.IO.File.AppendAllText(_filePath,$"ACKNOWLEDGED|{message}{Environment.NewLine}");
        }
    
    }
}
