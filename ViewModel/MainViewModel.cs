using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PortHelper.ViewModel
{
    public sealed class MainViewModel : INotifyPropertyChanged
    {
        private int _selectedIndex;

        public MainViewModel()
        {
            TcpServer = new TcpServerViewModel();
            UdpServer = new UdpServerViewModel();
            SelectedIndex = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

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

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}