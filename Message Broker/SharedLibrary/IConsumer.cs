namespace SharedLibrary;

public interface IConsumer<T>
{
    void Consume(T message);
}
