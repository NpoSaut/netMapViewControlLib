using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Channels;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Geographics;
using MapVisualization.Elements;

namespace MapVisualization
{
    public class MapView : DrawingContainer
    {
        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapView), new FrameworkPropertyMetadata(typeof(MapView)));
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

            var tiles = Enumerable.Range(x0, 4).SelectMany(x => Enumerable.Range(y0, 4).Select(y => GetMapTile(ZoomLevel, x, y)));
            foreach (var tile in tiles)
            {
                AddElement(tile);
            }
        }

        private MapTileElement GetMapTile(int Scale, int x, int y)
        {
            var u = OsmIndexes.GetTileUri(x, y, Scale);
            MemoryStream bitmapStream;
            using (var wc = new WebClient())
            {
                bitmapStream = new MemoryStream(wc.DownloadData(u));
            }
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = bitmapStream;
            source.EndInit();
            return new MapTileElement(source, x, y, Scale);
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
