using SharedLibrary;
namespace ProductionLibrary;

public class MessageCreator : IProducer<Message>
{

    private static uint MessageCounter;

    [RateLimit(3)] 
    [Retry(5)]
    public Message Produce()
    {
        uint nextId = Interlocked.Increment(ref MessageCounter);

        if(nextId==uint.MaxValue)
        {
            Interlocked.Exchange(ref MessageCounter,0);
        }

        return new Message{MessageId = nextId,MessageContent = RandomSentence.GenerateRandomSentence() , MessageTime = DateTime.Now};
    }

}