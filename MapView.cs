﻿using System;
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

            var delta = new Vector(sizeInfo.NewSize.Width - sizeInfo.PreviousSize.Width,
                                   sizeInfo.NewSize.Height - sizeInfo.PreviousSize.Height);
            Point oldScreenCentralPoint = Projector.Project(CentralPoint, ZoomLevel);
            Point newScreenCentralPoint = oldScreenCentralPoint + 0.5 * delta;

            CentralPoint = Projector.InverseProject(newScreenCentralPoint, ZoomLevel);
        }

        private async void RefreshTiles()
        {
            int x0 = OsmIndexes.GetHorizontalIndex(VisibleArea.MostWesternLongitude, ZoomLevel);
            int y0 = OsmIndexes.GetVerticalIndex(VisibleArea.MostNorthenLatitude, ZoomLevel);

            int w = (int)Math.Ceiling(ActualWidth / 256) + 1;
            int h = (int)Math.Ceiling(ActualHeight / 256) + 1;

            for (int x = x0; x < x0 + w; x++)
                for (int y = y0; y < y0 + h; y++)
                {
                    if (!_tiles.Any(t => t.HorizontalIndex == x && t.VerticalIndex == y))
                    {
                        var tempTile = new MapStrubTileElement(x, y, ZoomLevel);
                        Dispatcher.BeginInvoke((Action<MapTileElement>)AddTile, tempTile);

                        try
                        {
                            ImageSource tileImage = await TileLoader.GetTileAsync(x, y, ZoomLevel);
                            var tile = new MapImageTileElement(tileImage, x, y, ZoomLevel);
                            Dispatcher.BeginInvoke((Action<MapTileElement>)RemoveTile, tempTile);
                            Dispatcher.BeginInvoke((Action<MapTileElement>)AddTile, tile);
                        }
                        catch (Exception) { }
                    }
                }
        }

        private void RefreshObjectsVisuals()
        {
            foreach (var elementToVisual in _elementsToVisuals.ToList())
            {
                if (!elementToVisual.Key.TestVisual(VisibleArea))
                {
                    DeleteVisual(elementToVisual.Value);
                    _elementsToVisuals.Remove(elementToVisual.Key);
                }
            }

            EarthArea vArea = VisibleArea;
            foreach (
                MapElement element in
                    _elements.Except(_elementsToVisuals.Keys)
                             .AsParallel()
                             .Where(e => !_elementsToVisuals.ContainsKey(e) && e.TestVisual(vArea))
                             .ToList())
            {
                MapVisual visual = element.GetVisual(ZoomLevel);
                visual.Transform = _globalTransform;
                AddVisual(visual);
                _elementsToVisuals.Add(element, visual);
            }
        }

        /// <summary>Получает координаты точки, соответствующей точке с заданными экранными координатами</summary>
        /// <param name="screenPoint">Координаты точки относительно элемента управления карты</param>
        /// <returns>Координаты точки на поверхности Земли, соответствующие точке на карте</returns>
        public EarthPoint PointAt(Point screenPoint)
        {
            Point globalScreenCenter = Projector.Project(CentralPoint, ZoomLevel);
            Point globalScreenPoint = globalScreenCenter + (Vector)screenPoint
                                      - new Vector(ActualWidth / 2, ActualHeight / 2);
            return Projector.InverseProject(globalScreenPoint, ZoomLevel);
        }

        #region Работа со списком элементов

        private readonly List<MapElement> _elements = new List<MapElement>();

        private readonly Dictionary<MapElement, MapVisual> _elementsToVisuals = new Dictionary<MapElement, MapVisual>();
        private readonly List<MapTileElement> _tiles = new List<MapTileElement>();

        private readonly Dictionary<MapTileElement, MapVisual> _tilesToVisuals =
            new Dictionary<MapTileElement, MapVisual>();

        public void AddTile(MapTileElement Tile)
        {
            _tiles.Add(Tile);
            MapVisual visual = Tile.GetVisual(ZoomLevel);
            visual.Transform = _globalTransform;
            AddVisual(visual);
            _tilesToVisuals.Add(Tile, visual);
        }

        private void RemoveTile(MapTileElement Tile)
        {
            _tiles.Remove(Tile);
            if (_tilesToVisuals.ContainsKey(Tile))
            {
                DeleteVisual(_tilesToVisuals[Tile]);
                _tilesToVisuals.Remove(Tile);
            }
        }

        public void AddElement(MapElement Element)
        {
            _elements.Add(Element);
        }

        public void RemoveElement(MapElement Element)
        {
            _elements.Remove(Element);
            if (_elementsToVisuals.ContainsKey(Element))
                DeleteVisual(_elementsToVisuals[Element]);
        }

        #endregion

        #region События кликов мыши

        /// <summary>Нажатие кнопки мыши над картой</summary>
        public event EventHandler<GeographicEventArgs> GeographicMouseDown;

        /// <summary>Отпускание кнопки мыши над картой</summary>
        public event EventHandler<GeographicEventArgs> GeographicMouseUp;

        /// <summary>Щелчок мышью над картой</summary>
        public event EventHandler<GeographicEventArgs> GeographicMouseClick;

        protected virtual void OnGeographicMouseDown(GeographicEventArgs E)
        {
            EventHandler<GeographicEventArgs> handler = GeographicMouseDown;
            if (handler != null) handler(this, E);
        }

        protected virtual void OnGeographicMouseUp(GeographicEventArgs E)
        {
            EventHandler<GeographicEventArgs> handler = GeographicMouseUp;
            if (handler != null) handler(this, E);
        }

        protected virtual void OnGeographicMouseClick(GeographicEventArgs E)
        {
            EventHandler<GeographicEventArgs> handler = GeographicMouseClick;
            if (handler != null) handler(this, E);
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

        #region VisibleArea DependencyProperty

        public static readonly DependencyPropertyKey VisibleAreaPropertyKey = DependencyProperty
            .RegisterReadOnly("VisibleArea", typeof (EarthArea), typeof (MapView),
                              new PropertyMetadata(default(EarthArea), VisibleAreaPropertyChangedCallback));

        public static readonly DependencyProperty VisibleAreaProperty =
            VisibleAreaPropertyKey.DependencyProperty;

        public EarthArea VisibleArea
        {
            get { return (EarthArea)GetValue(VisibleAreaProperty); }
            protected set { SetValue(VisibleAreaPropertyKey, value); }
        }

        private static void VisibleAreaPropertyChangedCallback(DependencyObject Obj,
                                                               DependencyPropertyChangedEventArgs e)
        {
            var map = (MapView)Obj;
            var newVisibleArea = (EarthArea)e.NewValue;
            map.OnVisibleAreaChanged(newVisibleArea);
        }

        #endregion

        private readonly TranslateTransform _globalTransform;
        private Point? _dragStartPoint;
        private bool _isMapWasMovedLastTime;
        public int ZoomLevel { get; set; }

        protected virtual void OnCentralPointChanged(EarthPoint newCentralPoint)
        {
            Point screenCentralPoint = Projector.Project(newCentralPoint, ZoomLevel);
            _globalTransform.X = Math.Round(-screenCentralPoint.X + ActualWidth / 2);
            _globalTransform.Y = Math.Round(-screenCentralPoint.Y + ActualHeight / 2);

            VisibleArea = new EarthArea(
                // Top Left
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, -ActualHeight / 2), ZoomLevel),
                // Bottom Left
                Projector.InverseProject(screenCentralPoint + new Vector(-ActualWidth / 2, +ActualHeight / 2), ZoomLevel),
                // Top Right
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, -ActualHeight / 2), ZoomLevel),
                // BottomRight
                Projector.InverseProject(screenCentralPoint + new Vector(+ActualWidth / 2, +ActualHeight / 2), ZoomLevel)
                );

            RefreshTiles();
        }

        protected virtual void OnVisibleAreaChanged(EarthArea NewVisibleArea) { RefreshObjectsVisuals(); }

        public void Move(Vector delta)
        {
            Point p0 = ScreenProjector.DefaultProjector.Project(CentralPoint, ZoomLevel);
            Point p = p0 - delta;
            CentralPoint = ScreenProjector.DefaultProjector.InverseProject(p, ZoomLevel);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            _dragStartPoint = e.GetPosition(this);
            _isMapWasMovedLastTime = false;
            base.OnMouseDown(e);

            OnGeographicMouseDown(new GeographicEventArgs(PointAt(_dragStartPoint.Value)));
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            EarthPoint upPoint = PointAt(e.GetPosition(this));
            OnGeographicMouseDown(new GeographicEventArgs(upPoint));
            if (!_isMapWasMovedLastTime) OnGeographicMouseClick(new GeographicEventArgs(upPoint));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dragCurrentPoint = e.GetPosition(this);
                if (_dragStartPoint != null) Move(dragCurrentPoint - _dragStartPoint.Value);
                _dragStartPoint = dragCurrentPoint;
                _isMapWasMovedLastTime = true;
            }
            base.OnMouseMove(e);
        }

        #endregion
    }

    public class GeographicEventArgs : EventArgs
    {
        public GeographicEventArgs(EarthPoint Point) { this.Point = Point; }
        public EarthPoint Point { get; private set; }
    }
}
