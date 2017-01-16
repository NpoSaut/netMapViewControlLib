using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Media.Imaging;
using MapVisualization.Annotations;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кеширование тайлов в файловой системе</summary>
    public class WebTileLoader : ITileLoader
    {
        private static readonly string _tilesCacheRoot =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                         "Saut",
                         Assembly.GetExecutingAssembly().GetName().Name,
                         "Tiles Cache");

        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        /// <returns>ImageSource тайла</returns>
        public ITileLoadingContext GetTile(int x, int y, int zoom)
        {
            var context = new LoadingContext(x, y, zoom);
            context.BeginLoading();
            return context;
        }

        private class LoadingContext : ITileLoadingContext
        {
            private readonly int _x;
            private readonly int _y;
            private readonly int _zoom;
            private WebClient _webClient;

            public LoadingContext(int X, int Y, int Zoom)
            {
                _x = X;
                _y = Y;
                _zoom = Zoom;
            }

            public bool IsReady { get; private set; }
            public BitmapImage Image { get; private set; }
            public event EventHandler Ready;

            public void Abort()
            {
                if (_webClient != null)
                    _webClient.CancelAsync();
            }

            protected virtual void OnReady()
            {
                EventHandler handler = Ready;
                if (handler != null) handler(this, EventArgs.Empty);
            }

            public async void BeginLoading()
            {
                try
                {
                    string tileImageFileName = CachePath();
                    if (!File.Exists(tileImageFileName))
                    {
                        using (_webClient = new WebClient())
                        {
                            byte[] tileData = await _webClient.DownloadDataTaskAsync(OsmIndexes.GetTileUri(_x, _y, _zoom));
                            Directory.CreateDirectory(Path.GetDirectoryName(tileImageFileName));
                            File.WriteAllBytes(tileImageFileName, tileData);
                        }
                    }
                    Image = new BitmapImage(new Uri(tileImageFileName));
                    IsReady = true;
                    OnReady();
                    Debug.Print(" # {0}:{1} Ready", _x, _y);
                }
                catch (Exception e)
                {
                    Debug.Print(" # {0}:{1} {2}", _x, _y, e.Message);
                }
            }

            private string CachePath()
            {
                return Path.Combine(_tilesCacheRoot,
                                    _zoom.ToString(),
                                    String.Format("{0}.{1}.png", _x, _y));
            }
        }
    }
}
