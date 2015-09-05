using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using Skype.ViewModel;
using System.Collections.ObjectModel;
using Skype.Model;
using System.Threading.Tasks;
using Client;
using System.Threading;
using System.Diagnostics;


namespace Skype.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWinViewModel _mainWindowViewModel = new MainWinViewModel();

        public MainWindow()
        {
            InitializeComponent();
            this.contactTabItem.DataContext = _mainWindowViewModel.ContactSectionViewModel;
            this.chatGrid.DataContext = _mainWindowViewModel.ContactSectionViewModel;
            this.contactHeaderSP.DataContext = _mainWindowViewModel.ContactSectionViewModel;
            this.searchTabItem.DataContext = _mainWindowViewModel.SearchSectionModel;
            this.recentTabItem.DataContext = _mainWindowViewModel.RecentSectionViewModel;
            this.mainMenu.DataContext = _mainWindowViewModel.MainWinMenuViewModel;
            this.profileGrid.DataContext = _mainWindowViewModel.ProfileSectionViewModel;
            this.DataContext = _mainWindowViewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            App.Authenticate();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AsynchronousClientSocket.Shutdown();
        }
    }
}
