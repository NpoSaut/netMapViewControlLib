﻿namespace MapVisualization.TileLoaders.TilePathProvider
{
    public static class OsmTilePathProviders
    {
        // The good catalog with broviders is here: http://leaflet-extras.github.io/leaflet-providers/preview/index.html

        public static ITilePathProvider Default
        {
            get { return new TilePathProvider("Tiles Cache", "http://a.tile.openstreetmap.org/{zoom}/{x}/{y}.png"); }
        }

        public static ITilePathProvider Retina
        {
            get { return new TilePathProvider("Retina Tiles Cache", "https://c.osm.rrze.fau.de/osmhd/{zoom}/{x}/{y}.png"); }
        }

        public static ITilePathProvider LyrkRetina
        {
            get { return new TilePathProvider("Lirk Retina Tiles Cache", "https://tiles.lyrk.org/lr/{zoom}/{x}/{y}"); }
        }

        public static ITilePathProvider DarkMatter
        {
            get { return new TilePathProvider("CartoDB Datk Matter", "https://b.basemaps.cartocdn.com/dark_all/{zoom}/{x}/{y}{r}.png"); }
        }

        public static ITilePathProvider Voyager
        {
            get { return new TilePathProvider("CartoDB Vojager", "https://b.basemaps.cartocdn.com/rastertiles/voyager/{zoom}/{x}/{y}{r}.png"); }
        }
    }

    public static class YandexTilePathProviders
    {
        public static ITilePathProvider Satellite
        {
            get { return new TilePathProvider("Yandex Satellite Tiles Cache", "https://sat01.maps.yandex.net/tiles?l=sat&v=1.35.0&x={x}&y={y}&z={zoom}"); }
        }
    }
}
