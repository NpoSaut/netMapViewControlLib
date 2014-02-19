using System;
using System.IO;
using System.Reflection;

namespace MapVisualization.TileLoaders
{
    /// <summary>Загрузчик тайлов с кэшем в папке AppData</summary>
    public class AppDataFileCacheTileLoader : FileCacheTileLoader
    {
        public static AppDataFileCacheTileLoader DefaultLoader { get; private set; }

        static AppDataFileCacheTileLoader()
        {
            DefaultLoader = new AppDataFileCacheTileLoader();
        }

        public AppDataFileCacheTileLoader()
            : base(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "Saut", Assembly.GetExecutingAssembly().GetName().Name,
                                "Tiles Cache")) { }
    }
}