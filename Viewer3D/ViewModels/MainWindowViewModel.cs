using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Media;
using GalaSoft.MvvmLight.Command;
using HelixToolkit.Wpf;
using Viewer3D.Helpers;

namespace Viewer3D.ViewModels
{
    public partial class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
           
            SettingsClickCommand = new RelayCommand(OnSettingsClickCommand);
            StartClickCommand = new RelayCommand(OnStartClickCommand);
            CloseWindowClickCommand = new RelayCommand(OnCloseWindowClickCommand);
            _lines = new ConcurrentDictionary<Guid, TubeVisual3D>();
            _cts = new CancellationTokenSource();
        }

        public void InitViewer(HelixViewport3D view)
        {
            _dispatcher = view.Dispatcher;
            _view = view;
            var startText = new BillboardTextVisual3D {Text = "Start", Foreground = Brushes.Gray, Position = StartPoint, FontSize = 24, Background = Brushes.Transparent };
            var targetText = new BillboardTextVisual3D { Text = "Target",Foreground = Brushes.Red, Position = TargetPoint, FontSize = 24, Background = Brushes.Transparent};
            _view.Children.Add(startText);
            _view.Children.Add(targetText);
        }
    }
}
