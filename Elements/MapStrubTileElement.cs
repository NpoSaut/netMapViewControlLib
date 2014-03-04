using System.Windows;
using System.Windows.Media;

namespace MapVisualization.Elements
{
    internal class MapStrubTileElement : MapTileElement
    {
        public MapStrubTileElement(int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex, Zoom) { }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            dc.DrawRectangle(Brushes.LemonChiffon, null, TileRect);
        }
    }
}