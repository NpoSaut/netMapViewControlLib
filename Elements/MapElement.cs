using System.Linq;
using System.Windows;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    public abstract class MapElement
    {
        protected static GuidelineSet ScreenGuidelineSet;

        static MapElement()
        {
            ScreenGuidelineSet = new GuidelineSet(
                Enumerable.Range(0, 40000).Select(x => (double)x).ToArray(),
                Enumerable.Range(0, 40000).Select(y => (double)y).ToArray());
        }

        protected ScreenProjector Projector
        {
            get { return ScreenProjector.DefaultProjector; }
        }

        protected abstract void Draw(DrawingContext dc, int Zoom);

        public MapVisual GetVisual(int Zoom)
        {
            var res = new MapVisual();
            using (DrawingContext dc = res.RenderOpen())
            {
                Draw(dc, Zoom);
            }
            return res;
        }
    }

    public abstract class MapTileElement : MapElement
    {
        public MapTileElement(int HorizontalIndex, int VerticalIndex, int Zoom)
        {
            this.Zoom = Zoom;
            this.VerticalIndex = VerticalIndex;
            this.HorizontalIndex = HorizontalIndex;
        }

        public int HorizontalIndex { get; private set; }
        public int VerticalIndex { get; private set; }
        public int Zoom { get; private set; }

        protected override void Draw(DrawingContext dc, int RenderZoom)
        {
            EarthPoint topLeftPoint = OsmIndexes.GetTopLeftPoint(HorizontalIndex, VerticalIndex, RenderZoom);
            Point topLeftPointScreenProjection = Projector.Project(topLeftPoint, RenderZoom);
            dc.PushGuidelineSet(ScreenGuidelineSet);
            var tileRect = new Rect(topLeftPointScreenProjection, new Size(256, 256));
            DrawTile(dc, tileRect);
            dc.DrawRectangle(null, new Pen(Brushes.Gray, 2), tileRect);
        }

        protected abstract void DrawTile(DrawingContext dc, Rect TileRect);
    }

    internal class MapStrubTileElement : MapTileElement
    {
        public MapStrubTileElement(int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex, Zoom) { }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            dc.DrawRectangle(Brushes.LemonChiffon, null, TileRect);
        }
    }

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
