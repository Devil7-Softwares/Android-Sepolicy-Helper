using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Devil7.Android.SepolicyHelper.Views
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: Remove This. Temprorary workaroud as Binding of SelectedItem in data grid is not working.
            ViewModels.MainWindowViewModel viewModel = this.DataContext as ViewModels.MainWindowViewModel;
            DataGrid dataGrid = sender as DataGrid;
            if (viewModel != null && dataGrid != null && viewModel.SelectedSepolicy != dataGrid.SelectedItem)
                viewModel.SelectedSepolicy = dataGrid.SelectedItem as Models.SepolicyInfo;
        }
    }
}
