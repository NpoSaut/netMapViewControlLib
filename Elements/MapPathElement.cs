using System.Collections.Generic;
using System.Linq;
using Geographics;

namespace MapVisualization.Elements
{
    /// <summary>
    /// Элемент карты, состоящий из нескольких точек
    /// </summary>
    public abstract class MapPathElement : MapElement
    {
        protected virtual EarthArea ElementArea { get; private set; }

        /// <summary>
        /// Создаёт новый многоточечный объект на карте
        /// </summary>
        /// <param name="Points">Точки, входящие в состав объекта</param>
        public MapPathElement(IList<EarthPoint> Points)
        {
            this.Points = Points;
            ElementArea = new EarthArea(Points.ToArray());
        }
        /// <summary>
        /// Точки, входящие в состав объекта
        /// </summary>
        public IList<EarthPoint> Points { get; private set; }

        /// <summary>Проверяет, попадает ли этот элемент в указанную области видимости</summary>
        /// <param name="VisibleArea">Область видимости</param>
        /// <returns>True, если объект может оказаться виден в указанной области</returns>
        public override bool TestVisual(EarthArea VisibleArea) { return ElementArea.IsIntersects(VisibleArea); }
    }
}