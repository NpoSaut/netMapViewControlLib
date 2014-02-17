using System.Windows;
using System.Windows.Media;

namespace MapVisualization.Elements
{
    public abstract class MapElement
    {
        protected abstract void Draw(DrawingContext dc);

        public MapVisual GetVisual()
        {
            var res = new MapVisual();
            using (var dc = res.RenderOpen())
            {
                Draw(dc);
            }
            return res;
        }
    }

    public class MapTileElement : MapElement
    {
        public ImageSource TileImage { get; private set; }

        public MapTileElement(ImageSource TileImage) { this.TileImage = TileImage; }

        protected override void Draw(DrawingContext dc)
        {
            dc.DrawImage(TileImage, new Rect(new Size(TileImage.Width, TileImage.Height)));
        }
    }
}