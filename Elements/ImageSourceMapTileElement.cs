using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapVisualization.Elements
{
    public class ImageSourceMapTileElement : MapTileElement
    {
        private readonly BitmapImage _image;

        public ImageSourceMapTileElement(int x, int y, BitmapImage Image) : base(x, y)
        {
            _image = Image;
        }

        public override void Dispose() { }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            dc.DrawImage(_image, TileRect);
        }
    }
}
