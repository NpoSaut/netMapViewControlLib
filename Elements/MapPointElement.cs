using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    public abstract class MapPointElement : MapElement
    {
        public MapPointElement(EarthPoint Position) { this.Position = Position; }
        public EarthPoint Position { get; private set; }

        protected abstract void DrawPointElement(DrawingContext dc);

        protected override void Draw(DrawingContext dc, int Zoom)
        {
            var elementPoint = Projector.Project(Position, Zoom);
            dc.PushTransform(new TranslateTransform(elementPoint.X, elementPoint.Y));
            DrawPointElement(dc);
            dc.Pop();
        }

        public override bool TestVisual(EarthArea VisibleArea) { return Position.IsInArea(VisibleArea); }
    }
}
