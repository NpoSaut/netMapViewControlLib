using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapVisualization
{
    public class DrawingContainer : Panel
    {
        private readonly List<MapVisual> _visuals = new List<MapVisual>();

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        protected override Visual GetVisualChild(int index) { return _visuals[index]; }

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
    }
}
