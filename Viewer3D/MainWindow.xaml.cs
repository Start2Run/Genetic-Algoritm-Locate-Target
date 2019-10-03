using Viewer3D.ViewModels;

namespace Viewer3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private MainWindowViewModel _viewModel;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            _viewModel.InitViewer(MainView);
        }
    }
}
