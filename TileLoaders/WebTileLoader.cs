using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MapVisualization.Elements;
using MapVisualization.TileLoaders.TilePathProvider;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кэширование тайлов в файловой системе</summary>
    public class WebTileLoader : ITileLoader, IDisposable
    {
        private readonly ITilePathProvider _pathProvider;
        private readonly WebPool _webPool = new WebPool(10);

        public WebTileLoader(ITilePathProvider PathProvider)
        {
            _pathProvider = PathProvider;
        }

        public void Dispose()
        {
            _webPool?.Dispose();
        }

        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        public MapTileElement GetTile(int x, int y, int zoom)
        {
            var context = new LoadingContext(_pathProvider.GetLocalPath(x, y, zoom),
                                             _pathProvider.GetWebPath(x, y, zoom),
                                             _webPool);
            context.BeginLoading();
            return new ContextMapTileElement(context, x, y, zoom);
        }

        private class LoadingContext : ITileLoadingContext
        {
            private readonly CancellationTokenSource _cancel = new CancellationTokenSource();
            private readonly string _localPath;
            private readonly string _webPath;
            private readonly WebPool _webPool;

            public LoadingContext(string LocalPath, string WebPath, WebPool WebPool)
            {
                _localPath = LocalPath;
                _webPath   = WebPath;
                _webPool   = WebPool;
            }

            public bool               IsReady { get; private set; }
            public event EventHandler Ready;

            public void Abort()
            {
                _cancel.Cancel();
            }

            public BitmapImage Image { get; private set; }

            private void OnReady()
            {
                var handler = Ready;
                handler?.Invoke(this, EventArgs.Empty);
            }

            public async void BeginLoading()
            {
                try
                {
                    if (!File.Exists(_localPath) || DateTime.Now - File.GetLastWriteTime(_localPath) > TimeSpan.FromDays(40))
                    {
                        var tileData = await _webPool.Run(_webPath, _cancel.Token)
                                                     .ConfigureAwait(false);

                        var tilesDirectoryName = Path.GetDirectoryName(_localPath);
                        if (tilesDirectoryName != null)
                            Directory.CreateDirectory(tilesDirectoryName);
                        File.WriteAllBytes(_localPath, tileData);
                    }

                    Image = new BitmapImage(new Uri(_localPath));
                    Image.Freeze();

                    IsReady = true;
                    OnReady();
                }
                catch (Exception e)
                {
                    Debug.Print(" # Load tile error: {0}", e.Message);
                }
            }
        }

        private class WebPool : IDisposable
        {
            private readonly object _locker = new object();

            private readonly List<WebPoolElement> _pool;

            public WebPool(int PoolSize)
            {
                _pool = Enumerable.Range(0, PoolSize)
                                  .Select(id => new WebPoolElement(id))
                                  .ToList();
            }

            public void Dispose()
            {
                foreach (var poolElement in _pool) poolElement.Dispose();
            }

            public Task<byte[]> Run(string Url, CancellationToken Cancel)
            {
                lock (_locker)
                {
                    Cancel.ThrowIfCancellationRequested();
                    return _pool.OrderBy(e => e.QueueSize)
                                .First()
                                .Run(Url, Cancel);
                }
            }
        }

        private class WebPoolElement : IDisposable
        {
            private readonly WebClient _client = new WebClient();
            private readonly int _id;
            private readonly object _locker = new object();
            private readonly Queue<PoolTask> _tasks = new Queue<PoolTask>();
            private PoolTask _currentTask;
            private CancellationTokenRegistration _currentTaskCancelRegistration;

            public WebPoolElement(int Id)
            {
                _id                           =  Id;
                _client.DownloadDataCompleted += ClientOnDownloadDataCompleted;
            }

            public int QueueSize => _tasks.Count;

            public void Dispose()
            {
                _client?.Dispose();
            }

            private void Next()
            {
                lock (_locker)
                {
                    if (_tasks.Count > 0)
                    {
                        var task = _tasks.Dequeue();
                        if (!task.Cancel.IsCancellationRequested)
                        {
                            _currentTask = task;
                            _client.DownloadDataAsync(new Uri(_currentTask.Url));
                            _currentTaskCancelRegistration = _currentTask.Cancel.Register(_client.CancelAsync);
                        }
                        else
                        {
                            Next();
                        }
                    }
                }
            }

            private void ClientOnDownloadDataCompleted(object Sender, DownloadDataCompletedEventArgs e)
            {
                lock (_locker)
                {
                    _currentTaskCancelRegistration.Dispose();
                    if (e.Error != null)
                        _currentTask.CompletionSource.SetException(e.Error);
                    else if (e.Cancelled)
                        _currentTask.CompletionSource.SetCanceled();
                    else
                        _currentTask.CompletionSource.SetResult(e.Result);

                    _currentTask = null;
                    Next();
                }
            }

            public Task<byte[]> Run(string Url, CancellationToken Cancel)
            {
                lock (_locker)
                {
                    Cancel.ThrowIfCancellationRequested();
                    var task = new PoolTask(Url, Cancel);
                    _tasks.Enqueue(task);
                    if (_currentTask == null)
                        Next();
                    return task.CompletionSource.Task;
                }
            }

            private class PoolTask
            {
                public PoolTask(string Url, CancellationToken Cancel)
                {
                    this.Url         = Url;
                    this.Cancel      = Cancel;
                    CompletionSource = new TaskCompletionSource<byte[]>();
                }

                public string                       Url              { get; }
                public CancellationToken            Cancel           { get; }
                public TaskCompletionSource<byte[]> CompletionSource { get; }
            }
        }
    }
}
