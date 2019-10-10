using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using EvolutionOptimization.Models;
using GalaSoft.MvvmLight.Command;
using HelixToolkit.Wpf;
using MaterialDesignThemes.Wpf;
using Viewer3D.Annotations;

namespace Viewer3D.MainWindow.ViewModels
{
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        #region NotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        private Dispatcher _dispatcher;
        private ConcurrentDictionary<Guid, TubeVisual3D> _lines;
        private readonly CancellationTokenSource _cts;
        private HelixViewport3D _view;
        private bool _isSettingsBtnEnabled = true;
        private bool _isStartBtnEnabled = true;
        private bool _isConfigurationWindowOpen;
        private int _generations;
        private double _bestError;

        private Configuration _config;

        public System.Windows.Controls.UserControl DialogHostContent { get; set; }

        public readonly Point3D StartPoint = new Point3D(0, 0, 0);
        public readonly Point3D TargetPoint = new Point3D(3.2, -5.4, -4);

        public bool IsSettingsBtnEnabled
        {
            get => _isSettingsBtnEnabled;
            set
            {
                if (_isSettingsBtnEnabled == value) return;
                _isSettingsBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsStartBtnEnabled
        {
            get => _isStartBtnEnabled;
            set
            {
                if (_isStartBtnEnabled == value) return;
                _isStartBtnEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsConfigurationWindowOpen
        {
            get => _isConfigurationWindowOpen;
            set
            {
                if (_isConfigurationWindowOpen == value) return;
                _isConfigurationWindowOpen = value;
                OnPropertyChanged();
            }
        }

        public int Generations
        {
            get => _generations;
            set
            {
                if (_generations == value) return;
                _generations = value;
                OnPropertyChanged();
            }
        }

        public double BestError
        {
            get => _bestError;
            set
            {
                if (_bestError == value) return;
                _bestError = value;
                OnPropertyChanged();
            }
        }

        public System.Windows.Input.ICommand SettingsClickCommand { get; set; }
        public System.Windows.Input.ICommand StartClickCommand { get; set; }
        public System.Windows.Input.ICommand CloseWindowClickCommand { get; set; }
        public RelayCommand<DialogClosingEventArgs> CloseDialogHostCommand { get; set; }
    }
}
