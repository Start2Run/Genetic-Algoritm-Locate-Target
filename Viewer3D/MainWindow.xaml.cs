using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EvolutionOptimization.Interfaces;
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
        private readonly Point3D _targetPoint = new Point3D(12, 10, -5);

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
            var manager = new SolverManager(new[] { _targetPoint.X, _targetPoint.Y, _targetPoint.Z });
            manager.UpdateAction = UpdateView;
            var bestSolution = await manager.Solver();
            Console.WriteLine("Accuracy value at best solution = " + bestSolution.Error.ToString("F6") + " % ");
        }

        private void UpdateView(IEnumerable<IIndividual> collection)
        {
            var c = collection.ToArray();
            MainView?.Dispatcher?.Invoke(() =>
            {
                var colors = new[] { Colors.Black, Colors.Green, Colors.Aqua, Colors.Blue, Colors.BlueViolet };

                foreach (var item in Lines)
                {
                    MainView.Children.Remove(item.Value);
                }

                Lines.Clear();
                for (var i = 0; i < 5; i++)
                {
                    var item = c[i];
                    var line = ViewHelper.AddLine(item, _startPoint, _targetPoint, colors[i]);
                    Lines.TryAdd(item.Id, line);
                }

                foreach (var item in Lines)
                {
                    MainView.Children.Add(item.Value);
                }
            });
        }
    }
}
