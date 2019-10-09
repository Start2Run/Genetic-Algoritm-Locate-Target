using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Managers;
using Viewer3D.Helpers;

namespace Viewer3D.ViewModels
{
    public partial class MainWindowViewModel
    {
        #region Commands

        public async void OnSettingsClickCommand()
        {
            var view = new Controls.Views.ConfigurationView() { DataContext = new Controls.Configuration.ViewModels.ConfigurationViewModel() };
            //show the dialog
            var result = await MaterialDesignThemes.Wpf.DialogHost.Show(view, "RootDialog", ExtendedOpenedEventHandler, ExtendedClosingEventHandler);
        }

        private void ExtendedOpenedEventHandler(object sender, MaterialDesignThemes.Wpf.DialogOpenedEventArgs eventargs)
        {
            Console.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");
        }

        private void ExtendedClosingEventHandler(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {
        }

        public async void OnStartClickCommand()
        {
            IsSettingsBtnEnabled = false;
            IsStartBtnEnabled = false;
            var result = await StartSelectionProcess(TargetPoint);
        }

        public void OnCloseWindowClickCommand()
        {
            _cts.Cancel();
        }

        #endregion
        private async Task<IIndividual> StartSelectionProcess(Point3D target)
        {
            var manager = new SolverManager(new[] { target.X, target.Y, target.Z }) { UpdateAction = UpdateView };

            IIndividual bestSolution = null;
            try
            {
                bestSolution = await manager.Solver(_cts.Token);
            }
            catch (Exception e)
            {
                Console.WriteLine($@"{nameof(OperationCanceledException)} thrown with message: {e.Message}");
            }
            finally
            {
                _cts.Dispose();
            }
            //Console.WriteLine("Accuracy value at best solution = " + bestSolution?.Error.ToString("F6") + " % ");
            return bestSolution;
        }

        private void UpdateView(IEnumerable<IIndividual> collection, double error, int generation)
        {
            var c = collection.ToArray();
            _dispatcher?.Invoke(() =>
            {
                try
                {
                    var colors = new[] { Colors.Brown, Colors.Green, Colors.Aqua, Colors.Blue, Colors.BlueViolet };
                    foreach (var item in _lines)
                    {
                        _view.Children.Remove(item.Value);
                    }
                    _lines.Clear();
                    for (var i = 0; i < 5; i++)
                    {
                        var item = c[i];
                        var line = ViewHelper.AddLine(item, StartPoint, TargetPoint, colors[i]);
                        _lines.TryAdd(item.Id, line);
                    }
                    foreach (var item in _lines)
                    {
                        _view.Children.Add(item.Value);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }
    }
}
