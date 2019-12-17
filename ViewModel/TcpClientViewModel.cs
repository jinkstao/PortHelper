using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PortHelper.ViewModel
{
    public class TcpClientViewModel : IClientViewModel<Socket>
    {
        public string Name
        {
            get
            {
                var ipEndPoint = (Entity.RemoteEndPoint as IPEndPoint);
                return $"{ipEndPoint?.Address}:{ipEndPoint?.Port}";
            }
        }
        public Socket Entity { get; }

        public TcpClientViewModel(Socket socket)
        {
            Entity = socket;
        }
    }
}
