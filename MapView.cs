using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MapVisualization.Elements;

namespace MapVisualization
{
    public class MapView : DrawingContainer
    {
        static MapView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MapView), new FrameworkPropertyMetadata(typeof(MapView)));
        }

        public MapView()
        {
            var el = GetMapTile(13, 5472, 2514);
            AddElement(el);
        }

        private MapTileElement GetMapTile(int Scale, int x, int y)
        {
            var u = new Uri(String.Format("http://a.tile.openstreetmap.org/{0}/{1}/{2}.png", Scale, x, y));
            MemoryStream bitmapStream;
            using (var wc = new WebClient())
            {
                bitmapStream = new MemoryStream(wc.DownloadData(u));
            }
            var source = new BitmapImage();
            source.BeginInit();
            source.StreamSource = bitmapStream;
            source.EndInit();
            return new MapTileElement(source);
        }

        private readonly List<MapElement> _elements = new List<MapElement>();

        private void AddElement(MapElement Element)
        {
            _elements.Add(Element);
            var visual = Element.GetVisual();
            visual.Transform = _globalTransform;
            AddVisual(visual);
        }

        private TranslateTransform _globalTransform = new TranslateTransform();

        public void Move(double dx, double dy)
        {
            _globalTransform.X += dx;
            _globalTransform.Y += dy;
        }
    }
}
