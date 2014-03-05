using System;
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


        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof (MapView), new FrameworkPropertyMetadata(typeof (MapView)));
        }

        public MapView()
        {
            //TopLeftPoint = new EarthPoint(55.5466, 37.4696);
            TopLeftPoint = new EarthPoint(56.8302, 60.4928);
            ZoomLevel = 13;

            Point topLeftScreenCoordinate = ScreenProjector.DefaultProjector.Project(TopLeftPoint, ZoomLevel);
            _globalTransform = new TranslateTransform(-topLeftScreenCoordinate.X, -topLeftScreenCoordinate.Y);
        }

        public ITileLoader TileLoader
        {
            get { return (ITileLoader)GetValue(TileLoaderProperty); }
            set { SetValue(TileLoaderProperty, value); }
        }


        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            RefreshTiles();
        }

        private async void RefreshTiles()
        {
            int x0 = OsmIndexes.GetHorizontalIndex(TopLeftPoint.Longitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(TopLeftPoint.Latitude, ZoomLevel);

            int w = (int)Math.Ceiling(ActualWidth / 256) + 1;
            int h = (int)Math.Ceiling(ActualHeight / 256) + 1;

            for (int x = x0; x < x0 + w; x++)
                for (int y = y0; y < y0 + h; y++)
                {
                    if (!_elements.OfType<MapTileElement>().Any(t => t.HorizontalIndex == x && t.VerticalIndex == y))
                    {
                        var tempTile = new MapStrubTileElement(x, y, ZoomLevel);
                        Dispatcher.BeginInvoke((Action<MapElement>)AddElement, tempTile);

                        ImageSource tileImage = await TileLoader.GetTileAsync(x, y, ZoomLevel);
                        var tile = new MapImageTileElement(tileImage, x, y, ZoomLevel);
                        Dispatcher.BeginInvoke((Action<MapElement>)RemoveElement, tempTile);
                        Dispatcher.BeginInvoke((Action<MapElement>)AddElement, tile);
                    }
                }
        }

        #region Работа со списком элементов

        private readonly List<MapElement> _elements = new List<MapElement>();
        private readonly Dictionary<MapElement, MapVisual> _elementsToVisuals = new Dictionary<MapElement, MapVisual>();

        public void AddElement(MapElement Element)
        {
            _elements.Add(Element);
            MapVisual visual = Element.GetVisual(ZoomLevel);
            visual.Transform = _globalTransform;
            AddVisual(visual);
            _elementsToVisuals.Add(Element, visual);
        }

        private void RemoveElement(MapElement Element)
        {
            _elements.Remove(Element);
            if (_elementsToVisuals.ContainsKey(Element))
                DeleteVisual(_elementsToVisuals[Element]);
        }

        #endregion


        #region Скроллинг и позиционирование карты

        public EarthPoint TopLeftPoint { get; private set; }
        public int ZoomLevel { get; set; }

        private readonly TranslateTransform _globalTransform;
        private Point? _dragStartPoint;

        public void Move(double dx, double dy) { Move(new Vector(dx, dy)); }
        public void Move(Vector delta)
        {
            _globalTransform.X += delta.X;
            _globalTransform.Y += delta.Y;

            Point p0 = ScreenProjector.DefaultProjector.Project(TopLeftPoint, ZoomLevel);
            Point p = p0 - delta;

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
