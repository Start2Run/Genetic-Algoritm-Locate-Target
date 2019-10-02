using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using EvolutionOptimization.Helpers;
using EvolutionOptimization.Managers;
using HelixToolkit.Wpf;
using Viewer3D.Helpers;

namespace Viewer3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Point3D _startPoint = new Point3D(0, 0, 0);
        private readonly Point3D _targetPoint = new Point3D(-5, 4.6, 2.4);

        private ConcurrentDictionary<Guid, LinesVisual3D> Lines;

        public MainWindow()
        {
            InitializeComponent();
            InitView();
            StartSelectionProcess();
        }

        private void InitView()
        {
            var start = ViewHelper.AddPoint(_startPoint, Colors.Gray);
            var target = ViewHelper.AddPoint(_targetPoint, Colors.Red);
            MainView.Children.Add(start);
            MainView.Children.Add(target);

            Lines = new ConcurrentDictionary<Guid, LinesVisual3D>();
        }

        private async void StartSelectionProcess()
        {
            var evolv = new SolverManager(new[] { _targetPoint.X, _targetPoint.Y, _targetPoint.Z });
            var bestSolution = await evolv.Solver();
            var message = "Accuracy value at best solution = " + bestSolution.Error.ToString("F6") + " % ";
        }
    }
}
