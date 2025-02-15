namespace SharedLibrary;
public class MessageEnvelope<T>
{
    public uint ProducerId { get; set; }
    public T MessageContent { get; set; }
}
