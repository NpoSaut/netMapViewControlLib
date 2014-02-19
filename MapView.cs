using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Geographics;
using MapVisualization.Elements;
using MapVisualization.TileLoaders;

namespace MapVisualization
{
    public class MapView : DrawingContainer
    {
        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapView), new FrameworkPropertyMetadata(typeof(MapView)));
        }

        public static readonly DependencyProperty TileLoaderProperty = DependencyProperty.Register(
                                                        "TileLoader", typeof (ITileLoader), typeof (MapView), new PropertyMetadata(AppDataFileCacheTileLoader.DefaultLoader));

        public ITileLoader TileLoader
        {
            get { return (ITileLoader)GetValue(TileLoaderProperty); }
            set { SetValue(TileLoaderProperty, value); }
        }

        public EarthPoint TopLeftPoint { get; set; }
        public int ZoomLevel { get; set; }

        public MapView()
        {
            TopLeftPoint = new EarthPoint(60.5515, 56.8563);
            ZoomLevel = 13;

            var topLeftScreenCoordinate = ScreenProjector.DefaultProjector.Project(TopLeftPoint, ZoomLevel);
            _globalTransform = new TranslateTransform(-topLeftScreenCoordinate.X, -topLeftScreenCoordinate.Y);

            int x0 = OsmIndexes.GetHorizontalIndex(TopLeftPoint.Longitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(TopLeftPoint.Latitude, ZoomLevel);

            var tiles = Enumerable.Range(x0, 4).SelectMany(x => Enumerable.Range(y0, 4).Select(y => GetMapTile(x, y, ZoomLevel)));
            foreach (var tile in tiles)
            {
                AddElement(tile);
            }
        }

        private MapTileElement GetMapTile(int x, int y, int zoom)
        {
            var image = TileLoader.GetTile(x, y, zoom);
            return new MapTileElement(image, x, y, zoom);
        }

        private readonly List<MapElement> _elements = new List<MapElement>();

        private void AddElement(MapElement Element)
        {
            _elements.Add(Element);
            var visual = Element.GetVisual(ZoomLevel);
            visual.Transform = _globalTransform;
            AddVisual(visual);
        }

        private readonly TranslateTransform _globalTransform;

        public void Move(double dx, double dy)
        {
            _globalTransform.X += dx;
            _globalTransform.Y += dy;
        }
    }
}
