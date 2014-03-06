using System.Windows.Media;

namespace MapVisualization
{
    public class MapVisual : DrawingVisual
    {
        public MapVisual(int ZIndex = 0) { this.ZIndex = ZIndex; }

        /// <summary>
        /// Z-������ ����������� ��������
        /// </summary>
        public int ZIndex { get; private set; }
    }
}
