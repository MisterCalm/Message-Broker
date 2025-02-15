public interface IConsumer
{

    void Initialize(string brokerUrl);

    void StartConsuming();

    void StopConsuming();
}