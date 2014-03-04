using System.Windows;
using System.Windows.Media;

namespace MapVisualization.Elements
{
    public class MapImageTileElement : MapTileElement
    {
        public MapImageTileElement(ImageSource TileImage, int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex, Zoom)
        {
            this.TileImage = TileImage;
        }

        public ImageSource TileImage { get; private set; }

        protected override void DrawTile(DrawingContext dc, Rect TileRect) { dc.DrawImage(TileImage, TileRect); }
    }
}