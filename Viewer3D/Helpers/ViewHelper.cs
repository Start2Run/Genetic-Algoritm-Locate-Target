using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace Viewer3D.Helpers
{
    public class ViewHelper
    {
        public static SphereVisual3D AddPoint(Point3D point, Color color)
        {
            var targetPoint = new SphereVisual3D();
            targetPoint.BeginEdit();
            targetPoint.Center = point;
            targetPoint.Radius = 0.2;
            targetPoint.Fill = new SolidColorBrush(color);
            targetPoint.EndEdit();
            return targetPoint;
        }

        public static LinesVisual3D AddLine()
        {
            var line = new LinesVisual3D();
            line.Color = Colors.Black;
            line.Thickness = 1;
            line.Points.Add(new Point3D(0, 0, 0));
            line.Points.Add(new Point3D(-5, -5, -5));
            return line; 
        }
    }
}
