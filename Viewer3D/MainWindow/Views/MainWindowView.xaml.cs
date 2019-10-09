using Viewer3D.ViewModels;

namespace Viewer3D.MainWindow.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindowView
    {
        private MainWindowViewModel _viewModel;

        public MainWindowView()
        {
            _viewModel = new MainWindowViewModel();
            DataContext = _viewModel;
            InitializeComponent();
            _viewModel.InitViewer(MainView);
        }
    }
}
