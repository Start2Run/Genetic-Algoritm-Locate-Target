using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using HelixToolkit.Wpf;
using Viewer3D.Annotations;

namespace Viewer3D.ViewModels
{
    public partial class MainWindowViewModel:INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private Dispatcher _dispatcher;
        private ConcurrentDictionary<Guid, TubeVisual3D> _lines;
        private CancellationTokenSource _cts;
        private HelixViewport3D _view;
        private bool _isSettingsBtnEnabled = true;
        private bool _isStartBtnEnabled = true;


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

        public System.Windows.Input.ICommand SettingsClickCommand { get; set; }
        public System.Windows.Input.ICommand StartClickCommand { get; set; }
        public System.Windows.Input.ICommand CloseWindowClickCommand { get; set; }
    }
}
