using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кеширование тайлов в файловой системе</summary>
    public class FileCacheTileLoader : ITileLoader
    {
        /// <summary>Создаёт загрузчик тайлов с файловым кэшем, расположенным по указанному пути</summary>
        /// <param name="CachePath">Путь к директории с кэшем</param>
        public FileCacheTileLoader(string CachePath) { this.CachePath = CachePath; }

        /// <summary>Путь к директории с кэшем</summary>
        public string CachePath { get; private set; }

        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        /// <returns>ImageSource тайла</returns>
        public async Task<ImageSource> GetTileAsync(int x, int y, int zoom)
        {
            var tilesDir =
                new DirectoryInfo(Path.Combine(CachePath, zoom.ToString(CultureInfo.InvariantCulture)));
            var tileFile = new FileInfo(Path.Combine(tilesDir.FullName, string.Format("{0}.{1}.png", x, y)));
            if (!tilesDir.Exists) tilesDir.Create();
            if (!tileFile.Exists)
            {
                Uri u = OsmIndexes.GetTileUri(x, y, zoom);
                using (var wc = new WebClient())
                {
                    Debug.Print(" # Downloading Map tile: {0}", u);
                    await wc.DownloadFileTaskAsync(u, tileFile.FullName);
                    Debug.Print(" # Downloading done");
                }
            }
            return new BitmapImage(new Uri(tileFile.FullName));
        }
    }
}
