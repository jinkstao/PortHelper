using System;
using PortHelper.ViewModel;
using System.Windows.Input;

namespace PortHelper.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }

        public MainViewModel ViewModel { get; set; }

        private void SendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel?.CurrentViewModel switch
            {
                TcpServerViewModel tcpViewModel => tcpViewModel.Connected && tcpViewModel.RemoteClient != null,
                UdpServerViewModel udpViewModel => udpViewModel.Connected && !string.IsNullOrEmpty(udpViewModel.RemoteIP) && udpViewModel.RemotePort != null,
                _ => false
            };
        }

        private void SendMessage_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.Send();
        }

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.OpenAsync();
        }

        private void EventSetter_OnHandler(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}