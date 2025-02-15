using Broker;
using System.Collections.Concurrent;
public class Initializer
{
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FileDirectory", "MessageLog.txt");
    private readonly  ConcurrentQueue<string> _messageQueue = new ConcurrentQueue<string>();
    private readonly ConcurrentDictionary<uint, byte> _freeId = new ConcurrentDictionary<uint, byte>();
    private readonly MessageCleaner cleaner;
        
    public Initializer(int MessageCleanerIntervalTime = 60000, uint maxId = 5000)
    {
        InitFreeId(maxId);
        RestoreData();
        cleaner = new MessageCleaner(MessageCleanerIntervalTime);
    }

    public void RestoreData()
    {
        if (new FileInfo(_filePath).Length != 0)
        {
            List<string> messages = new List<string>(File.ReadAllLines(_filePath));

            lock (_freeId)
            {
                foreach (string message in messages)
                {
                    var parts = message.Split("|");

                    if (parts.Length > 3)
                    {
                        _messageQueue.Enqueue($"{parts[1]}|{parts[2]}|{parts[3]}");

                        if (uint.TryParse(parts[1], out uint id))
                        {
                            _freeId.TryRemove(id, out _);
                        }
                        else
                        {
                            Console.WriteLine("Invalid number format!");
                        }
                    }
                }
            }
        }
    }

    private void InitFreeId(uint maxId = 5000)
    {
        for (uint i = 0; i < maxId; i++)
        {
            _freeId.TryAdd(i, 0);
        }
    }

    public ConcurrentQueue<string> GetMessageQueue()
    {
        return _messageQueue;
    }

    public string GetFilePath()
    {
        return _filePath;
    }

    public ConcurrentDictionary<uint, byte> GetFreeId()
    {
        return _freeId;
    }


}
