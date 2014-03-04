using System.Linq;
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
}
