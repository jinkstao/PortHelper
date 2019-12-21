using System.Windows;
using System.Windows.Controls;
using PortHelper.ViewModel;
using System.Windows.Input;

namespace PortHelper.View
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new MainViewModel();
            DataContext = ViewModel;
        }

        #endregion Constructors

        #region Properties

        public MainViewModel ViewModel { get; set; }

        #endregion Properties

        #region Methods

        private void Open_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.OpenAsync();
        }

        private void SendMessage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ViewModel?.CurrentViewModel switch
            {
                TcpServerViewModel tcpViewModel => tcpViewModel.Connected && tcpViewModel.RemoteClient != null,
                UdpServerViewModel udpViewModel => !string.IsNullOrEmpty(udpViewModel.RemoteIP) && udpViewModel.RemotePort != null,
                _ => false
            };
        }

        private void SendMessage_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.CurrentViewModel.Send();
        }

        #endregion Methods

        private void Copy_Execute(object sender, ExecutedRoutedEventArgs e)
        {
            Clipboard.SetDataObject(e.Parameter);
        }
    }
}