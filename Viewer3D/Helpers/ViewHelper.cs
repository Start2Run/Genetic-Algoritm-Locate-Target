using System.Windows.Media;
using System.Windows.Media.Media3D;
using EvolutionOptimization.Interfaces;
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
            targetPoint.Radius = 0.1;
            targetPoint.Fill = new SolidColorBrush(color);
            targetPoint.EndEdit();
            return targetPoint;
        }

        public static LinesVisual3D AddLine(IIndividual item, Point3D startPoint, Point3D targetPoint, Color color)
        {
            var line = new LinesVisual3D { Color = color, Thickness = 1 };

            Point3D lastPoint= startPoint;
            //line.Points.Add(lastPoint);
            foreach (var chromosome in item.Chromosome)
            {
                double x = 0;
                double y = 0;
                double z = 0;
                line.Points.Add(lastPoint);
                if (chromosome.Genes.Length > 0)
                {
                    x = chromosome.Genes[0]+lastPoint.X;
                }

                if (chromosome.Genes.Length > 1)
                {
                    y = chromosome.Genes[1] + lastPoint.Y;
                }

                if (chromosome.Genes.Length > 1)
                {
                    z = chromosome.Genes[2] + lastPoint.Z;
                }
                lastPoint = new Point3D(x, y, z);
                line.Points.Add(lastPoint);
            }

            return line;
        }
    }
}
