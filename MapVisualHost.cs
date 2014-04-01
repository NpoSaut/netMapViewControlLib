using System.Windows.Controls;
using System.Windows.Media;

namespace MapVisualization
{
    /// <summary>Панель - хост для визуальных элементов</summary>
    public abstract class MapVisualHost : Panel
    {
        /// <summary>Визуальные элементы карты</summary>
        private readonly VisualCollection _visuals;

        protected MapVisualHost() { _visuals = new VisualCollection(this); }

        protected override int VisualChildrenCount
        {
            get { return _visuals.Count; }
        }

        protected override Visual GetVisualChild(int index) { return _visuals[index]; }

        /// <summary>Добавляет визуальный элемент на карту</summary>
        /// <param name="v">Визуальный элемент</param>
        protected void AddVisual(MapVisual v)
        {
            int index;
            for (index = 0; index < _visuals.Count; index++)
                if (((MapVisual)_visuals[index]).ZIndex > v.ZIndex) break;

            _visuals.Insert(index, v);
        }

        /// <summary>Удаляет визуальный элемент с карты</summary>
        /// <param name="v"></param>
        protected void DeleteVisual(MapVisual v) { _visuals.Remove(v); }
    }
}
