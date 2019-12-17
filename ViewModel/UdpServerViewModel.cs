using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace PortHelper.ViewModel
{
    public sealed class UdpServerViewModel : IBaseProtocolViewModel
    {
        #region Fields

        private bool _connected;

        private bool _isTextMode;
        private int? _localPort;

        private string _remoteIP;

        private int? _remotePort;

        private byte[] _sendBytes;

        private string _sendMessage;

        #endregion Fields

        #region Constructors

        public UdpServerViewModel()
        {
            IsTextMode = true;
        }

        #endregion Constructors

        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Events

        #region Properties

        public bool Connected
        {
            get => _connected;
            set
            {
                _connected = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(OpenButtonText));
            }
        }

        public bool HeartbeatFeedback { get; set; }

        public bool IsTextMode
        {
            get => _isTextMode;
            set
            {
                if (_isTextMode != value)
                {
                    _isTextMode = value;
                    foreach (var receiveLog in ReceiveLogs) receiveLog.IsTextMode = _isTextMode;
                    foreach (var sendLog in SendLogs) sendLog.IsTextMode = _isTextMode;

                    if (!string.IsNullOrEmpty(SendMessage))
                    {
                        if (!_isTextMode)
                        {
                            _sendBytes = Encoding.UTF8.GetBytes(SendMessage);
                            var hexString = BitConverter.ToString(_sendBytes);
                            _sendMessage = hexString.Replace('-', ' ');
                        }
                        else
                        {
                            var hex = SendMessage.Replace(" ", "");
                            _sendBytes = new byte[hex.Length / 2];
                            for (var i = 0; i < _sendBytes.Length; i++)
                                _sendBytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);

                            _sendMessage = Encoding.UTF8.GetString(_sendBytes);
                        }

                        OnPropertyChanged(nameof(SendMessage));
                    }

                    OnPropertyChanged();
                }
            }
        }

        public int? LocalPort
        {
            get => _localPort;
            set
            {
                if (_localPort == value) return;
                _localPort = value;
                OnPropertyChanged();
            }
        }

        public string OpenButtonText => Connected ? "Close" : "Open";

        public ObservableCollection<LogViewModel> ReceiveLogs { get; } =
            new ObservableCollection<LogViewModel>();

        public string RemoteIP
        {
            get => _remoteIP;
            set
            {
                if (_remoteIP == value) return;
                _remoteIP = value;
                OnPropertyChanged();
            }
        }

        public int? RemotePort
        {
            get => _remotePort;
            set
            {
                if (_remotePort == value) return;
                _remotePort = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LogViewModel> SendLogs { get; } =
            new ObservableCollection<LogViewModel>();

        public string SendMessage
        {
            get => _sendMessage;
            set
            {
                _sendMessage = value;
                if (!_isTextMode)
                {
                    var hex = _sendMessage.Replace(" ", "");
                    _sendBytes = new byte[hex.Length / 2];
                    for (var i = 0; i < _sendBytes.Length; i++)
                        _sendBytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                    var textString = Encoding.UTF8.GetString(_sendBytes);
                    var bytes = Encoding.UTF8.GetBytes(textString);
                    var hexString = BitConverter.ToString(bytes);
                    _sendMessage = hexString.Replace('-', ' ');
                }
                else
                {
                    _sendBytes = Encoding.UTF8.GetBytes(SendMessage);
                }

                OnPropertyChanged();
            }
        }

        public Socket Server { get; set; }

        #endregion Properties

        #region Methods

        public async Task OpenAsync()
        {
            try
            {
                if (!Connected)
                {
                    Server = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                        ProtocolType.Udp);
                    Server.Bind(new IPEndPoint(IPAddress.Any, LocalPort ?? 0));
                    LocalPort = ((IPEndPoint)Server.LocalEndPoint).Port;
                    ReceiveLogs.Add(new LogViewModel
                    {
                        IsSystemLog = true,
                        Time = DateTime.Now,
                        Text = $"** Start Listening Port: {LocalPort} **"
                    });
                    Connected = true;
                    while (Connected) await Receive();
                }

                Server.Close();
            }
            catch (Exception)
            {
                Connected = false;
                ReceiveLogs.Add(new LogViewModel
                {
                    IsSystemLog = true,
                    Time = DateTime.Now,
                    Text = "** Stop Listening **"
                });
            }
        }

        public async Task Receive()
        {
            EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            var buffer = new byte[1024];
            var length = await Task.Run(() => Server.ReceiveFrom(buffer, ref remoteEndPoint));
            //int length = Server.ReceiveFrom(buffer, ref RemoteEndPoint);
            if (length == 0 && HeartbeatFeedback)
            {
                Server.SendTo(new byte[0], remoteEndPoint);
            }
            else
            {
                var readString = Encoding.UTF8.GetString(buffer, 0, length);
                var remoteIPEndPoint = (IPEndPoint)remoteEndPoint;
                var receiveLog = new LogViewModel
                {
                    IsTextMode = true,
                    Time = DateTime.Now,
                    Text = readString,
                    Source = $"{remoteIPEndPoint.Address}:{remoteIPEndPoint.Port}"
                };
                receiveLog.IsTextMode = IsTextMode;
                ReceiveLogs.Add(receiveLog);
            }
        }

        public async Task Send()
        {
            var remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(RemoteIP), RemotePort.Value);
            Server.SendTo(_sendBytes, remoteIPEndPoint);
            var sendLog = new LogViewModel
            {
                IsTextMode = IsTextMode,
                Time = DateTime.Now,
                Text = SendMessage,
                Source = $"{remoteIPEndPoint.Address}:{remoteIPEndPoint.Port}"
            };
            SendLogs.Add(sendLog);
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}