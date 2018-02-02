using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MapVisualization.TileLoaders;

namespace MapVisualization.Elements
{
    public class ContextMapTileElement : MapTileElement
    {
        private readonly ITileLoadingContext _loadingContext;
        private BitmapImage _image;

        public ContextMapTileElement(ITileLoadingContext LoadingContext, int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex)
        {
            _loadingContext       =  LoadingContext;
            _loadingContext.Ready += (s, e) => OnReady();
            //_loadingContext.Ready += (s, e) => Dispatcher.CurrentDispatcher.BeginInvoke((Action)OnReady);
        }

        private void OnReady()
        {
            _image = new BitmapImage(_loadingContext.ImageUri);
            RequestChangeVisual();
        }

        public override void Dispose()
        {
            _loadingContext.Abort();
        }

        protected override void DrawTile(DrawingContext dc, Rect TileRect)
        {
            if (!_loadingContext.IsReady)
                dc.DrawRectangle(Brushes.LemonChiffon, null, TileRect);
            else
                dc.DrawImage(_image, TileRect);
        }
    }
}
