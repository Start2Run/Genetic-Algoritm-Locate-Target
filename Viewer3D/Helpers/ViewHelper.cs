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

        public static TubeVisual3D AddLine(IIndividual item, Point3D startPoint, Point3D targetPoint, Color color)
        {
            var points = new Point3DCollection();
            var lastPoint = startPoint;
            foreach (var chromosome in item.Chromosome)
            {
                double x = 0;
                double y = 0;
                double z = 0;
                points.Add(lastPoint);
                if (chromosome.Genes.Length > 0)
                {
                    x = chromosome.Genes[0] + lastPoint.X;
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
                points.Add(lastPoint);
            }

            return new TubeVisual3D
            {
                Material = MaterialHelper.CreateMaterial(null, new SolidColorBrush(color), Brushes.White, 0.5, 40),
                Diameter = 0.05, Path = points
            };
        }
    }
}
