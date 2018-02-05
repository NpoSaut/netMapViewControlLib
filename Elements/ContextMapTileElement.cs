using System;
using System.Windows;
using System.Windows.Media;
using MapVisualization.TileLoaders;

namespace MapVisualization.Elements
{
    public class ContextMapTileElement : MapTileElement
    {
        private readonly ITileLoadingContext _loadingContext;

        public ContextMapTileElement(ITileLoadingContext LoadingContext, int HorizontalIndex, int VerticalIndex, int Zoom)
            : base(HorizontalIndex, VerticalIndex)
        {
            _loadingContext       =  LoadingContext;
            _loadingContext.Ready += OnReady;
        }

        private void OnReady(object Sender, EventArgs EventArgs)
        {
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
                dc.DrawImage(_loadingContext.Image, TileRect);
        }
    }
}
