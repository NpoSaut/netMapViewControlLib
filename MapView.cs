using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshTiles();
        }

        private void RefreshTiles()
        {
            int x0 = OsmIndexes.GetHorizontalIndex(TopLeftPoint.Longitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(TopLeftPoint.Latitude, ZoomLevel);

            int w = (int)Math.Ceiling(this.ActualWidth / 256) + 1;
            int h = (int)Math.Ceiling(this.ActualHeight / 256) + 1;
            
            IEnumerable<MapTileElement> tiles =
                Enumerable.Range(x0, w)
                          .SelectMany(x => Enumerable.Range(y0, h)
                              .Where(y => !_elements.OfType<MapTileElement>().Any(t => t.HorizontalIndex == x && t.VerticalIndex == y))
                              .Select(y => GetMapTile(x, y, ZoomLevel)));
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

        public void Move(double dx, double dy) { Move(new Vector(dx, dy)); }
        public void Move(Vector delta)
        {
            _globalTransform.X += delta.X;
            _globalTransform.Y += delta.Y;

            var p0 = ScreenProjector.DefaultProjector.Project(TopLeftPoint, ZoomLevel);
            var p = p0 - delta;

            TopLeftPoint = ScreenProjector.DefaultProjector.InverseProject(p, ZoomLevel);

            RefreshTiles();
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
