using System;
using System.ComponentModel;

namespace PortHelper.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        #region Fields

        private int _selectedIndex;

        #endregion Fields

        #region Constructors

        public MainViewModel()
        {
            TcpServer = new TcpServerViewModel();
            UdpServer = new UdpServerViewModel();
            SelectedIndex = 0;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public IBaseProtocolViewModel CurrentViewModel { get; set; }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                CurrentViewModel = _selectedIndex switch
                {
                    0 => TcpServer,
                    1 => UdpServer,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }

        public TcpServerViewModel TcpServer { get; }

        public UdpServerViewModel UdpServer { get; }

        #endregion Properties
    }
}