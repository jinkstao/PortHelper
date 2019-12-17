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

        public TcpServerViewModel()
        {
            IsTextMode = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Socket Client { get; set; }

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
        public int Port { get; set; }

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

        public async Task Receive()
        {
            while (true)
            {
                var stream = new NetworkStream(Client);
                var readBytes = new byte[1024];
                var readCount = await stream.ReadAsync(readBytes, 0, readBytes.Length);
                if (readCount > 0)
                {
                    var readString = Encoding.UTF8.GetString(readBytes, 0, readCount);
                    var receiveLog = new LogViewModel
                    {
                        IsTextMode = true,
                        Time = DateTime.Now,
                        Text = readString
                    };
                    receiveLog.IsTextMode = IsTextMode;
                    ReceiveLogs.Add(receiveLog);
                }
            }
        }

        public async Task Send()
        {
            var stream = new NetworkStream(Client);
            await stream.WriteAsync(_sendBytes, 0, _sendBytes.Length);
            var sendLog = new LogViewModel
            {
                IsTextMode = IsTextMode,
                Time = DateTime.Now,
                Text = SendMessage
            };
            SendLogs.Add(sendLog);
        }

        public async Task StartListeningAsync()
        {
            try
            {
                if (!Connected)
                {
                    Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream,
                        ProtocolType.Tcp);
                    Server.Bind(new IPEndPoint(IPAddress.Any, Port));
                    ReceiveLogs.Add(new LogViewModel
                    {
                        IsSystemLog = true,
                        Time = DateTime.Now,
                        Text = $"** Start Listening Port: {((IPEndPoint)Server.LocalEndPoint).Port} **"
                    });
                    Server.Listen(10);
                    Connected = true;
                    while (Connected)
                    {
                        Client = await Server.AcceptAsync();
                        _ = Receive();
                    }
                }

                Server.Close();
                Connected = false;
                ReceiveLogs.Add(new LogViewModel
                {
                    IsSystemLog = true,
                    Time = DateTime.Now,
                    Text = "** Stop Listening **"
                });
            }
            catch (Exception e)
            {
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}