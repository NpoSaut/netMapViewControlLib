using System;
using System.Windows;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    public abstract class MapTileElement : MapElement, IDisposable
    {
        protected MapTileElement(int HorizontalIndex, int VerticalIndex)
        {
            this.VerticalIndex = VerticalIndex;
            this.HorizontalIndex = HorizontalIndex;
        }

        protected override int ZIndex
        {
            get { return -10; }
        }

        public int HorizontalIndex { get; private set; }
        public int VerticalIndex { get; private set; }

        protected override void Draw(DrawingContext dc, int RenderZoom)
        {
            EarthPoint topLeftPoint = OsmIndexes.GetTopLeftPoint(HorizontalIndex, VerticalIndex, RenderZoom);
            Point topLeftPointScreenProjection = Projector.Project(topLeftPoint, RenderZoom);
            var tileRect = new Rect(topLeftPointScreenProjection, new Size(256, 256));
            DrawTile(dc, tileRect);
        }

        protected abstract void DrawTile(DrawingContext dc, Rect TileRect);

        public override bool TestVisual(EarthArea VisibleArea) { return true; }

        public abstract void Dispose();
    }
}
