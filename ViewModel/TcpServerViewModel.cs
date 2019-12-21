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
    public sealed class TcpServerViewModel : IBaseProtocolViewModel
    {
        #region Fields

        private bool _connected;

        private bool _isTextMode;

        private int? _localPort;

        private int? _maxCount;

        private TcpClientViewModel _remoteClient;

        private byte[] _sendBytes;

        private string _sendMessage;

        #endregion Fields

        #region Constructors

        public TcpServerViewModel()
        {
            IsTextMode = true;
            RemoteClients = new ObservableCollection<IClientViewModel>();
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

        public int? MaxCount
        {
            get => _maxCount;
            set
            {
                if (_maxCount == value) return;
                _maxCount = value;
                OnPropertyChanged();
            }
        }

        public string OpenButtonText => Connected ? "Close" : "Open";

        public ObservableCollection<LogViewModel> ReceiveLogs { get; } =
            new ObservableCollection<LogViewModel>();

        public TcpClientViewModel RemoteClient
        {
            get => _remoteClient;
            set
            {
                if (_remoteClient == value) return;
                _remoteClient = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<IClientViewModel> RemoteClients { get; }

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
                    Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                        ProtocolType.Tcp);
                    Server.Bind(new IPEndPoint(IPAddress.Any, LocalPort ?? 0));
                    LocalPort = ((IPEndPoint)Server.LocalEndPoint).Port;
                    ReceiveLogs.Add(new LogViewModel
                    {
                        IsSystemLog = true,
                        Text = $"** Start Listening Port: {LocalPort} **"
                    });
                    MaxCount ??= 10;
                    Server.Listen(MaxCount.Value);
                    Connected = true;
                    while (Connected)
                    {
                        var socket = await Server.AcceptAsync();
                        var client = new TcpClientViewModel(socket);
                        RemoteClients.Add(client);
                        var builtLog = new LogViewModel
                        {
                            IsSystemLog = true,
                            Text = "** Connection Built. **",
                            Source = client.Name
                        };
                        ReceiveLogs.Add(builtLog);
                        _ = ReceiveAsync(client);
                    }
                }

                Server.Close();
            }
            catch (Exception)
            {
                Connected = false;
                ReceiveLogs.Add(new LogViewModel
                {
                    IsSystemLog = true,
                    Text = "** Stop Listening **"
                });
            }
        }

        public async Task ReceiveAsync(TcpClientViewModel client)
        {
            while (true)
            {
                var stream = new NetworkStream(client.Entity);
                var readBytes = new byte[1024];
                try
                {
                    if (client.Entity.Poll(100, SelectMode.SelectRead)) throw new SocketException();
                    var readCount = await stream.ReadAsync(readBytes, 0, readBytes.Length);
                    if (readCount > 0)
                    {
                        var readString = Encoding.UTF8.GetString(readBytes, 0, readCount);
                        var receiveLog = new LogViewModel
                        {
                            IsTextMode = true,
                            Text = readString,
                            Source = client.Name
                        };
                        receiveLog.IsTextMode = IsTextMode;
                        ReceiveLogs.Add(receiveLog);
                    }
                }
                catch (Exception)
                {
                    RemoteClients.Remove(client);
                    var closeLog = new LogViewModel
                    {
                        IsSystemLog = true,
                        Text = "** Connection Closed. **",
                        Source = client.Name
                    };
                    ReceiveLogs.Add(closeLog);
                    return;
                }
            }
        }

        public async Task Send()
        {
            if (RemoteClient == null) return;
            var stream = new NetworkStream(RemoteClient.Entity);
            await stream.WriteAsync(_sendBytes, 0, _sendBytes.Length);
            var sendLog = new LogViewModel
            {
                IsTextMode = IsTextMode,
                Text = SendMessage,
                Source = RemoteClient.Name
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