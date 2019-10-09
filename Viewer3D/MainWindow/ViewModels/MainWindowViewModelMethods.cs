using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using EvolutionOptimization.Interfaces;
using EvolutionOptimization.Managers;
using MaterialDesignThemes.Wpf;
using Viewer3D.Controls.Configuration.ViewModels;
using Viewer3D.Controls.Configuration.Views;
using Viewer3D.Helpers;

namespace Viewer3D.MainWindow.ViewModels
{
    public partial class MainWindowViewModel
    {
        #region Commands

        public async void OnSettingsClickCommand()
        {
            IsConfigurationWindowOpen = true;
        }

        private void OnCloseDialogHostCommand(DialogClosingEventArgs eventArgs)
        {
            if (eventArgs?.Parameter.ToString() == false.ToString()) return;
            _config = ((eventArgs?.Session?.Content as ConfigurationView)?.DataContext as ConfigurationViewModel)?.Config;
        }

        public async void OnStartClickCommand()
        {
            IsSettingsBtnEnabled = false;
            IsStartBtnEnabled = false;
            await StartSelectionProcess(TargetPoint);
        }

        public void OnCloseWindowClickCommand()
        {
            _cts.Cancel();
        }

        #endregion
        private async Task<IIndividual> StartSelectionProcess(Point3D target)
        {
            var manager = new SolverManager(new[] { target.X, target.Y, target.Z }, _config) { UpdateAction = UpdateView };

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
