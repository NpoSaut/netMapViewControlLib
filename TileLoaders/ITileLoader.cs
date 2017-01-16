using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapVisualization.TileLoaders
{
    /// <summary>��������� ���������� ������ �����</summary>
    public interface ITileLoader
    {
        /// <summary>��������� ���� � ���������� ���������</summary>
        /// <param name="x">�������������� ������</param>
        /// <param name="y">������������ ������</param>
        /// <param name="zoom">������� ���������������</param>
        /// <returns>ImageSource �����</returns>
        BitmapImage GetTile(int x, int y, int zoom);
    }
}
