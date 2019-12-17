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
        private bool _connected;

        private bool _isTextMode;

        private byte[] _sendBytes;

        private string _sendMessage;

        private int? _localPort;

        private TcpClientViewModel _remoteClient;

        public TcpServerViewModel()
        {
            IsTextMode = true;
            RemoteClients = new ObservableCollection<IClientViewModel>();
        }

        public ObservableCollection<IClientViewModel> RemoteClients { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;

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

        public string OpenButtonText => Connected ? "Close" : "Open";

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

        public ObservableCollection<LogViewModel> ReceiveLogs { get; } =
            new ObservableCollection<LogViewModel>();

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

        public async Task Receive(TcpClientViewModel client)
        {
            while (true)
            {
                var stream = new NetworkStream(client.Entity);
                var readBytes = new byte[1024];
                try
                {
                    var readCount = await stream.ReadAsync(readBytes, 0, readBytes.Length);
                    if (readCount > 0)
                    {
                        var readString = Encoding.UTF8.GetString(readBytes, 0, readCount);
                        var receiveLog = new LogViewModel
                        {
                            IsTextMode = true,
                            Time = DateTime.Now,
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
                        Time = DateTime.Now,
                        Text = "** Connection Closed. **",
                        Source = client.Name
                    };
                    ReceiveLogs.Add(closeLog);
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
                Time = DateTime.Now,
                Text = SendMessage,
                Source = RemoteClient.Name
            };
            SendLogs.Add(sendLog);
        }

        public async Task OpenAsync()
        {
            try
            {
                if (!Connected)
                {
                    Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                        ProtocolType.Tcp);
                    Server.Bind(new IPEndPoint(IPAddress.Any, LocalPort ?? 0));
                    LocalPort = ((IPEndPoint) Server.LocalEndPoint).Port;
                    ReceiveLogs.Add(new LogViewModel
                    {
                        IsSystemLog = true,
                        Time = DateTime.Now,
                        Text = $"** Start Listening Port: {LocalPort} **"
                    });
                    Server.Listen(10);
                    Connected = true;
                    while (Connected)
                    {
                        var socket = await Server.AcceptAsync();
                        var client = new TcpClientViewModel(socket);
                        RemoteClients.Add(client);
                        _ = Receive(client);
                        var builtLog = new LogViewModel
                        {
                            IsSystemLog = true,
                            Time = DateTime.Now,
                            Text = "** Connection Built. **",
                            Source = client.Name
                        };
                        ReceiveLogs.Add(builtLog);
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
                    Time = DateTime.Now,
                    Text = "** Stop Listening **",
                });
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}