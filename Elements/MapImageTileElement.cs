using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapVisualization.Elements
{
    public class MapImageTileElement : MapTileElement
    {
        private readonly BitmapImage _tileImage;

        public MapImageTileElement(BitmapImage TileImage, int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex, Zoom)
        {
            _tileImage = TileImage;
            _tileImage.DownloadCompleted += (Sender, Args) =>
                                            {
                                                RequestChangeVisual();
                                                Debug.Print(" # Tile Loaded");
                                            };
        }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            if (_tileImage.IsDownloading)
                dc.DrawRectangle(Brushes.LemonChiffon, null, TileRect);
            else
                dc.DrawImage(_tileImage, TileRect);
        }
    }
}
