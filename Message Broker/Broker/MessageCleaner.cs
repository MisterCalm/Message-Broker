using System;
using System.IO;

namespace Broker
{
    public class MessageCleaner
    {
        public static readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FileDirectory", "MessageLog.txt");
        private static readonly string _tempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "FileDirectory", "Temp.txt");
        private static readonly object _fileLock = new object();
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly int _intervalTime;
        Task _cleaningTask;


        public MessageCleaner(int intervalTime = 60000)
        {
            _intervalTime = intervalTime;
            _cancellationTokenSource = new CancellationTokenSource();
            _cleaningTask = StartCleaner();
        }

        private async Task StartCleaner()
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            lock(_fileLock)
            {
            MessageLogCleaner();
            Console.WriteLine($"Cleaning completed at {DateTime.Now:HH:mm:ss.fff}");
            }
            await Task.Delay(_intervalTime, _cancellationTokenSource.Token);
        }
    }

        public void MessageLogCleaner()
        {
            var lines = new List<string>(File.ReadAllLines(_filePath));
            var pendingMessages = new Dictionary<string, string>();
            var acknowledgedMessages = new HashSet<string>();

            foreach (var line in lines)
            {
                var parts = line.Split('|');
                if (parts.Length < 2) continue;
                
                string status = parts[0];
                string messageId = parts[1];
                
                if (status == "PENDING")
                {
                    pendingMessages[messageId] = line;
                }
                else if (status == "ACKNOWLEDGED")
                {
                    acknowledgedMessages.Add(messageId);
                }
            }

            foreach (var ackId in acknowledgedMessages)
            {
                pendingMessages.Remove(ackId);
            }

            File.WriteAllLines(_tempPath,pendingMessages.Values.ToList());

            File.Replace(_tempPath, _filePath, null);

            File.AppendAllText(_tempPath,string.Empty);
        

        }

        public void Stop()
    {
        _cancellationTokenSource.Cancel();
        try
        {
            _cleaningTask.Wait();
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Cleaning stopped gracefully.");
        }
    }

    }
}
