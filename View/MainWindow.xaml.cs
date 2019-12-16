using System.Windows.Input;
using PortHelper.ViewModel;

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

        private void StartListening_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.StartListeningAsync();
        }

        private void SendMessage_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.Send();
        }

        private void SendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel?.CurrentViewModel switch
            {
                TcpServerViewModel tcpViewModel => (tcpViewModel.Connected &&
                                                    tcpViewModel.Client != null),
                UdpServerViewModel udpViewModel => (udpViewModel.Connected &&
                                                    udpViewModel.RemoteEndPoint != null),
                _ => e.CanExecute
            };
        }
    }
}