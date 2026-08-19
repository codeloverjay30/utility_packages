using System.Collections.Concurrent;

namespace ILoggerBuilderFactoryServices
{
    public sealed class FileLogManager : IDisposable
    {
        // 使用 Lazy 確保 Singleton 且執行緒安全
        private static readonly Lazy<FileLogManager> _instance =
            new Lazy<FileLogManager>(() => new FileLogManager());
        public static FileLogManager Instance => _instance.Value;

        private readonly BlockingCollection<string> _logQueue = new BlockingCollection<string>();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private string? _currentPath;
        private StreamWriter? _writer;

        private FileLogManager()
        {
            // 啟動背景寫入任務
            Task.Run(() => ProcessQueue() , _cts.Token);
        }

        public void Initialize(string filePath)
        {
            if(_currentPath == filePath) return;

            _currentPath = filePath;
            var directory = Path.GetDirectoryName(filePath);
            if(!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);

            // 開啟檔案流：FileShare.Read 允許其他程式（如記事本）讀取日誌
            var stream = new FileStream(filePath , FileMode.Append , FileAccess.Write , FileShare.Read);
            _writer = new StreamWriter(stream) { AutoFlush = true };
        }

        public void Enqueue(string message)
        {
            if(!_logQueue.IsAddingCompleted)
            {
                _logQueue.Add($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} {message}");
            }
        }

        private void ProcessQueue()
        {
            foreach(var message in _logQueue.GetConsumingEnumerable())
            {
                _writer?.WriteLine(message);
            }
        }

        public void Dispose()
        {
            _logQueue.CompleteAdding();
            _cts.Cancel();
            _writer?.Dispose();
        }
    }
}
