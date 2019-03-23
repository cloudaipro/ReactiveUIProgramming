using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ReactiveDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ReactiveWindow<AppViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new AppViewModel();

            this.WhenActivated(disposableRegistration =>
            {
                this.OneWayBind(ViewModel,
                                viewModel => viewModel.IsAvailable,
                                view => view.searchResultsListBox.Visibility)
                                .DisposeWith(disposableRegistration);
                this.OneWayBind(ViewModel,
                                viewModel => viewModel.SearchReaults,
                                view => view.searchResultsListBox.ItemsSource)
                                .DisposeWith(disposableRegistration);
                this.Bind(ViewModel,
                          viewModel => viewModel.SearchTerm,
                          view => view.searchTextBox.Text)
                          .DisposeWith(disposableRegistration);
            });
        }
    }
}
