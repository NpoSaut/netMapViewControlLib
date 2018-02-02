using System;
using System.Windows.Media.Imaging;
using MapVisualization.Elements;
using MapVisualization.TileLoaders.TilePathProvider;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов, обеспечивающий кэширование тайлов в файловой системе</summary>
    public class SimpleWebTileLoader : ITileLoader
    {
        private readonly ITilePathProvider _pathProvider;

        public SimpleWebTileLoader(ITilePathProvider PathProvider)
        {
            _pathProvider = PathProvider;
        }

        /// <summary>Загружает тайл с указанными индексами</summary>
        /// <param name="x">Горизонтальный индекс</param>
        /// <param name="y">Вертикальный индекс</param>
        /// <param name="zoom">Уровень масштабирования</param>
        /// <returns>ImageSource тайла</returns>
        public MapTileElement GetTile(int x, int y, int zoom)
        {
            var image = new BitmapImage(new Uri(_pathProvider.GetWebPath(x, y, zoom)));
            return new ImageSourceMapTileElement(x, y, image);
        }
    }
}
