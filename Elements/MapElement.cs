using System;
using System.Linq;
using System.Windows.Media;
using Geographics;

namespace MapVisualization.Elements
{
    /// <summary>Элемент карты</summary>
    public abstract class MapElement
    {
        protected static GuidelineSet ScreenGuidelineSet;

        static MapElement()
        {
            ScreenGuidelineSet = new GuidelineSet(
                Enumerable.Range(0, 40000).Select(x => (double)x).ToArray(),
                Enumerable.Range(0, 40000).Select(y => (double)y).ToArray());
        }

        /// <summary>Z-индекс элемента на карте</summary>
        /// <remarks>Меньшее значения индекса соответствуют нижним слоям на карте</remarks>
        protected virtual int ZIndex
        {
            get { return 0; }
        }

        /// <summary>Проектор географических координат в экранные</summary>
        protected ScreenProjector Projector
        {
            get { return ScreenProjector.DefaultProjector; }
        }

        /// <summary>Визуальный элемент, изображающий данный элемент карты</summary>
        internal MapVisual AttachedVisual { get; set; }

        /// <summary>Событие, сигнализирующее о том, что элемент запросил изменение своего визуального отображения</summary>
        internal event EventHandler ChangeVisualRequested;

        /// <summary>Отправляет запрос на смену своего визуального отображения</summary>
        protected void RequestChangeVisual()
        {
            EventHandler handler = ChangeVisualRequested;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        /// <summary>Отрисовывает объект в указанном контексте рисования</summary>
        /// <param name="dc">Контекст рисования</param>
        /// <param name="Zoom">Индекс масштаба рисования</param>
        protected abstract void Draw(DrawingContext dc, int Zoom);

        /// <summary>Получает визуальный элемент для этого элемента карты</summary>
        /// <param name="Zoom">Индекс масштаба отображения</param>
        /// <returns></returns>
        public MapVisual GetVisual(int Zoom)
        {
            var res = new MapVisual(ZIndex);
            using (DrawingContext dc = res.RenderOpen())
            {
                Draw(dc, Zoom);
            }
            return res;
        }

        /// <summary>Проверяет, попадает ли этот элемент в указанную области видимости</summary>
        /// <param name="VisibleArea">Область видимости</param>
        /// <returns>True, если объект может оказаться виден в указанной области</returns>
        public abstract bool TestVisual(EarthArea VisibleArea);
    }
}
