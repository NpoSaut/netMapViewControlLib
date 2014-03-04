using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Geographics;
using MapVisualization.Elements;
using MapVisualization.TileLoaders;

namespace MapVisualization
{
    public class MapView : DrawingContainer
    {
        public static readonly DependencyProperty TileLoaderProperty = DependencyProperty.Register(
                                                                                                   "TileLoader",
                                                                                                   typeof (ITileLoader),
                                                                                                   typeof (MapView),
                                                                                                   new PropertyMetadata(
                                                                                                       AppDataFileCacheTileLoader
                                                                                                           .DefaultLoader));

        private readonly List<MapElement> _elements = new List<MapElement>();
        private readonly TranslateTransform _globalTransform;
        private Point? _dragStartPoint;

        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MapView), new FrameworkPropertyMetadata(typeof (MapView)));
        }

        public MapView()
        {
            TopLeftPoint = new EarthPoint(60.5515, 56.8563);
            ZoomLevel = 13;

            Point topLeftScreenCoordinate = ScreenProjector.DefaultProjector.Project(TopLeftPoint, ZoomLevel);
            _globalTransform = new TranslateTransform(-topLeftScreenCoordinate.X, -topLeftScreenCoordinate.Y);

            int x0 = OsmIndexes.GetHorizontalIndex(TopLeftPoint.Longitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(TopLeftPoint.Latitude, ZoomLevel);

            IEnumerable<MapTileElement> tiles =
                Enumerable.Range(x0, 4)
                          .SelectMany(x => Enumerable.Range(y0, 4).Select(y => GetMapTile(x, y, ZoomLevel)));
            foreach (MapTileElement tile in tiles)
            {
                AddElement(tile);
            }
        }

        public ITileLoader TileLoader
        {
            get { return (ITileLoader)GetValue(TileLoaderProperty); }
            set { SetValue(TileLoaderProperty, value); }
        }

        public EarthPoint TopLeftPoint { get; set; }
        public int ZoomLevel { get; set; }

        private MapTileElement GetMapTile(int x, int y, int zoom)
        {
            ImageSource image = TileLoader.GetTile(x, y, zoom);
            return new MapTileElement(image, x, y, zoom);
        }

        private void AddElement(MapElement Element)
        {
            _elements.Add(Element);
            MapVisual visual = Element.GetVisual(ZoomLevel);
            visual.Transform = _globalTransform;
            AddVisual(visual);
        }

        #region Скроллинг карты

        private void Move(Vector delta) { Move(delta.X, delta.Y); }

        public void Move(double dx, double dy)
        {
            _globalTransform.X += dx;
            _globalTransform.Y += dy;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dragCurrentPoint = e.GetPosition(this);
                if (_dragStartPoint != null) Move(dragCurrentPoint - _dragStartPoint.Value);
                _dragStartPoint = dragCurrentPoint;
            }
            base.OnMouseMove(e);
        }

        #endregion
    }
}
