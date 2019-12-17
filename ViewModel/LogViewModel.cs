using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PortHelper.ViewModel
{
    public sealed class LogViewModel : INotifyPropertyChanged
    {
        private byte[] _content;

        private bool _isTextMode;

        private string _text;
        private DateTime _time;

        public event PropertyChangedEventHandler PropertyChanged;

        public byte[] Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged();
            }
        }

        public bool IsSystemLog { get; set; }

        public bool IsTextMode
        {
            get => _isTextMode;
            set
            {
                _isTextMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Text));
            }
        }

        public string Text
        {
            get
            {
                if (_isTextMode || IsSystemLog) return Encoding.UTF8.GetString(Content);

                var hexString = BitConverter.ToString(Content);
                return hexString.Replace('-', ' ');
            }
            set
            {
                _text = value;
                if (_isTextMode || IsSystemLog)
                {
                    Content = Encoding.UTF8.GetBytes(value);
                }
                else
                {
                    _text = _text.Replace(" ", "");
                    Content = new byte[_text.Length / 2];
                    for (var i = 0; i < _content.Length; i++)
                        _content[i] = Convert.ToByte(_text.Substring(i * 2, 2), 16);

                    _text = Encoding.UTF8.GetString(Content);
                }

                OnPropertyChanged();
            }
        }

        public DateTime Time
        {
            get => _time;
            set
            {
                _time = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}