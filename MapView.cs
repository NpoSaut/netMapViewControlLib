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
    public class MapView : MapVisualHost
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
            ZoomLevel = 14;

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
            EarthArea vArea = VisibleArea;
            var elementsVisibility =
                _elements.AsParallel().Select(e => new { element = e, visibility = e.TestVisual(vArea) }).ToList();
            foreach (var ev in elementsVisibility)
            {
                CheckVisual(ev.element, ev.visibility);
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
            CheckVisual(Element);
        }

        public void RemoveElement(MapElement Element)
        {
            CheckVisual(Element, false);
            _elements.Remove(Element);
        }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        private void CheckVisual(MapElement Element)
        {
            CheckVisual(Element, VisibleArea);
        }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        /// <param name="OnArea">Видимая в область карты</param>
        private void CheckVisual(MapElement Element, EarthArea OnArea)
        {
            CheckVisual(Element, Element.TestVisual(OnArea));
        }

        /// <summary>Проверяет, и при необходимости отрисовывает или скрывает объект с карты</summary>
        /// <param name="Element">Проверяемый объект</param>
        /// <param name="IsElementVisible">Видим ли объект на карте в данный момент</param>
        private void CheckVisual(MapElement Element, bool IsElementVisible)
        {
            if (IsElementVisible)
            {
                if (Element.AttachedVisual == null) VisualizeElement(Element);
            }
            else
            {
                if (Element.AttachedVisual != null) HideElement(Element);
            }
        }

        /// <summary>Выводит визуальное представление элемента на карту</summary>
        /// <param name="Element">Элемент для визуализации</param>
        private void VisualizeElement(MapElement Element)
        {
            Element.AttachedVisual = Element.GetVisual(ZoomLevel);
            Element.AttachedVisual.Transform = _globalTransform;
            Element.ChangeVisualRequested += OnMapElementChangeVisualRequested;
            AddVisual(Element.AttachedVisual);
        }

        /// <summary>Скрывает визуальное представление с карты</summary>
        /// <param name="Element">Элемент для сокрытия</param>
        private void HideElement(MapElement Element)
        {
            DeleteVisual(Element.AttachedVisual);
            Element.ChangeVisualRequested -= OnMapElementChangeVisualRequested;
            Element.AttachedVisual = null;
        }

        /// <summary>Выполняет действия по перерисовке визуального отображения элемента карты</summary>
        private void OnMapElementChangeVisualRequested(object Sender, EventArgs Args)
        {
            var element = (MapElement)Sender;
            HideElement(element);
            VisualizeElement(element);
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
        private double _isMapWasMovedDisstance;
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
            _isMapWasMovedDisstance = 0;
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point dragCurrentPoint = e.GetPosition(this);
                _isMapWasMovedDisstance += (dragCurrentPoint - _dragStartPoint).Value.Length;
                if (_dragStartPoint != null) Move(dragCurrentPoint - _dragStartPoint.Value);
                _dragStartPoint = dragCurrentPoint;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (_isMapWasMovedDisstance < 10)
            {
                var act = MouseAction.None;
                if (e.ChangedButton == MouseButton.Left)
                    act = e.ClickCount == 1 ? MouseAction.LeftClick : MouseAction.LeftDoubleClick;
                else if (e.ChangedButton == MouseButton.Right)
                    act = e.ClickCount == 1 ? MouseAction.RightClick : MouseAction.RightDoubleClick;
                else if (e.ChangedButton == MouseButton.Middle)
                    act = MouseAction.WheelClick;

                if (act != MouseAction.None) OnClick(act, e.GetPosition(this));
            }

            base.OnMouseUp(e);
        }

        #region Работа с мышью

        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof (ICommand), typeof (MapView),
                                        new PropertyMetadata(default(ICommand)));

        public static readonly DependencyProperty ClickCommandParameterProperty =
            DependencyProperty.Register("ClickCommandParameter", typeof (Object), typeof (MapView),
                                        new PropertyMetadata(default(Object)));

        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        public Object ClickCommandParameter
        {
            get { return GetValue(ClickCommandParameterProperty); }
            set { SetValue(ClickCommandParameterProperty, value); }
        }

        public event EventHandler<MapMouseActionEventArgs> Click;

        private void OnClick(MouseAction ActionKind, Point ScreenPoint)
        {
            EventHandler<MapMouseActionEventArgs> handler = Click;
            if (handler != null) handler(this, new MapMouseActionEventArgs(PointAt(ScreenPoint), ActionKind));

            if (ClickCommand != null)
                ClickCommand.Execute(ClickCommandParameter);
        }

        #endregion

        #endregion
    }

    /// <summary>Представляет данные для событий, связанных с нажатием мышью на карте</summary>
    public class MapMouseActionEventArgs : EventArgs
    {
        public MapMouseActionEventArgs(EarthPoint Point, MouseAction Action)
        {
            this.Action = Action;
            this.Point = Point;
        }

        /// <summary>Вид нажатия</summary>
        public MouseAction Action { get; private set; }

        /// <summary>Точка на карте, в которой совершено нажатие</summary>
        public EarthPoint Point { get; private set; }
    }
}
