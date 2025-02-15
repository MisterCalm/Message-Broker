using System.Collections.Concurrent;

namespace Broker;

public class MessageQueue<T>
{
    private readonly  ConcurrentQueue<T> _queue;

    public MessageQueue()
    {
        _queue = new ConcurrentQueue<T>();
    }

    public void Enqueue(T message)
    {
        _queue.Enqueue(message);
    }

    public T Dequeue()
    {
        if(_queue.TryDequeue(out T message))
        {
            return message;
        }
        return default(T);
    }

    public T Peek()
    {
        if(_queue.TryPeek(out T message))
        {
            return message;
        }
        return default(T);
    }

    public int CountQueue () => _queue.Count;
}