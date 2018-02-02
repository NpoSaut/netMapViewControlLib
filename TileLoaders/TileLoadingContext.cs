using System;
using System.Windows.Media.Imaging;

namespace MapVisualization.TileLoaders
{
    public interface ITileLoadingContext
    {
        bool IsReady { get; }
        Uri ImageUri { get; }
        event EventHandler Ready;
        void Abort();
    }
}
