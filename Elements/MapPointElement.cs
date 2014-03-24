using System.Windows;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    public abstract class MapPointElement : MapElement
    {
        private EarthPoint _position;
        public MapPointElement(EarthPoint Position) { _position = Position; }

        public EarthPoint Position
        {
            get { return _position; }
            set
            {
                _position = value;
                OnPositionChanged();
            }
        }

        /// <summary>Возникает при изменении позиции точки</summary>
        protected virtual void OnPositionChanged() { RequestChangeVisual(); }

        protected abstract void DrawPointElement(DrawingContext dc);

        protected override void Draw(DrawingContext dc, int Zoom)
        {
            Point elementPoint = Projector.Project(Position, Zoom);
            dc.PushTransform(new TranslateTransform(elementPoint.X, elementPoint.Y));
            DrawPointElement(dc);
            dc.Pop();
        }

        public override bool TestVisual(EarthArea VisibleArea) { return Position.IsInArea(VisibleArea); }
    }
}
