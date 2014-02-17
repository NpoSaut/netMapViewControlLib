using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MapVisualization.Visuals;

namespace MapVisualization
{
    public class MapView : Panel
    {
        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapView), new FrameworkPropertyMetadata(typeof(MapView)));
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
    }
}
