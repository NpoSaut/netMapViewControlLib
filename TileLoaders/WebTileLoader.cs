using System.Net.Cache;
using System.Windows.Media.Imaging;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кеширование тайлов в файловой системе</summary>
    public class WebTileLoader : ITileLoader
    {
        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        /// <returns>ImageSource тайла</returns>
        public BitmapImage GetTile(int x, int y, int zoom) { return new BitmapImage(OsmIndexes.GetTileUri(x, y, zoom), new RequestCachePolicy()); }
    }
}
