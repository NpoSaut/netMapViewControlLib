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
            Projector = ScreenProjector.DefaultProjector;
            ZoomLevel = 13;

            Point topLeftScreenCoordinate = ScreenProjector.DefaultProjector.Project(CentralPoint, ZoomLevel);
            _globalTransform = new TranslateTransform(-topLeftScreenCoordinate.X, -topLeftScreenCoordinate.Y);
        }

        public ScreenProjector Projector { get; private set; }

        public ITileLoader TileLoader
        {
            get { return (ITileLoader)GetValue(TileLoaderProperty); }
            set { SetValue(TileLoaderProperty, value); }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            var delta = new Vector(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width, sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height);
            var oldScreenCentralPoint = Projector.Project(CentralPoint, ZoomLevel);
            var newScreenCentralPoint = oldScreenCentralPoint + 0.5 * delta;

            CentralPoint = Projector.InverseProject(newScreenCentralPoint, ZoomLevel);
            RefreshTiles();
        }

        private async void RefreshTiles()
        {
            int x0 = OsmIndexes.GetHorizontalIndex(MostWesternLongitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(MostNorthenLatitude, ZoomLevel);

            int w = (int)Math.Ceiling(ActualWidth / 256) + 1;
            int h = (int)Math.Ceiling(ActualHeight / 256) + 1;

            for (int x = x0; x < x0 + w; x++)
                for (int y = y0; y < y0 + h; y++)
                {
                    if (!_elements.OfType<MapTileElement>().Any(t => t.HorizontalIndex == x && t.VerticalIndex == y))
                    {
                        var tempTile = new MapStrubTileElement(x, y, ZoomLevel);
                        Dispatcher.BeginInvoke((Action<MapElement>)AddElement, tempTile);

                        try
                        {
                            ImageSource tileImage = await TileLoader.GetTileAsync(x, y, ZoomLevel);
                            var tile = new MapImageTileElement(tileImage, x, y, ZoomLevel);
                            Dispatcher.BeginInvoke((Action<MapElement>)RemoveElement, tempTile);
                            Dispatcher.BeginInvoke((Action<MapElement>)AddElement, tile);
                        }
                        catch (Exception) { }
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

        #region CentralPoint DependencyProperty

        public static readonly DependencyProperty CentralPointProperty =
            DependencyProperty.Register("CentralPoint", typeof (EarthPoint), typeof (MapView),
                                        new PropertyMetadata(new EarthPoint(56.8302, 60.4928),
                                                             CentralPointPropertyChangedCallback));

        public EarthPoint CentralPoint
        {
            get { return (EarthPoint)GetValue(CentralPointProperty); }
            set { SetValue(CentralPointProperty, value); }
        }

        private static void CentralPointPropertyChangedCallback(DependencyObject Obj,
                                                                DependencyPropertyChangedEventArgs e)
        {
            var newPoint = (EarthPoint)e.NewValue;
            var map = (MapView)Obj;
            map.OnCentralPointChanged(newPoint);
        }

        #endregion

        #region MostWesternLongitude DependencyProperty

        public static readonly DependencyPropertyKey MostWesternLongitudePropertyKey = DependencyProperty
            .RegisterReadOnly("MostWesternLongitude", typeof (Degree), typeof (MapView),
                              new PropertyMetadata(default(Degree)));

        public static readonly DependencyProperty MostWesternLongitudeProperty =
            MostWesternLongitudePropertyKey.DependencyProperty;

        public Degree MostWesternLongitude
        {
            get { return (Degree)GetValue(MostWesternLongitudeProperty); }
            protected set { SetValue(MostWesternLongitudePropertyKey, value); }
        }

        #endregion

        #region MostEasternLongitude DependencyProperty

        public static readonly DependencyPropertyKey MostEasternLongitudePropertyKey = DependencyProperty
            .RegisterReadOnly("MostEasternLongitude", typeof (Degree), typeof (MapView),
                              new PropertyMetadata(default(Degree)));

        public static readonly DependencyProperty MostEasternLongitudeProperty =
            MostEasternLongitudePropertyKey.DependencyProperty;

        public Degree MostEasternLongitude
        {
            get { return (Degree)GetValue(MostEasternLongitudeProperty); }
            protected set { SetValue(MostEasternLongitudePropertyKey, value); }
        }

        #endregion

        #region MostNorthenLatitude DependencyProperty

        public static readonly DependencyPropertyKey MostNorthenLatitudePropertyKey = DependencyProperty
            .RegisterReadOnly("MostNorthenLatitude", typeof (Degree), typeof (MapView),
                              new PropertyMetadata(default(Degree)));

        public static readonly DependencyProperty MostNorthenLatitudeProperty =
            MostNorthenLatitudePropertyKey.DependencyProperty;

        public Degree MostNorthenLatitude
        {
            get { return (Degree)GetValue(MostNorthenLatitudeProperty); }
            protected set { SetValue(MostNorthenLatitudePropertyKey, value); }
        }

        #endregion

        #region MostSouthernLatitude DependencyProperty

        public static readonly DependencyPropertyKey MostSouthernLatitudePropertyKey = DependencyProperty
            .RegisterReadOnly("MostSouthernLatitude", typeof (Degree), typeof (MapView),
                              new PropertyMetadata(default(Degree)));

        public static readonly DependencyProperty MostSouthernLatitudeProperty =
            MostSouthernLatitudePropertyKey.DependencyProperty;

        public Degree MostSouthernLatitude
        {
            get { return (Degree)GetValue(MostSouthernLatitudeProperty); }
            protected set { SetValue(MostSouthernLatitudePropertyKey, value); }
        }

        #endregion

        private readonly TranslateTransform _globalTransform;
        private Point? _dragStartPoint;
        public int ZoomLevel { get; set; }

        private void OnCentralPointChanged(EarthPoint newCentralPoint)
        {
            Point screenCentralPoint = Projector.Project(newCentralPoint, ZoomLevel);
            _globalTransform.X = Math.Round(-screenCentralPoint.X + ActualWidth / 2);
            _globalTransform.Y = Math.Round(-screenCentralPoint.Y + ActualHeight / 2);

            EarthPoint topLeft =
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, -ActualHeight / 2), ZoomLevel);
            EarthPoint bottomLeft =
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, +ActualHeight / 2), ZoomLevel);
            EarthPoint topRight =
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, -ActualHeight / 2), ZoomLevel);
            EarthPoint bottomRight =
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, +ActualHeight / 2), ZoomLevel);

            MostWesternLongitude = new Degree(Math.Min(topLeft.Longitude.Value, bottomLeft.Longitude.Value));
            MostEasternLongitude = new Degree(Math.Max(topRight.Longitude.Value, bottomRight.Longitude.Value));
            MostNorthenLatitude = new Degree(Math.Max(topLeft.Latitude.Value, topRight.Latitude.Value));
            MostSouthernLatitude = new Degree(Math.Min(bottomLeft.Latitude.Value, bottomRight.Latitude.Value));
        }

        public void Move(double dx, double dy) { Move(new Vector(dx, dy)); }

        public void Move(Vector delta)
        {
            Point p0 = ScreenProjector.DefaultProjector.Project(CentralPoint, ZoomLevel);
            Point p = p0 - delta;
            CentralPoint = ScreenProjector.DefaultProjector.InverseProject(p, ZoomLevel);

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
