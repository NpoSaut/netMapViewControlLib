using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;

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

        protected abstract void Draw(DrawingContext dc, int Zoom);

        protected ScreenProjector Projector { get { return ScreenProjector.DefaultProjector; } }

        public MapVisual GetVisual(int Zoom)
        {
            var res = new MapVisual();
            using (var dc = res.RenderOpen())
            {
                Draw(dc, Zoom);
            }
            return res;
        }
    }

    public class MapTileElement : MapElement
    {
        public int HorizontalIndex { get; private set; }
        public int VerticalIndex { get; private set; }
        public int Zoom { get; private set; }

        public ImageSource TileImage { get; private set; }

        public MapTileElement(ImageSource TileImage, int HorizontalIndex, int VerticalIndex, int Zoom)
        {
            this.Zoom = Zoom;
            this.VerticalIndex = VerticalIndex;
            this.HorizontalIndex = HorizontalIndex;
            this.TileImage = TileImage;
        }

        protected override void Draw(DrawingContext dc, int RenderZoom)
        {
            var topLeftPoint = OsmIndexes.GetTopLeftPoint(HorizontalIndex, VerticalIndex, RenderZoom);
            var topLeftPointScreenProjection = Projector.Project(topLeftPoint, RenderZoom);
            dc.PushGuidelineSet(ScreenGuidelineSet);
            dc.DrawImage(TileImage, new Rect(topLeftPointScreenProjection, new Size(TileImage.Width, TileImage.Height)));
        }
    }
}