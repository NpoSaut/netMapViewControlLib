using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapVisualization
{
    public class MapView : Panel
    {
        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapView), new FrameworkPropertyMetadata(typeof(MapView)));
        }

        public MapView()
        {
            AddVisual(GetVisual(100, 50));
            AddVisual(GetVisual(200, 250));
        }

        private TranslateTransform transform = new TranslateTransform();

        private MapVisual GetVisual(double x0, double y0)
        {
            var res = new MapVisual();
            using (var dc = res.RenderOpen())
            {
                dc.DrawRectangle(Brushes.BlueViolet, new Pen(Brushes.DarkMagenta, 3), new Rect(x0, y0, 50, 50));
            }
            res.Transform = transform;
            return res;
        }

        private readonly List<MapVisual> _visuals = new List<MapVisual>();

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return _visuals[index];
        }

        protected void AddVisual(MapVisual v)
        {
            _visuals.Add(v);
            base.AddVisualChild(v);
            base.AddLogicalChild(v);
        }

        protected void DeleteVisual(MapVisual v)
        {
            _visuals.Remove(v);
            base.RemoveVisualChild(v);
            base.RemoveLogicalChild(v);
        }

        public void Move(double dx, double dy)
        {
            transform.X += dx;
            transform.Y += dy;
        }
    }
}
